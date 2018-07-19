using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CultOfUvhash
{
    public class BloodCrystal : ThingWithComps
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Find.World.GetComponent<WorldComponent_Uvhash>().CurrentCrystalStage >= CrystalStage.BookCreated)
            {
                ThingDef buildable = UvhashDefOf.Uvhash_BloodNexus;
                Designator_Build des = FindDesignator(buildable);
                ThingDef stuff = ThingDefOf.WoodLog;
                if (des == null)
                {
                    yield break;
                }
                if (!des.Visible)
                {
                    yield break;
                }
                var commandAction = new Command_Action
                {
            
                    action = delegate
                    {
                        SoundDefOf.SelectDesignator.PlayOneShotOnCamera();
                        //des.SetStuffDef(stuff);
                        des.ProcessInput(new UnityEngine.Event());
                        Find.DesignatorManager.Select(des);
                    },
                    defaultLabel = buildable.label,
                    defaultDesc = buildable.description,
                    icon = des.icon,
                    iconProportions = des.iconProportions,
                    iconDrawScale = des.iconDrawScale,
                    iconTexCoords = des.iconTexCoords
                };
                if (stuff != null)
                {
                    commandAction.defaultIconColor = stuff.stuffProps.color;
                }
                else
                {
                    commandAction.defaultIconColor = buildable.IconDrawColor;
                }
                commandAction.hotKey = KeyBindingDefOf.Misc11;
                yield return commandAction;
            }
            yield break;   
        }
        
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

        private static bool isValid(Pawn selPawn)
        {
            return selPawn.Spawned && !selPawn.Dead && !selPawn.Downed;
        }
        
        private static Designator_Build FindDesignator(BuildableDef buildable)
        {
            List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                foreach (Designator current in allDefsListForReading[i].ResolvedAllowedDesignators)
                {
                    Designator_Build designator_Build = current as Designator_Build;
                    if (designator_Build != null && designator_Build.PlacingDef == buildable)
                    {
                        return designator_Build;
                    }
                }
            }
            return null;
        }
    }
}