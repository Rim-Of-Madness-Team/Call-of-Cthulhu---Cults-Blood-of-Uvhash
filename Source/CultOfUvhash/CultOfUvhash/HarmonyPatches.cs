using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;

namespace CultOfUvhash
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.uvhash");
            harmony.Patch(typeof(Pawn).GetMethod("ButcherProducts"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("ButcherProducts_PostFix")));
            harmony.Patch(typeof(Pawn).GetMethod("Kill"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(Kill)));
            harmony.Patch(typeof(Designator_Build).GetMethod("get_Visible"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(HideBloodCultsThings)));
            //harmony.Patch(typeof(Pawn_RecordsTracker).GetMethod("Increment"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Increment_PostFix")), null);
            harmony.Patch(typeof(Mineable).GetMethod("DestroyMined"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("DestroyMined")), null);
        }
        
        // RimWorld.Designator_Build
        public static void HideBloodCultsThings(Designator_Build __instance, ref bool __result)
        {
            var entDef = Traverse.Create(__instance).Field("entDef").GetValue<BuildableDef>();
            if (entDef is ThingDef d && d == UvhashDefOf.Uvhash_CastingAltar)
            {
                __result = Find.World.GetComponent<WorldComponent_Uvhash>().CurrentCrystalStage >= CrystalStage.BookCreated;
            }
        }

        //Mineable
        public static void DestroyMined(Mineable __instance, Pawn pawn)
        {
            if (Find.World.GetComponent<WorldComponent_Uvhash>() is WorldComponent_Uvhash uvhashComp &&
                !uvhashComp.SpawnedCrystal && __instance.def == UvhashDefOf.Uvhash_Bloodstone)
            {
                uvhashComp.DecrementCellsUntilCrystalCount(pawn);
            }
        }
        
        public static void Kill(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance?.RaceProps?.Humanlike ?? false)
            {
                var uvhashComp = Find.World.GetComponent<WorldComponent_Uvhash>();
                if (uvhashComp != null)
                {
                    uvhashComp.Notify_Death();
                }   
            }
        }

        public static readonly float boneDivisor = 2f;

        // Verse.Pawn
        public static void ButcherProducts_PostFix(Pawn __instance, Pawn butcher, float efficiency)
        {
            //Log.Message("1");
            if (__instance.RaceProps.meatDef != null)
            {

                //Log.Message("2");
                int boneCount = GenMath.RoundRandom((__instance.GetStatValue(StatDefOf.MeatAmount, true) * efficiency) / boneDivisor);
                if (boneCount > 0)
                {

                    //Log.Message("3");
                    Thing bone = ThingMaker.MakeThing(ThingDef.Named("Uvhash_Bone"), null);
                    bone.stackCount = boneCount;
                    GenPlace.TryPlaceThing(bone, butcher.PositionHeld, butcher.MapHeld, ThingPlaceMode.Near);
                    //__result.Add(meat);
                }
            }
        }

    }
}
