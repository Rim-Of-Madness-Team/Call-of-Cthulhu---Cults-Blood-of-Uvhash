using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;
using Verse.Sound;
using RimWorld;
using System.Reflection;

namespace CultOfUvhash
{
    public class Building_BloodFactory : Building_Casket, IThingContainerOwner
    {
        private bool isLoading = false;

        private int rareTicks = 250;
        private float bloodLoaded = 0.0f;
        private static readonly float bloodLoadRate = 0.1f;
        private static readonly float bloodTurnoverPoint = 5.5f;
        private static readonly float bloodLossFatalVolume = 2.5f;
        private static HashSet<IntVec3> reachableCells = new HashSet<IntVec3>();
        public Pawn lastLoader = null;
        
        public Building_BloodCollector nearestBloodTank = null;
        public Building_BloodCollector NearestBloodTank
        {
            get
            {
                if (nearestBloodTank == null || nearestBloodTank.Destroyed || !nearestBloodTank.Spawned)
                {
                    Map map = this.MapHeld;
                    if (map != null)
                    {
                        IEnumerable<Building_BloodCollector> bloodTanks = map.listerBuildings.AllBuildingsColonistOfClass<Building_BloodCollector>();
                        if (bloodTanks != null)
                        {
                            if (this.Position != null)
                            {
                                float num = -1;
                                Building_BloodCollector candidate = null;
                                foreach (Building_BloodCollector tank in bloodTanks)
                                {
                                    if (tank.Position != null)
                                    {
                                        float num2 = tank.Position.DistanceToSquared(this.Position);
                                        if (num2 < num || candidate == null)
                                        {
                                            candidate = tank;
                                            num = num2;
                                        }
                                    }
                                }
                                nearestBloodTank = candidate;
                            }
                        }
                    }
                }

                return nearestBloodTank;
            }
        }

