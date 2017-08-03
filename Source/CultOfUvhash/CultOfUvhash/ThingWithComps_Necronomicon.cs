using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;

namespace CultOfUvhash
{
    class ThingWithComps_Necronomicon : ThingWithComps
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
            Command_Action command_Action = new Command_Action();
            command_Action.action = delegate
            {
                SoundDefOf.SelectDesignator.PlayOneShotOnCamera();
                //des.SetStuffDef(stuff);
                des.ProcessInput(new UnityEngine.Event());
                Find.DesignatorManager.Select(des);
            };
            command_Action.defaultLabel = "BloodCommandBuildCastingAltar".Translate();
            command_Action.defaultDesc = "BloodCommandBuildCastingAltarDesc".Translate();
            command_Action.icon = des.icon;
            command_Action.iconProportions = des.iconProportions;
            command_Action.iconDrawScale = des.iconDrawScale;
            command_Action.iconTexCoords = des.iconTexCoords;
            if (stuff != null)
            {
                command_Action.defaultIconColor = stuff.stuffProps.color;
            }
            else
            {
                command_Action.defaultIconColor = buildable.IconDrawColor;
            }
            command_Action.hotKey = KeyBindingDefOf.Misc11;
            yield return command_Action;
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

