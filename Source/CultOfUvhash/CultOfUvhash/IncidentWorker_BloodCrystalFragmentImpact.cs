using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfUvhash
{
    public class IncidentWorker_BloodCrystalFragmentImpact : IncidentWorker
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            IntVec3 intVec;
            return this.TryFindCell(out intVec, map);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            if (!this.TryFindCell(out intVec, map))
            {
                return false;
            }
            
            var list = GenerateBloodStone();
            map.weatherManager.eventHandler.AddEvent(new WeatherEvent_BloodCometFlash(map));
            SkyfallerMaker.SpawnSkyfaller(UvhashDefOf.Uvhash_BloodCometFragmentIncoming, list, intVec, map);
            LetterDef textLetterDef = (!list[0].def.building.isResourceRock) ? LetterDefOf.NeutralEvent : LetterDefOf.PositiveEvent;
            string text = string.Format(this.def.letterText, list[0].def.label).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, textLetterDef, new TargetInfo(intVec, map, false), null);
            return true;
        }

        private static List<Thing> GenerateBloodStone()
        {
            ItemCollectionGeneratorParams parms2 = default(ItemCollectionGeneratorParams);
            int? count = parms2.count;
            parms2.count = new int?((count == null)
                ? ItemCollectionGenerator_Meteorite.MineablesCountRange.RandomInRange
                : count.Value);
            parms2.extraAllowedDefs = Gen.YieldSingle<ThingDef>(UvhashDefOf.Uvhash_Bloodstone);
            List<Thing> list = new List<Thing>(ItemCollectionGeneratorDefOf.Standard.Worker.Generate(parms2));
            return list;
        }

        private bool TryFindCell(out IntVec3 cell, Map map)
        {
            int maxMineables = ItemCollectionGenerator_Meteorite.MineablesCountRange.max;
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, true, false, false, false, delegate(IntVec3 x)
            {
                int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
                CellRect cellRect = CellRect.CenteredOn(x, num, num);
                int num2 = 0;
                CellRect.CellRectIterator iterator = cellRect.GetIterator();
                while (!iterator.Done())
                {
                    if (iterator.Current.InBounds(map) && iterator.Current.Standable(map))
                    {
                        num2++;
                    }
                    iterator.MoveNext();
                }
                return num2 >= maxMineables;
            });
        }
    }
}
