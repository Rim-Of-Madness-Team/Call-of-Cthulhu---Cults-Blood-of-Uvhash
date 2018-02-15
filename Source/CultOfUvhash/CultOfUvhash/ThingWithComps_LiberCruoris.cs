using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;

namespace CultOfUvhash
{
    public class ThingWithComps_LiberCruoris : ThingWithComps
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            ThingDef buildable = UvhashDefOf.Uvhash_CastingAltar;
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
                defaultLabel = "BloodCommandBuildCastingAltar".Translate(),
                defaultDesc = "BloodCommandBuildCastingAltarDesc".Translate(),
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
            yield break;
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

