using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using RimWorld;
using Verse;

namespace CultOfUvhash
{
    public class Building_CastingAltar : Building
    {
        

        private Building_BloodHub nearestHub;
        public Building_BloodHub NearestHub
        {
            get
            {
                if (this.Position != null && (nearestHub == null || nearestHub.Destroyed || !nearestHub.Spawned))
                {
                    if (this.MapHeld?.listerBuildings?.AllBuildingsColonistOfClass<Building_BloodHub>() is IEnumerable<Building_BloodHub> bloodHubs &&
                        (bloodHubs?.Count() ?? 0) > 0)
                    {
                        float num = -1;
                        Building_BloodHub candidate = null;
                        foreach (Building_BloodHub hub in bloodHubs)
                        {
                            if (hub.Position != null && hub.Spawned)
                            {
                                float num2 = hub.Position.DistanceToSquared(this.Position);
                                if (num2 < num || candidate == null)
                                {
                                    candidate = hub;
                                    num = num2;
                                }
                            }
                        }
                        nearestHub = candidate;
                    }
                }
                return nearestHub;
            }
        }

        public override string GetInspectString()
        {
            var s = new StringBuilder();
            s.Append(base.GetInspectString());
            s.AppendLine();
            if (NearestHub == null)
            {
                s.AppendLine("CastingAltarBlood_NexusRequired".Translate());
            }
            else
            {
                if (NearestHub.TryGetComp<CompBloodTank>() is CompBloodTank bloodTank)
                s.AppendLine("CastingAltarBlood_Available".Translate(new object[] {bloodTank.StoredBlood, bloodTank.Props.storedBloodMax}));
            }
            return s.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (Find.World.GetComponent<WorldComponent_Uvhash>().CurrentBloodMage != null && NearestHub != null)
                yield return new Command_Action
                {
                    defaultLabel = "CastingAltar_GiveBloodTattoo".Translate(),
                    icon = UvhashCommand.GiveBloodTattoo,
                    action = delegate
                    {
                        ShowBloodTattooMenu();
                    }
                };


//            if (UvhashDefOf.Forbidden_BloodMagicI.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Azathoth",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//            if (UvhashDefOf.Forbidden_BloodMagicAlterationI.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Gloon",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//
//            if (UvhashDefOf.Forbidden_BloodMagicAlterationII.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Rhogog",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//            if (UvhashDefOf.Forbidden_BloodMagicAlterationIII.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Hypnos",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//
//            if (UvhashDefOf.Forbidden_BloodMagicDestructionI.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Fthaggua",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//            if (UvhashDefOf.Forbidden_BloodMagicDestructionII.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Gtuhanai",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//
//            if (UvhashDefOf.Forbidden_BloodMagicDestructionIII.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Cthugha",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//
//            if (UvhashDefOf.Forbidden_BloodMagicDestructionIII.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Cthugha",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//            if (UvhashDefOf.Forbidden_BloodMagicSummoningI.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Baoht Z'uqqa-mogg",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//
//            if (UvhashDefOf.Forbidden_BloodMagicSummoningII.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Ythogtha",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
//
//            if (UvhashDefOf.Forbidden_BloodMagicSummoningIII.IsFinished)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Uvhash",
//                    icon = TexCommand.Attack,
//                    action = delegate
//                    {
//                        Log.Message("Spaghetti");
//                    }
//                };
//            }
        }

        private void ShowBloodTattooMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            var Uvhash = Find.World.GetComponent<WorldComponent_Uvhash>();
            var BloodMage = Uvhash.CurrentBloodMage;
            var BloodMageComp = BloodMage.GetComp<CompBloodMage>();
            int curBloodMageLevel = BloodMageComp.BloodMageLevel;
            if (curBloodMageLevel > 0)
            {
                if (Uvhash.CurrentBloodApprentice == null)
                {
                    list.Add(new FloatMenuOption("Mark of the Apprentice", delegate
                    {
                        var parms = new TargetingParameters
                        {
                            canTargetBuildings  = false, 
                            canTargetFires = false, 
                            canTargetLocations = false, 
                            canTargetItems =  false,
                            canTargetPawns = true,
                            canTargetSelf = false
                        };
                        Find.Targeter.BeginTargeting(parms, info =>
                        {
                            Log.Message(BloodMage.Label + " -> " + info.ToString());
                        } );
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));      
                }
            }
            if (list.Count == 0)
            {
                Messages.Message("NoStuffsToBuildWith".Translate(), MessageTypeDefOf.RejectInput);
            }
            else
            {
                FloatMenu floatMenu = new FloatMenu(list);
                floatMenu.vanishIfMouseDistant = true;
                Find.WindowStack.Add(floatMenu);
                //Find.DesignatorManager.Select(this);
            }
        }
    }
}
