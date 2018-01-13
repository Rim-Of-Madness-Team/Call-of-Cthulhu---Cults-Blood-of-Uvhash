using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CultOfUvhash
{
    public enum PortalStatus : Int32
    {
        Closed = 0,
        Closing = 1,
        Opening = 2,
        Open = 3,
        Exploring = 4,
        Contacted = 5,
        Freed = 6
    }

    public class Building_PortalBeyond : Building, IThingHolder
    {
        private static readonly IntRange maxExploredTicks = new IntRange(GenDate.TicksPerDay, GenDate.TicksPerDay * 3);
        private PortalStatus state = PortalStatus.Closed;
        private bool workerNeeded = false;
        private int ticksUntilExplored = 0;
        protected ThingOwner innerContainer;


        public bool HasAnyContents => this.innerContainer.Count > 0;
        public Thing ContainedThing => (this.innerContainer.Count != 0) ? this.innerContainer[0] : null;
        public bool CanOpen => this.HasAnyContents;


        public void Notify_Opened(Pawn actor)
        {
            workerNeeded = false;
            state = PortalStatus.Open;
            Messages.Message("ROM_PortalOpenMess".Translate(actor.LabelShort), MessageSound.Standard);
        }

        public void Notify_Closed(Pawn actor)
        {
            workerNeeded = false;
            state = PortalStatus.Closed;
            Messages.Message("ROM_PortalCloseMess".Translate(actor.LabelShort), MessageSound.Standard);
        }

        public void Notify_Exploring(Pawn actor)
        {
            actor.DeSpawn();
            innerContainer.TryAdd(actor);
            ticksUntilExplored = maxExploredTicks.RandomInRange;
        }

        public void ExplorationResult()
        {
            state = PortalStatus.Contacted;
            ticksUntilExplored = 0;
            innerContainer.TryDropAll(this.PositionHeld, this.MapHeld, ThingPlaceMode.Near);
            Messages.Message("ROM_PortalExplored".Translate(), MessageSound.Standard);
        }

        public override void Tick()
        {
            base.Tick();
            this.innerContainer.ThingOwnerTick(true);
            if (state == PortalStatus.Exploring)
            {
                if (ticksUntilExplored > 0)
                    ticksUntilExplored--;
                else
                    ExplorationResult();
            }
                
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            if (state <= PortalStatus.Open)
                yield return new Command_Toggle
                {
                    defaultLabel = "ROM_PortalOpen".Translate(),
                    defaultDesc = "ROM_PortalOpenDesc".Translate(),
                    isActive = () => workerNeeded,
                    toggleAction = delegate
                    {
                        workerNeeded = !workerNeeded;
                        state = (workerNeeded) ? PortalStatus.Opening : PortalStatus.Closed;
                    }
                };
            if (state >= PortalStatus.Closing && state < PortalStatus.Contacted)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "ROM_PortalClose".Translate(),
                    defaultDesc = "ROM_PortalCloseDesc".Translate(),
                    isActive = () => workerNeeded,
                    toggleAction = delegate
                    {
                        workerNeeded = !workerNeeded;
                        state = (workerNeeded) ? PortalStatus.Closing : PortalStatus.Open;
                    }
                };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<PortalStatus>(ref this.state, "state", PortalStatus.Closed);
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
        }
        
        public Building_PortalBeyond()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public override void TickRare()
        {
            base.TickRare();
            this.innerContainer.ThingOwnerTickRare(true);
        }



        public virtual void Open()
        {
            if (!this.HasAnyContents)
            {
                return;
            }
            this.EjectContents();
        }
        
        public override bool ClaimableBy(Faction fac)
        {
            if (this.innerContainer.Any)
            {
                for (int i = 0; i < this.innerContainer.Count; i++)
                {
                    if (this.innerContainer[i].Faction == fac)
                    {
                        return true;
                    }
                }
                return false;
            }
            return base.ClaimableBy(fac);
        }

        public virtual bool Accepts(Thing thing)
        {
            return this.innerContainer.CanAcceptAnyOf(thing, true);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
                return true;
            }
            else if (this.innerContainer.TryAdd(thing, true))
            {
                return true;
            }
            return false;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                if (mode != DestroyMode.Deconstruct)
                {
                    List<Pawn> list = new List<Pawn>();
                    foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
                    {
                        Pawn pawn = current as Pawn;
                        if (pawn != null)
                        {
                            list.Add(pawn);
                        }
                    }
                    foreach (Pawn current2 in list)
                    {
                        HealthUtility.DamageUntilDowned(current2);
                    }
                }
                this.EjectContents();
            }
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
        }

        public virtual void EjectContents()
        {
            this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near);
        }

        public override string GetInspectString()
        {
            return base.GetInspectString();
            //string text = base.GetInspectString();
            //string str;
            //if (!this.contentsKnown)
            //{
            //    str = "UnknownLower".Translate();
            //}
            //else
            //{
            //    str = this.innerContainer.ContentsString;
            //}
            //if (!text.NullOrEmpty())
            //{
            //    text += "\n";
            //}
            //return text + "CasketContains".Translate() + ": " + str;
        }

    }
}
