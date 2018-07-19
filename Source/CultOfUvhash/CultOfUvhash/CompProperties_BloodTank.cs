using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CultOfUvhash
{
    public class CompProperties_BloodTank : CompProperties
    {
        public float storedBloodMax = 16.5f;

        public float efficiency = 1.0f;

        public bool transmitsBlood = true;

        public List<GraphicData> graphicDatas = null;
        
        public CompProperties_BloodTank()
        {
            this.compClass = typeof(CompBloodTank);
        }
    }
}
