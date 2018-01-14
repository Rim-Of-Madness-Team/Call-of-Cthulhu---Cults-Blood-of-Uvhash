using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace CultOfUvhash
{
    public class CompBloodTank : ThingComp
    {
        public CompBloodTank connectParent;

        public List<CompBloodTank> connectChildren;

        private float storedBlood;
        
        public float AmountCanAccept
        {
            get
            {
                if (this.parent.IsBrokenDown())
                {
                    return 0f;
                }
                CompProperties_BloodTank props = this.Props;
                return (props.storedBloodMax - this.storedBlood) / props.efficiency;
            }
        }

        public float StoredBlood
        {
            get
            {
                return this.storedBlood;
            }
        }

        public CompProperties_BloodTank Props
        {
            get
            {
                return (CompProperties_BloodTank)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.storedBlood, "storedBlood", 0f, false);
            CompProperties_BloodTank props = this.Props;
            if (this.storedBlood > props.storedBloodMax)
            {
                this.storedBlood = props.storedBloodMax;
            }
            Thing thing = null;
            if (Scribe.mode == LoadSaveMode.Saving && this.connectParent != null)
            {
                thing = this.connectParent.parent;
            }
            Scribe_References.Look<Thing>(ref thing, "parentThing", false);
            if (thing != null)
            {
                this.connectParent = ((ThingWithComps)thing).GetComp<CompBloodTank>();
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.connectParent != null)
            {
                //this.ConnectToTransmitter(this.connectParent, true);
            }
        }

        public void AddBlood(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot add negative blood " + amount);
                return;
            }
            if (amount > this.AmountCanAccept)
            {
                amount = this.AmountCanAccept;
                if (Find.TickManager.TicksGame % 250 == 0) Messages.Message("BloodTankFull".Translate(), new RimWorld.Planet.GlobalTargetInfo(this.parent), MessageTypeDefOf.RejectInput);
                FilthMaker.MakeFilth(this.parent.PositionHeld.RandomAdjacentCell8Way(), this.parent.MapHeld, ThingDefOf.FilthBlood, 1);
            }
            amount *= this.Props.efficiency;
            this.storedBlood += amount;
        }

        public void DrawBlood(float amount)
        {
            this.storedBlood -= amount;
            if (this.storedBlood < 0f)
            {
                Log.Error("Drawing blood we don't have from " + this.parent);
                this.storedBlood = 0f;
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "Breakdown")
            {
                this.DrawBlood(this.StoredBlood);
            }
        }

        public override string CompInspectStringExtra()
        {
            CompProperties_BloodTank props = this.Props;
            StringBuilder s = new StringBuilder();
            s.AppendLine(base.CompInspectStringExtra());
            s.AppendLine("BloodTankStored".Translate() + ": " + this.storedBlood.ToString("F") + " / " + props.storedBloodMax.ToString("F") + " " + "BloodTankLitres".Translate());
            s.AppendLine("BloodTankEfficiency".Translate() + ": " + (props.efficiency * 100f).ToString("F0") + "%");
            return s.ToString().TrimEndNewlines();
        }

        public bool TransmitsBloodNow
        {
            get
            {
                if (StoredBlood > 5.5f) return true;
                return false;
            }
        }
        
        public virtual void ResetBloodVars()
        {
            this.connectParent = null;
            this.connectChildren = null;
        }

        public void SetUpConnections()
        {
            Map map = this.parent.MapHeld;
            if (map != null)
            {
                if (this.parent != null)
                {
                    //Hubs need generators
                    if (this.parent is Building_BloodHub)
                    {
                        IEnumerable<Building_BloodCollector> bloodTanks = map.listerBuildings.AllBuildingsColonistOfClass<Building_BloodCollector>();
                        if (bloodTanks !=  null)
                        {
                            foreach (Building_BloodCollector current in bloodTanks)
                            {
                               if (current.TryGetComp<CompBloodTank>() != null)
                                {
                                    connectChildren.Add(current.TryGetComp<CompBloodTank>());
                                }
                            }
                        }
                    }

                    //Tanks / Blood users need hubs
                    else {
                        connectParent = FindNearestHub().TryGetComp<CompBloodTank>();
                    }
                }

            }
        }

        public Building_BloodHub FindNearestHub()
        {
            Building_BloodHub parentCandidate = null;

            Map map = this.parent.MapHeld;
            if (map != null)
            {
                IEnumerable<Building_BloodHub> bloodHubs = map.listerBuildings.AllBuildingsColonistOfClass<Building_BloodHub>();
                if (bloodHubs != null)
                {
                    if (this.parent.Position != null)
                    {
                        float num = -1;
                        foreach (Building_BloodHub hub in bloodHubs)
                        {
                            if (hub.Position != null)
                            {
                                float num2 = hub.Position.DistanceToSquared(this.parent.Position);
                                if (num2 < num || parentCandidate == null)
                                {
                                    parentCandidate = hub;
                                    num = num2;
                                }
                            }
                        }
                    }
                }
            }
            return parentCandidate;
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.Props.transmitsBlood)
            {
                this.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid, true, false);
                if (this.Props.transmitsBlood)
                {
                    //this.parent.Map.powerNetManager.Notify_TransmitterSpawned(this);
                    //this.parent.Map.powerNetManager.Notify_ConnectorWantsConnect(this);
                }
                this.SetUpConnections();
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (this.Props.transmitsBlood)
            {
                if (this.Props.transmitsBlood)
                {
                    if (this.connectChildren != null)
                    {
                        for (int i = 0; i < this.connectChildren.Count; i++)
                        {
                            this.connectChildren[i].LostConnectParent();
                        }
                    }
                    //map.powerNetManager.Notify_TransmitterDespawned(this);
                    //map.powerNetManager.Notify_ConnectorDespawned(this);
                }
                map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid, true, false);
            }
        }

        public virtual void LostConnectParent()
        {
            this.connectParent = null;
            //this.parent.Map.powerNetManager.Notify_ConnectorWantsConnect(this);
        }

        public virtual void LostConnectChild(CompBloodTank tank)
        {
            if (tank != null)
            {
                if (this.connectChildren != null && this.connectChildren.Count > 0)
                {
                    if (this.connectChildren.Contains(tank)) this.connectChildren.Remove(tank);
                }
            }
        }

        public void ExtractBlood()
        {
            int bloodLitres = (int)this.storedBlood;
            if (bloodLitres > 0)
            {
                this.storedBlood -= bloodLitres;
                Cthulhu.Utility.SpawnThingDefOfCountAt(ThingDef.Named("Uvhash_Blood"), bloodLitres, new TargetInfo(this.parent));
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
            
                int bloodLitres = (int)this.StoredBlood;
                if (bloodLitres > 0)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.action = new Action(this.ExtractBlood);
                    command_Action.defaultLabel = "BloodCommandExtract".Translate();
                    command_Action.defaultDesc = "BloodCommandExtractDesc".Translate(new object[]
                        {
                            bloodLitres.ToString()
                        });
                    command_Action.hotKey = KeyBindingDefOf.Misc1;
                    command_Action.icon = ContentFinder<Texture2D>.Get(ThingDef.Named("Uvhash_Blood").graphicData.texPath, true);
                    yield return command_Action;
                }
            
        }

    }



}
