using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CultOfUvhash
{
    public class Building_BloodRune : Building
    {
        public Building_BloodHub FindNearestHub()
        {
            Building_BloodHub parentCandidate = null;

            Map map = this.MapHeld;
            if (map != null)
            {
                IEnumerable<Building_BloodHub> bloodHubs = map.listerBuildings.AllBuildingsColonistOfClass<Building_BloodHub>();
                if (bloodHubs != null)
                {
                    if (this.Position != null)
                    {
                        float num = -1;
                        foreach (Building_BloodHub hub in bloodHubs)
                        {
                            if (hub.Position != null)
                            {
                                float num2 = hub.Position.DistanceToSquared(this.Position);
                                if (num2 < num || parentCandidate == null)
                                {
                                    parentCandidate = hub;
                                    num = num2;
                                }
                            }
                        }
                    }
                }
            }
            return parentCandidate;
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
        }
    }
}
