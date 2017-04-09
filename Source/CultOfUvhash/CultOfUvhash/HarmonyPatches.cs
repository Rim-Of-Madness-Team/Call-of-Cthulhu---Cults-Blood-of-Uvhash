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

namespace CompActivatableEffect
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.uvhash");
            harmony.Patch(typeof(Pawn).GetMethod("ButcherProducts"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("ButcherProducts_PostFix")));
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
