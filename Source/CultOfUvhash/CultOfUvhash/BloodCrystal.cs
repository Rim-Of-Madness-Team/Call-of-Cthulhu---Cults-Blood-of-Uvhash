using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CultOfUvhash
{
    public class BloodCrystal : ThingWithComps
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (isValid(selPawn))
            {
                if (Find.World.GetComponent<WorldComponent_Uvhash>().CurrentBloodMage == null &&
                    Find.World.GetComponent<WorldComponent_Uvhash>().CurrentCrystalStage < CrystalStage.Investigated)
                {
                    yield return new FloatMenuOption(
                        "BloodCrystal_Investigate".Translate(), () =>
                        {
                            var job = new Job(UvhashDefOf.Uvhash_InvestigateCrystal, this);
                            selPawn.jobs.StartJob(job);
                        }
                    );   
                }
                yield return new FloatMenuOption(
                    "BloodCrystal_Destroy".Translate(), () =>
                    {
                        var job = new Job(UvhashDefOf.Uvhash_DestroyCrystal, this);
                        selPawn.jobs.StartJob(job);
                    }
                );
            }
            
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(selPawn))
                yield return o;
            
        }

        public static bool isValid(Pawn selPawn)
        {
            return selPawn.Spawned && !selPawn.Dead && !selPawn.Downed;
        }
    }
}