using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace CultOfUvhash
{
    public static class RuneUtility
    {
        // RimWorld.BuildCopyCommandUtility
        public static Command PlaceRuneBlueprint(BuildableDef buildable, ThingDef stuff)
        {
            Designator_Build des = RuneUtility.FindAllowedDesignator(buildable);
            if (des == null)
            {
                return null;
            }
            if (!des.Visible)
            {
                return null;
            }
            Command_Action command_Action = new Command_Action();
            command_Action.action = delegate
            {
                SoundDefOf.SelectDesignator.PlayOneShotOnCamera(null);
                des.SetStuffDef(stuff);
                Find.DesignatorManager.Select(des);
            };
            command_Action.defaultLabel = buildable.label.Translate();
            command_Action.defaultDesc = buildable.description.Translate();
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
            return command_Action;
        }

        // RimWorld.BuildCopyCommandUtility
        private static Designator_Build FindAllowedDesignator(BuildableDef buildable)
        {
            List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
            GameRules rules = Current.Game.Rules;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                List<Designator> allResolvedDesignators = allDefsListForReading[i].AllResolvedDesignators;
                for (int j = 0; j < allResolvedDesignators.Count; j++)
                {
                    if (rules.DesignatorAllowed(allResolvedDesignators[j]))
                    {
                        Designator_Build designator_Build = allResolvedDesignators[j] as Designator_Build;
                        if (designator_Build != null && designator_Build.PlacingDef == buildable)
                        {
                            return designator_Build;
                        }
                    }
                }
            }
            return null;
        }


    }
}
