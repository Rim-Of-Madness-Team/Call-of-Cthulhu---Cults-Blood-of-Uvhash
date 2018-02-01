using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;
using static System.Int32;

namespace CultOfUvhash
{
    public enum ModStage
    {
        Comet,
        Crystal,
        Uvhash
    }

    public enum CometStage
    {
        NotStarted,
        InSkyline,
        Passing,
        Gone
    }

    public enum CrystalStage
    {
        None = 0,
        Discovered = 1,
        Investigated = 2,
        Whispering = 3,
        BookCreated = 4,
        NexusFormed = 5
    }

    public enum UvhashStage
    {
        None,
        Communicated,
        Summoned,
        Hungry,
        Awake
    }


    public class WorldComponent_Uvhash : WorldComponent
    {
        private readonly int DEATHSUNTILCOMET = 1;
        private readonly int TICKSUNTILCOMETARRIVES = GenDate.TicksPerDay * 3;
        private readonly int TICKSUNTILCOMETLEAVES = GenDate.TicksPerDay;

        private Pawn currentBloodMage = null;
        private Pawn currentBloodApprentice = null;
        private bool spawnedCrystal = false;
        private bool triggeredUvhash = false;
        private int deathsRecorded = 0;
        private int minedCellsUntilCrystal = 0;
        private int curCometDepartureTicks = MinValue;
        private int curCometArrivalTicks = MinValue;
        private CometStage currentCometStage = CometStage.NotStarted;
        private CrystalStage currentCrystalStage = CrystalStage.None;
        private UvhashStage currentUvhashStage = UvhashStage.None;


        public Pawn CurrentBloodApprentice {get => currentBloodApprentice; set => currentBloodApprentice = value;}
        public Pawn CurrentBloodMage {get => currentBloodMage; set => currentBloodMage = value;}
        public bool SpawnedCrystal {get => spawnedCrystal; set => spawnedCrystal = value;}