        public bool IsLoaded
        {
            get
            {
                if (Occupant == null) return false;
                return true;
            }
        }
        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;
            }
        }

        public Building_BloodFactory()
        {
            //this.container = new ThingContainer(this, false, LookMode.Deep);
            this.rareTicks = 250;
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
        }

        public Thing Occupant
        {
            get
            {
                Thing occupant = null;
                if (GetInnerContainer().Count != 0)
                {
                    IntVec3 intVec = this.RandomAdjacentCell8Way();
                    foreach (Thing t in GetInnerContainer())
                    {
                        occupant = t;
                    }

                }
                return occupant;
            }
        }

            public void ProcessPrisonerButton()
        {
            if (!isLoading)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                Map map = this.Map;
                List<Pawn> prisoners = map.mapPawns.PrisonersOfColonySpawned;
                if (prisoners.Count != 0)
                {
                    foreach (Pawn current in map.mapPawns.PrisonersOfColonySpawned)
                    {
                        if (!current.Dead)
                        {
                            string text = current.Name.ToStringFull;
                            List<FloatMenuOption> arg_121_0 = list;
                            Func<Rect, bool> extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, current);
                            arg_121_0.Add(new FloatMenuOption(text, delegate
                            {
                                this.TryLoadPrisoner(current);
                            }, MenuOptionPriority.Default, null, null, 29f, extraPartOnGUI, null));
                        }
                    }
                }
                else
                {
                    list.Add(new FloatMenuOption("BloodNoPrisoners".Translate(), delegate
                    {
                    }, MenuOptionPriority.Default));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            else
            {
                TryCancelLoadingPrisoner();
            }
        }

        private void TryCancelLoadingPrisoner(string reason = "")
        {
            //Pawn pawn = null;
            //List<Pawn> listeners = Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            //bool[] flag = new bool[listeners.Count];
            //for (int i = 0; i < listeners.Count; i++)
            //{
            //    pawn = listeners[i];
            if (lastLoader != null)
            {
                if (lastLoader.Faction == Faction.OfPlayer)
                {
                    if (lastLoader.CurJob.def.defName == "BloodHaulPrisoner")
                    {
                        lastLoader.jobs.StopAll();
                    }
                }
            }
                
            //}
            isLoading = false;
            Messages.Message("BloodBodyLoadable_Cancel".Translate() + " " + reason, MessageSound.Negative);
        }
        private void StartLoadingPrisoner(Pawn executioner, Pawn sacrifice)
        {
            if (this.Destroyed || !this.Spawned)
            {
                TryCancelLoadingPrisoner("BloodBodyLoadable_MissingFactory".Translate());
                return;
            }
            if (!Cthulhu.Utility.IsActorAvailable(executioner))
            {
                TryCancelLoadingPrisoner("BloodBodyLoadable_ActorUnavailable".Translate());
                return;
            }
            if (!Cthulhu.Utility.IsActorAvailable(sacrifice, true))
            {
                TryCancelLoadingPrisoner("BloodBodyLoadable_BodyUnavailable".Translate(new object[]
                    {
                        sacrifice.LabelShort
                    }));
                return;
            }

            Messages.Message("BloodBodyLoadable_GatheringBody".Translate(new object[]
                {
                    executioner.LabelShort,
                    sacrifice.LabelShort
                }), TargetInfo.Invalid, MessageSound.Standard);
            isLoading = true;

            Cthulhu.Utility.DebugReport("Force load called");
            Job job = new Job(DefDatabase<JobDef>.GetNamed("BloodHaulPrisoner"), sacrifice, this);
            job.count = 1;
            executioner.QueueJob(job);
            executioner.jobs.EndCurrentJob(JobCondition.InterruptForced);
            lastLoader = executioner;
            Cthulhu.Utility.DebugReport("Load state set to gathering");
        }
        private void TryLoadPrisoner(Pawn prisoner)
        {
            Pawn executioner = null;

            //Try to find an executioner.
            foreach (Pawn current in this.Map.mapPawns.FreeColonistsSpawned)
            {
                if (!current.Dead)
                {
                    if (current.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                      current.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                    {
                        if (Cthulhu.Utility.IsActorAvailable(current))
                        {
                            executioner = current;
                            break;
                        }
                    }
                }
            }

            if (executioner != null)
            {
                StartLoadingPrisoner(executioner, prisoner);
            }
            else
            {
                //Messages.Message("Cannot find executioner to carry out sacrifice", MessageSound.RejectInput);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            this.EjectContents();
            base.Destroy(mode);
        }

        private Graphic cachedGraphicFull = null;
        public override Graphic Graphic
        {
            get
            {
                if (Occupant == null)
                {
                    return base.Graphic;
                }
                if (this.def.building.fullGraveGraphicData == null)
                {
                    return base.Graphic;
                }
                if (this.cachedGraphicFull == null)
                {
                    this.cachedGraphicFull = this.def.building.fullGraveGraphicData.GraphicColoredFor(this);
                }
                return this.cachedGraphicFull;
            }
        }

        public override void EjectContents()
        {
            if (Occupant != null && Occupant is Pawn)
            {
                Pawn pawn = Occupant as Pawn;
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                if (firstHediffOfDef != null)
                {
                    int injuries = 0;
                    injuries = (int)(firstHediffOfDef.Severity * 10f);
                    if (injuries > 0)
                    {
                        for (int i = 0; i < injuries; i++)
                        {
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Stab, Rand.Range(2, 4), -1, this));
                        }

                        Hediff bloodLossSickness = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Uvhash_BloodLossSickness"));
                        if (bloodLossSickness == null)
                        {
                            Hediff bloodLossSicknessHediff = HediffMaker.MakeHediff(HediffDef.Named("Uvhash_BloodLossSickness"), pawn, null);
                            if (firstHediffOfDef.Severity > 0.5f) bloodLossSicknessHediff.Severity = 1.0f;
                            else bloodLossSicknessHediff.Severity = firstHediffOfDef.Severity;
                            pawn.health.AddHediff(bloodLossSicknessHediff);
                        }
                    }
                }
            }
            else if (Occupant != null && Occupant is Corpse)
            {
                Corpse corpse = Occupant as Corpse;
                Hediff bloodLossSickness = corpse.InnerPawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Uvhash_BloodLossSickness"));
                if (bloodLossSickness == null)
                {
                    Hediff bloodLossSicknessHediff = HediffMaker.MakeHediff(HediffDef.Named("Uvhash_BloodLossSickness"), corpse.InnerPawn, null);
                    bloodLossSicknessHediff.Severity = 1.0f;
                    corpse.InnerPawn.health.AddHediff(bloodLossSicknessHediff);
                }
            }
            base.EjectContents();
        }

        public bool warningMessage = false;

        public float DetermineBloodRemaining(Thing newOccupant, out float difference)
        {
            difference = 0.0f;
            float result = 0.0f;
            if (newOccupant != null)
            {
                //Live
                if (newOccupant is Pawn)
                {
                    Pawn pawn = newOccupant as Pawn;
                    if (pawn.Dead)
                    {
                        this.EjectContents();
                        return result;
                    }
                    if (pawn.health != null)
                    {
                        Hediff bloodLossSickness = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Uvhash_BloodLossSickness"));
                        if (bloodLossSickness == null)
                        {
                            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                            if (firstHediffOfDef == null)
                            {
                                Hediff bloodLossHediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn, null);
                                bloodLossHediff.Severity = 0.01f;
                                pawn.health.AddHediff(bloodLossHediff, null, new DamageInfo(DamageDefOf.Stab, Rand.Range(1, 3), -1, this));
                                firstHediffOfDef = bloodLossHediff;
                            }
                            firstHediffOfDef.Severity += 0.01f;

                            result = bloodTurnoverPoint - (bloodLossFatalVolume * firstHediffOfDef.Severity);

                            if (result < 4.0f && warningMessage == false)
                            {
                                warningMessage = true;
                                Messages.Message("BloodLossWarning".Translate(new object[]
                                    {
                                newOccupant.LabelShort
                                    }), MessageSound.Negative);
                            }
                        }
                        else
                        {
                            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                            if (firstHediffOfDef == null)
                            {
                                Hediff bloodLossHediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn, null);
                                bloodLossHediff.Severity = 0.01f;
                                if (bloodLossSickness.Severity > 0.0f) bloodLossHediff.Severity = bloodLossSickness.Severity;
                                pawn.health.AddHediff(bloodLossHediff, null, new DamageInfo(DamageDefOf.Stab, Rand.Range(1, 3), -1, this));
                                firstHediffOfDef = bloodLossHediff;
                            }
                            if (bloodLossSickness.Severity > 0.0f) firstHediffOfDef.Severity = bloodLossSickness.Severity;
                            pawn.health.RemoveHediff(bloodLossSickness);
                            firstHediffOfDef.Severity += 0.01f;

                            result = bloodTurnoverPoint - (bloodLossFatalVolume * firstHediffOfDef.Severity);
                        }
                    }
                }
                //Dead
                else if (newOccupant is Corpse)
                {
                    Corpse corpse = newOccupant as Corpse;
                    corpse.HitPoints -= 1;
                    if (corpse.HitPoints <= 1 || bloodLoaded <= 0.0f)
                    {
                        this.EjectContents();
                    }
                    CompRottable compRottable = corpse.GetComp<CompRottable>();
                    if (compRottable != null)
                    {
                        compRottable.RotProgress = compRottable.RotProgress + 3000f;
                    }
                    result = bloodLoaded - (bloodTurnoverPoint * 0.01f);
                }
                else
                {
                    result = bloodLoaded - (bloodTurnoverPoint * 0.01f);
                }
            }
            difference = bloodLoaded - result;
            return result;
        }

        public void CheckStatus()
        {
            if (IsLoaded)
            {
                if (GetInnerContainer().Count != 0)
                {
                    if (NearestBloodTank != null)
                    {
                        float difference = 0.0f;
                        bloodLoaded = DetermineBloodRemaining(Occupant, out difference);
                        if (difference > 0.0f) NearestBloodTank.TryGetComp<CompBloodTank>().AddBlood(difference);
                    }
                }
            }
            bool flag1 = false;
            if (lastLoader != null)
            {

                //foreach (Pawn current in this.Map.mapPawns.FreeColonistsSpawned)
                //{
                if (lastLoader.CurJob.def.defName == "BloodHaulPrisoner")
                {
                    flag1 = true;
                }
                //}
            }
            this.isLoading = flag1;
        }
        #region Overrides

        public override void Tick()
        {
            base.Tick();
            this.rareTicks--;
            if (rareTicks < 0)
            {
                rareTicks = 250;
                CheckStatus();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref this.isLoading, "isLoading", true, false);
            Scribe_Values.LookValue<float>(ref this.bloodLoaded, "bloodLoaded", 0.0f);
            Scribe_Values.LookValue<int>(ref this.rareTicks, "rareTicks", 250, false);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            if (IsLoaded)
            {
                stringBuilder.AppendLine("BloodBodyLoadable_BodyLoaded".Translate(new object[]
                    {
                        (bloodLoaded).ToString("F")
                    }));
            }
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
            if (!IsLoaded)
            {
                if (!isLoading)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.action = new Action(this.ProcessPrisonerButton);
                    command_Action.defaultLabel = "BloodCommandLoadPrisoner".Translate();
                    command_Action.defaultDesc = "BloodCommandLoadPrisonerDesc".Translate();
                    command_Action.hotKey = KeyBindingDefOf.Misc1;
                    command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners", true);
                    yield return command_Action;
                }
                else
                {
                    Command_Action command_Cancel = new Command_Action();
                    command_Cancel.action = new Action(this.ProcessPrisonerButton);
                    command_Cancel.defaultLabel = "CommandCancelConstructionLabel".Translate();
                    command_Cancel.defaultDesc = "BloodCommandCancelLoadPrisonerDesc".Translate();
                    command_Cancel.hotKey = KeyBindingDefOf.DesignatorCancel;
                    command_Cancel.icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
                    yield return command_Cancel;
                }
            }
            else
            {
                //Command_Action command_Action = new Command_Action();
                //command_Action.action = new Action(this.ProcessPrisonerButton);
                //command_Action.defaultLabel = "CommandPitSacrifice".Translate();
                //command_Action.defaultDesc = "CommandPitSacrificeDesc".Translate();
                //command_Action.hotKey = KeyBindingDefOf.Misc1;
                //command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners", true);
                //yield return command_Action;
            }
        }
    }

    #endregion Overrides
}