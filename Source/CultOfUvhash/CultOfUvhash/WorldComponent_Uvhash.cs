using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;

namespace CultOfUvhash
{
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
        public bool spawnedCrystal = false;
        public bool spawnedNecronomicon = false;
        public bool triggeredUvhash = false;
        public int minedCellsUntilCrystal = 0;
        public UvhashStage currentStage = UvhashStage.None;
        
        public WorldComponent_Uvhash(World world) : base(world)
        {
            minedCellsUntilCrystal = Rand.Range(3, 5);
        }

        public void DecrementCellsUntilCrystalCount(Pawn p)
        {
            if (minedCellsUntilCrystal >= 0) minedCellsUntilCrystal--;
            else
            {
                Notify_CrystalDiscovered(p);
            }
        }

        public void Notify_CrystalDiscovered(Pawn p)
        {
            minedCellsUntilCrystal = -1;
            ThingWithComps bloodCrystal = (ThingWithComps)ThingMaker.MakeThing(UvhashDefOf.Uvhash_BloodCrystal, null);
            GenPlace.TryPlaceThing(bloodCrystal, p.PositionHeld, p.Map, ThingPlaceMode.Near);
            spawnedCrystal = true;
            Find.WindowStack.Add(new Dialog_MessageBox("BloodCrystalEventDesc".Translate(new object[]
            {
                            p.Name.ToStringShort
            }), "BloodCrystalEventLabel".Translate()));
            BodyPartRecord hand = p?.health?.hediffSet?.GetNotMissingParts().FirstOrDefault(x => x.def == BodyPartDefOf.RightHand || x.def == BodyPartDefOf.LeftHand);
            if (hand == null) hand = p?.health?.hediffSet?.GetNotMissingParts().FirstOrDefault(x => x.def.tags.Contains("ManipulationLimbSegment"));
            if (hand == null) hand = p?.health?.hediffSet?.GetNotMissingParts().FirstOrDefault(x => x.depth != BodyPartDepth.Inside);
            if (hand != null) {
                p.TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.Range(1, 2), -1, bloodCrystal, hand));
                FilthMaker.MakeFilth(p.PositionHeld, p.MapHeld, p.def.race.BloodDef, Rand.Range(1,2));
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
            
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.spawnedCrystal, "spawnedCrystal", false);
            Scribe_Values.Look<bool>(ref this.spawnedNecronomicon, "spawnedNecronomicon", false);
            Scribe_Values.Look(ref this.triggeredUvhash, "triggeredUvhash", false);
            Scribe_Values.Look(ref this.currentStage, "currentStage", UvhashStage.None);
            Scribe_Values.Look(ref this.minedCellsUntilCrystal, "minedCellsUntilCrystal", 0);
        }
    }
}