        public CometStage CurrentCometStage
        {
            get => currentCometStage;
            set
            {
                currentCometStage = value;
                Map map = Find.VisibleMap ?? Find.AnyPlayerHomeMap;
                switch (value)
                {
                    case CometStage.NotStarted:
                        break;
                    case CometStage.InSkyline:
                        if (map != null)
                            map.weatherManager.eventHandler.AddEvent(new WeatherEvent_BloodCometFlash(map));
                        Find.LetterStack.ReceiveLetter("CometInSkylineLabel".Translate(),
                            "CometInSkylineDesc".Translate(), LetterDefOf.NeutralEvent, null);
                        curCometArrivalTicks = TICKSUNTILCOMETARRIVES;
                        break;
                    case CometStage.Passing:
                        if (map != null)
                            map.weatherManager.eventHandler.AddEvent(new WeatherEvent_BloodCometFlash(map));
                        Find.LetterStack.ReceiveLetter("CometPassingLabel".Translate(), "CometPassingDesc".Translate(),
                            LetterDefOf.NeutralEvent, null);
                        curCometArrivalTicks = MinValue;
                        curCometDepartureTicks = TICKSUNTILCOMETLEAVES;
                        break;
                    case CometStage.Gone:
                        //Find.LetterStack.ReceiveLetter("", "", LetterDefOf.NeutralEvent, null);
                        IncidentDef incident = UvhashDefOf.Uvhash_BloodCometFragmentCrater;
                        IncidentParms parms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def,
                            incident.category, (from x in Find.Maps
                                where x.IsPlayerHome
                                select x).RandomElement<Map>());
                        if (!incident.Worker.TryExecute(parms))
                        {
                            Log.ErrorOnce("Uvhash :: Failed to create fragment.", 765222);
                        }
                        curCometDepartureTicks = MinValue;
                        //Notify_CrystalDiscovered(Find.AnyPlayerHomeMap.mapPawns.FreeColonists.RandomElement());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public CrystalStage CurrentCrystalStage
        {
            get => currentCrystalStage;
            set
            {
                currentCrystalStage = value;
                SpawnedCrystal = currentCrystalStage > CrystalStage.None;
            }
        }

        public UvhashStage CurrentUvhashStage
        {
            get => currentUvhashStage;
            set { currentUvhashStage = value; }
        }

        public WorldComponent_Uvhash(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            //Check for comet arrival
            if (curCometArrivalTicks != MinValue)
            {
                --curCometArrivalTicks;
                if (curCometArrivalTicks < 0)
                {
                    CurrentCometStage = CometStage.Passing;
                }
            }

            //Check for comet departure
            if (curCometDepartureTicks != MinValue)
            {
                --curCometDepartureTicks;
                if (curCometDepartureTicks < 0)
                {
                    CurrentCometStage = CometStage.Gone;
                }
            }
        }

        public void Notify_Death()
        {
            ++deathsRecorded;
            if (deathsRecorded == DEATHSUNTILCOMET)
            {
                CurrentCometStage = CometStage.InSkyline;
            }
        }

        public void DecrementCellsUntilCrystalCount(Pawn p)
        {
            if (minedCellsUntilCrystal >= 0)
                minedCellsUntilCrystal--;
            else
                Notify_CrystalDiscovered(p);
        }

        private void Notify_CrystalDiscovered(Pawn p)
        {
            CurrentCrystalStage = CrystalStage.Discovered;
            ThingWithComps bloodCrystal = (ThingWithComps) ThingMaker.MakeThing(UvhashDefOf.Uvhash_BloodCrystal, null);
            GenPlace.TryPlaceThing(bloodCrystal, p.PositionHeld, p.Map, ThingPlaceMode.Near);
            spawnedCrystal = true;
            Find.WindowStack.Add(new Dialog_MessageBox("BloodCrystalEventDesc".Translate(new object[]
            {
                p.Name.ToStringShort
            }).AdjustedFor(p), "BloodCrystalEventLabel".Translate()));
            BodyPartRecord hand = p?.health?.hediffSet?.GetNotMissingParts()
                .FirstOrDefault(x => x.def == BodyPartDefOf.RightHand || x.def == BodyPartDefOf.LeftHand);
            if (hand == null)
                hand = p?.health?.hediffSet?.GetNotMissingParts()
                    .FirstOrDefault(x => x.def.tags.Contains("ManipulationLimbSegment"));
            if (hand == null)
                hand = p?.health?.hediffSet?.GetNotMissingParts().FirstOrDefault(x => x.depth != BodyPartDepth.Inside);
            if (hand != null)
            {
                p.TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.Range(1, 2), -1, bloodCrystal, hand));
                FilthMaker.MakeFilth(p.PositionHeld, p.MapHeld, p.def.race.BloodDef, Rand.Range(1, 2));
            }
        }


        public void Notify_BloodBond(Pawn p)
        {
            if (p.TryGetComp<CompBloodMage>() is CompBloodMage bloodMageComp)
            {
                bloodMageComp.IsBloodMage = true;
                CurrentBloodMage = p;
                Find.WindowStack.Add(new Dialog_MessageBox("BloodCrystalBondDesc".Translate(new object[]
                {
                    p.Name.ToStringShort
                }).AdjustedFor(p), "BloodCrystalBond".Translate(p)));
                CurrentCrystalStage = CrystalStage.Investigated;
            }
        }

        public void Notify_UvhashCommunicated(Pawn p)
        {
            triggeredUvhash = true;
            Find.WindowStack.Add(new Dialog_MessageBox("BloodCrystalEventDesc".Translate(new object[]
            {
                p.Name.ToStringShort
            }), "BloodCrystalEventLabel".Translate()));
        }

        public void Notify_UvhashSummoned(Pawn p)
        {
            //todo   
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.currentBloodMage, "currentBloodMage");
            Scribe_References.Look(ref this.currentBloodApprentice, "currentBloodApprentice");
            Scribe_Values.Look(ref this.spawnedCrystal, "spawnedCrystal", false);
            Scribe_Values.Look(ref this.deathsRecorded, "deathsRecorded", 0);
            Scribe_Values.Look(ref this.triggeredUvhash, "triggeredUvhash", false);
            Scribe_Values.Look(ref this.currentUvhashStage, "currentStage", UvhashStage.None);
            Scribe_Values.Look(ref this.minedCellsUntilCrystal, "minedCellsUntilCrystal", 0);
        }

        public void Notify_BloodCrystalDestroyed(Pawn pawn)
        {
            CurrentCrystalStage = CrystalStage.None;
        }
    }
}