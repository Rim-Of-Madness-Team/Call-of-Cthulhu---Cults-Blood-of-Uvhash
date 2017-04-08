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
    public class JobDriver_BloodHaulPrisoner : JobDriver
    {
        private const TargetIndex TakeeIndex = TargetIndex.A;
        private const TargetIndex AltarIndex = TargetIndex.B;
        private string customString = "";

        protected Pawn Takee
        {
            get
            {
                return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Building_BloodFactory DropPoint
        {
            get
            {
                return (Building_BloodFactory)base.CurJob.GetTarget(TargetIndex.B).Thing;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);

            yield return Toils_Reserve.Reserve(TakeeIndex, 1);
            yield return Toils_Reserve.Reserve(AltarIndex, 1);

            yield return new Toil
            {
                initAction = delegate
                {
                    DropPoint.IsLoading = true;
                    customString = "BloodHaulPrisoner_Gathering".Translate();
                }
            };

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            Toil waitingTime = new Toil();
            waitingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            waitingTime.defaultDuration = 1200;
            waitingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            waitingTime.initAction = delegate
            {
                customString = "BloodHaulPrisoner_Strapping".Translate(new object[]
                    {
                        this.Takee.LabelShort
                    });
            };
            yield return waitingTime;
            yield return new Toil
            {
                initAction = delegate
                {
                    customString = "BloodHaulPrisoner_Finished".Translate();
                    IntVec3 position = this.DropPoint.Position;
                    Thing thing;
                    this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out thing, null);
                    if (!this.DropPoint.Destroyed)
                    {
                        PrisonerHaulCompleted();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return Toils_Reserve.Release(TargetIndex.B);

            //Toil 9: Think about that.
            yield return new Toil
            {
                initAction = delegate
                {
                    ////It's a day to remember
                    //TaleDef taleToAdd = TaleDef.Named("HeldSermon");
                    //if ((this.pawn.IsColonist || this.pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
                    //{
                    //    TaleRecorder.RecordTale(taleToAdd, new object[]
                    //    {
                    //       this.pawn,
                    //    });
                    //}
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield break;


        }

        public override string GetReport()
        {
            if (customString == "")
            {
                return base.GetReport();
            }
            return customString;
        }


        private void PrisonerHaulCompleted()
        {
            //Drop them in~~
            this.Takee.Position = DropPoint.Position;
            this.Takee.Notify_Teleported(false);
            this.Takee.stances.CancelBusyStanceHard();
            //....the pit
            this.DropPoint.GetInnerContainer().TryAdd(Takee);
            //this.DropPoint.IsLoaded = true;
            
            //Record a tale
            //TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, new object[]
            //{
            //            this.pawn,
            //            this.Takee
            //});
        }
    }
}
