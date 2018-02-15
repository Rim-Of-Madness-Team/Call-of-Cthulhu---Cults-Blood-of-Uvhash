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
    public class JobDriver_CreateBook : JobDriver
    {
	    public override RandomSocialMode DesiredSocialMode()
	    {
		    return RandomSocialMode.Off;
	    }

	    private string curReport = "";
	    private MoteProlonged moteThrown = null;
	    private MoteProlonged moteTarget = null;
	    public override bool TryMakePreToilReservations() => true;

	    private List<string> cultMadText = new List<string>
	    {
			"เค เค",
		    "เα เα",
		    "ยשђครђ",
		    "µѵɦαรɦ",
		    "ɠɳ'ƭɦ'ɓƭɦɳҡ",
		    "૨'ℓµɦɦσ૨"
	    };
	    private Corpse Corpse => this.job.GetTarget(TargetIndex.A).Thing as Corpse;
	    private Pawn Prey
	    {
		    get
		    {
			    Corpse corpse = this.Corpse;
			    if (corpse != null)
			    {
				    return corpse.InnerPawn;
			    }
			    return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
		    }
	    }

	    public override string GetReport()
	    {
		    if (curReport == "")
			    return base.GetReport();
		    return curReport;
	    }

	    protected override IEnumerable<Toil> MakeNewToils()
        {

	        var checkIncaseJobWasCancelledPreviously = new Toil
	        {
		        initAction = () =>
		        {
			        var curMap = this.GetActor().MapHeld;
			        var lastCorpse = curMap.listerThings.AllThings.FirstOrDefault(x =>
				        x is Corpse c && c.InnerPawn.health.hediffSet.HasHediff(UvhashDefOf.Uvhash_TattooParalysis));
			        if (lastCorpse != null)
				        this.job.SetTarget(TargetIndex.A, lastCorpse);
		        }
	        };

	        var prepareToTransformCorpse = new Toil();
			prepareToTransformCorpse.initAction = delegate
			{
				Pawn actor = prepareToTransformCorpse.actor;
				Corpse corpse = this.Corpse;
				if (corpse == null)
				{
					Pawn prey = this.Prey;
					if (prey == null)
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
						return;
					}
					corpse = prey.Corpse;
					if (corpse == null || !corpse.Spawned)
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
						return;
					}
				}
				corpse.SetForbidden(actor.Faction != Faction.OfPlayer, false);
				actor.CurJob.SetTarget(TargetIndex.A, corpse);
			};
	        var goToCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
	        var goToPreyPos = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			var gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, true).JumpIfDespawnedOrNull(TargetIndex.A, prepareToTransformCorpse).FailOn(() => Find.TickManager.TicksGame > this.jobStartTick + 5000);
			var moveIfCannotHit = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, goToPreyPos);
	        var castSpell = new Toil {defaultCompleteMode = ToilCompleteMode.Delay};
	        castSpell.WithProgressBarToilDelay(TargetIndex.A);
	        castSpell.defaultDuration = 1200;
	        castSpell.AddPreInitAction(() =>
	        {
		        curReport = "BloodMage_CastingOn".Translate(TargetA.Thing.Label);
		        moteThrown = UvhashUtility.ThrowMagicMote(1f, this.GetActor());
		        moteThrown.LifeSpanOffset += 99999f;
		        moteTarget.LifeSpanOffset += 99999f;
		        moteTarget = UvhashUtility.ThrowMagicMote(TargetA.Thing.def.race.baseBodySize, TargetA.Thing);
	        });
	        castSpell.tickAction = () =>
	        {
		        if (moteThrown != null)
		        {
			        moteThrown.exactPosition = this.GetActor().DrawPos;
		        }
		        if (moteTarget != null)
		        {
			        moteTarget.exactPosition = TargetA.Thing.DrawPos;
		        }
		        if (Find.TickManager.TicksGame % 15 == 0)
		        {
			        MoteMaker.ThrowText(this.GetActor().DrawPos + new Vector3(new FloatRange(-1, 1).RandomInRange, 0, new FloatRange(-1, 1).RandomInRange), this.GetActor().MapHeld, cultMadText.RandomElement(), Color.red);
			        //MoteMaker.ThrowText(this.TargetA.Thing.DrawPos + new Vector3(new FloatRange(-1, 1).RandomInRange, 0, new FloatRange(-1, 1).RandomInRange), this.GetActor().MapHeld, cultMadText.RandomElement(), Color.red);
		        }
	        };
	        castSpell.PlaySustainerOrSound(UvhashDefOf.Uvhash_BloodMagicCastingSustainer);
	        castSpell.AddFinishAction(() =>
	        {
		        if (!Prey.Downed)
			        HealthUtility.AdjustSeverity(Prey, UvhashDefOf.Uvhash_TattooParalysis, 1.0f);
		        curReport = "";
		        if (moteThrown != null)
			        moteThrown.LifeSpanOffset -= 999999f;
		        if (moteTarget != null)
			        moteTarget.LifeSpanOffset -= 999999f;
	        });
	        var executeTarget = new Toil {defaultCompleteMode = ToilCompleteMode.Delay};
			executeTarget.WithProgressBarToilDelay(TargetIndex.B);
			executeTarget.defaultDuration = 200;
			executeTarget.AddFinishAction(() =>
			{
				ExecutionUtility.DoExecutionByCut(this.GetActor(), Prey);
				
			});
			Toil transformCorpse = new Toil();
			transformCorpse.defaultCompleteMode = ToilCompleteMode.Delay;
			transformCorpse.WithEffect(() => EffecterDef.Named("ButcherFlesh"), TargetIndex.A);
			transformCorpse.WithProgressBarToilDelay(TargetIndex.A);
	        transformCorpse.defaultDuration = 800;
			transformCorpse.AddPreInitAction(() =>
			{
				Messages.Message("BloodBookCreation_Message".Translate(new object[]{this.pawn.LabelCap, this.Prey.Label}), MessageTypeDefOf.NeutralEvent);
			});
			var destroyCorpseAndSpawnBook = new Toil();
			destroyCorpseAndSpawnBook.initAction = ()=>
			{
				var corpsePos = Corpse.PositionHeld;
				var corpseMap = Corpse.MapHeld;
				Corpse.Destroy();
				FilthMaker.MakeFilth(corpsePos, corpseMap, ThingDefOf.FilthBlood, Rand.Range(1, 2));
				for (int i = 0; i < 10; i++)
				{
					FilthMaker.MakeFilth(corpsePos.RandomAdjacentCell8Way(), corpseMap, ThingDefOf.FilthBlood, Rand.Range(1, 2));
					this.GetActor().filth.GainFilth(ThingDefOf.FilthBlood);
				}
				var book = (ThingWithComps_LiberCruoris)ThingMaker.MakeThing(UvhashDefOf.Uvhash_LiberCruoris);
				GenPlace.TryPlaceThing(book, corpsePos, corpseMap, ThingPlaceMode.Near);
				
				Find.World.GetComponent<WorldComponent_Uvhash>().CurrentCrystalStage = CrystalStage.BookCreated;
				this.GetActor().MentalState.RecoverFromState();
				UvhashUtility.ShowMessageBox();
			};

			yield return new Toil
			{
				initAction = delegate
				{
					this.jobStartTick = Find.TickManager.TicksGame;
				}
			};
			yield return checkIncaseJobWasCancelledPreviously;
	        yield return Toils_Jump.JumpIf(goToCorpse, () => Prey.Dead);
	        yield return Toils_Jump.JumpIf(goToPreyPos, () => Prey.Downed);
			yield return Toils_Combat.TrySetJobToUseAttackVerb();
			yield return gotoCastPos.FailOn(() => this.GetActor().IsFighting());
	        yield return castSpell;
	        yield return goToPreyPos;
			yield return executeTarget;
			yield return prepareToTransformCorpse;
	        yield return goToCorpse;
		    yield return transformCorpse;
			yield return destroyCorpseAndSpawnBook;
	        
	        this.AddFinishAction(() =>
	        {
		        if (moteThrown != null)
			        moteThrown.LifeSpanOffset -= 999999f;
		        if (moteTarget != null)
			        moteTarget.LifeSpanOffset -= 999999f;
	        });
	        yield break;

		}


	    private bool notifiedPlayer;

	    private bool firstHit = true;
	    private int jobStartTick;

	    public override void ExposeData()
	    {
		    base.ExposeData();
		    Scribe_Values.Look<bool>(ref this.firstHit, "firstHit", false, false);
	    }

    }
}
