// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfUvhash
{
    public class JobDriver_InvestigateBloodCrystal : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        private TargetIndex InvestigateeIndex = TargetIndex.A;

        protected Thing Investigatee
        {
            get
            {
                return base.job.GetTarget(TargetIndex.A).Thing;
            }
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(InvestigateeIndex, JobCondition.Incompletable);
            //this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(InvestigateeIndex, this.job.def.joyMaxParticipants);
            var gotoInvestigatee = Toils_Goto.GotoThing(InvestigateeIndex, PathEndMode.ClosestTouch);
            yield return gotoInvestigatee;

            yield return Toils_Goto.GotoCell(Investigatee.InteractionCell, PathEndMode.OnCell);

            var watchToil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = this.job.def.joyDuration
            };
            watchToil.WithProgressBarToilDelay(TargetIndex.A);
            watchToil.AddPreTickAction(() =>
            {
                this.pawn.rotationTracker.FaceCell(this.TargetA.Cell);
                this.pawn.GainComfortFromCellIfPossible();
            });
            watchToil.AddFinishAction(() =>
            {
                Find.World.GetComponent<WorldComponent_Uvhash>().Notify_BloodBond(this.pawn);
            });
            yield return watchToil;
        }
    }
}
