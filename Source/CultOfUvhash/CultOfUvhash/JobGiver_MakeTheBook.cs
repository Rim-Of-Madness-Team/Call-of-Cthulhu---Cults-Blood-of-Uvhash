using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfUvhash
{
    public class JobGiver_MakeTheBook : ThinkNode_JobGiver
    {
        private int lastJobGiverTick = -1;

        private bool ShouldMakeBookNow => Find.World?.GetComponent<WorldComponent_Uvhash>()?.CurrentCrystalStage <
                                          CrystalStage.BookCreated;
        
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (ShouldMakeBookNow && !pawn.Dead && !pawn.Downed && pawn.IsBloodMage())
            {
                if (lastJobGiverTick < Find.TickManager.TicksGame &&
                    pawn?.CurJob?.def != UvhashDefOf.Uvhash_CreateBook)
                {
                    lastJobGiverTick = Find.TickManager.TicksGame + 1000;
                    var nearestAnimal = GetClosestAnimal(pawn);
                    if (nearestAnimal == null)
                    {
                        Log.Error("Uvhash :: Unable to find nearest animal.");
                        return null;
                    }
                    var murderInnocentCreature = new Job(UvhashDefOf.Uvhash_CreateBook, new LocalTargetInfo(nearestAnimal));
                    murderInnocentCreature.locomotionUrgency = LocomotionUrgency.Sprint;
                    return murderInnocentCreature;
                }
            }
            return null;
        }

        private Pawn GetClosestAnimal(Pawn p)
        {
            Pawn closestAnimal = null;
            TraverseParms traverseParams = TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, true);
            closestAnimal = (Pawn)GenClosest.ClosestThingReachable(
                p.PositionHeld, p.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch,
                traverseParams,
                99f, x => (x is Pawn o && o.Spawned && !o.Dead && (o?.RaceProps?.Animal ?? false) && (o?.RaceProps?.canBePredatorPrey ?? false)));
            if (closestAnimal == null)
            {
                HashSet<PawnKindDef> tempSet = new HashSet<PawnKindDef>
                {
                    PawnKindDef.Named("Chicken"),
                    PawnKindDef.Named("Rat"),    
                    PawnKindDef.Named("Squirrel"), 
                    PawnKindDef.Named("Pig"),
                    PawnKindDef.Named("Cow")
                };
                PawnKindDef pawnKindDef = tempSet.RandomElement();
                var loc = CellFinder.RandomClosewalkCellNear(p.PositionHeld, p.MapHeld, 12, null);
                var pawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                GenSpawn.Spawn(pawn, loc, p.MapHeld, Rot4.Random, false);
                pawn.SetFaction(Faction.OfPlayer, null);
                closestAnimal = pawn;
            }
            return closestAnimal;
        }
    }
}