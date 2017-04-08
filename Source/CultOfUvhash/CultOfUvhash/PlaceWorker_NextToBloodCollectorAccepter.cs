using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace CultOfUvhash
{
    public class PlaceWorker_NextToBloodCollectorAccepter : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
            for (int i = 0; i < 4; i++)
            {
                IntVec3 c = loc + GenAdj.CardinalDirections[i];
                if (c.InBounds(base.Map))
                {
                    List<Thing> thingList = c.GetThingList(base.Map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        Thing thing = thingList[j];
                        if (thing is Building_BloodFactory)
                        {
                            return true;
                        }
                    }
                }
            }
            return "MustPlaceNextToBloodCollectorAccepter".Translate();
        }
    }
}
