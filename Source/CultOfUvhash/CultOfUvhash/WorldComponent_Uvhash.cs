using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
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

    public struct ud
    {
        public int Value;
        public bool UvhashRequired;

        public ud(int value, bool uvhashRequired = false)
        {
            this.Value = value;
            this.UvhashRequired = uvhashRequired;
        }
    }
    
    public static class BloodMagic_SpellUnlockLevels
    {
        //Tattoos
        public static ud TattooLeg = new ud(1);
        public static ud TattooArm = new ud(1);
        public static ud TattooHand = new ud(1);
        public static ud TattooEye = new ud(2);
        public static ud TattooEar = new ud(2);
        public static ud TattooSpine = new ud(3);
        public static ud TattooSternum = new ud(3);
        public static ud TattooWaist = new ud(3);
        
        //Tattoos - Named
        public static ud TattooTheMage = new ud(0);
        public static ud TattooTheApprentice = new ud(1);
        public static ud TattooTheLamb = new ud(7, true);
        public static ud TattooTheBeast = new ud(7, true);
        public static ud TattooTheCollar = new ud(7, true);
        public static ud TattooBloodBomb = new ud(13, true);
        public static ud TattooTheHive = new ud(13, true);
        public static ud TattooEyebiter = new ud(13, true);
        public static ud TattooBoneMantis = new ud(25, true);
        public static ud TattooTheDefiler = new ud(25, true);
        
        //Infusions
        public static ud InfusionLeech = new ud(7, true);
        public static ud InfusionCollection = new ud(7, true);
        public static ud InfusionSadism = new ud(13, true);
        public static ud InfusionMasochism = new ud(13, true);
        public static ud InfusionUvhashEmbraces = new ud(25, true);
        public static ud InfusionUvhashGuides = new ud(25, true);
        public static ud InfusionUvhashLeads = new ud(25, true);
        public static ud InfusionUvhashSees = new ud(25, true);
        public static ud InfusionUvhashWills = new ud(25, true);
        public static ud InfusionUvhashConsumes = new ud(25, true);
        public static ud InfusionUvhashLaughs = new ud(25, true);
        public static ud InfusionUvhashDesires = new ud(25, true);
        public static ud InfusionUvhashKnows = new ud(25, true);
        public static ud InfusionUvhashDestroys = new ud(25, true);
        public static ud InfusionUvhashWrath = new ud(25, true);
        
        //Rituals
        public static ud RitualSummonUvhash = new ud(7, true);
        public static ud RitualAvatar = new ud(25, true);

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
        private int currentBloodMagicCap;


        public Pawn CurrentBloodApprentice {get => currentBloodApprentice; set => currentBloodApprentice = value;}
        public Pawn CurrentBloodMage {get => currentBloodMage; set => currentBloodMage = value;}
        public int CurrentBloodMagicCap {get => currentBloodMagicCap;
            set => currentBloodMagicCap = value;
        }
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
            var dialogDesc = "BloodCrystalEventDesc".Translate(new object[]
            {
                p.Name.ToStringShort
            }).AdjustedFor(p);
            var dialogButton = "BloodCrystalEventLabel".Translate();
            UvhashUtility.ShowMessageBox(dialogDesc, dialogButton);
            
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
            if (p.TryGetComp<CompBloodMage>() is CompBloodMage bloodMageComp && CurrentCrystalStage != CrystalStage.Investigated)
            {
                CurrentCrystalStage = CrystalStage.Investigated;
                bloodMageComp.BloodMageState = BloodMageState.Discovery;
                CurrentBloodMage = p;
                Find.WindowStack.Add(new Dialog_MessageBox("BloodCrystalBondDesc".Translate(new object[]
                {
                    p.Name.ToStringShort
                }).AdjustedFor(p), "BloodCrystalBond".Translate(p)));
                p.mindState.mentalStateHandler.TryStartMentalState(UvhashDefOf.Uvhash_WillOfUvhash);
                HealthUtility.AdjustSeverity(p, UvhashDefOf.Uvhash_TattooBloodMage, 1.0f);
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
            Scribe_Values.Look(ref this.spawnedCrystal, "spawnedCrystal");
            Scribe_Values.Look(ref this.deathsRecorded, "deathsRecorded");
            Scribe_Values.Look(ref this.triggeredUvhash, "triggeredUvhash");
            Scribe_Values.Look(ref this.currentUvhashStage, "currentStage");
            Scribe_Values.Look(ref this.currentCometStage, "currentCometStage");
            Scribe_Values.Look(ref this.currentCrystalStage, "currentCrystalStage");
            Scribe_Values.Look(ref this.minedCellsUntilCrystal, "minedCellsUntilCrystal");
        }

        public void Notify_BloodCrystalDestroyed(Pawn pawn)
        {
            CurrentCrystalStage = CrystalStage.None;
        }
    }
}