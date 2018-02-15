using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfUvhash
{
    public class HediffWithComps_BloodTattoo_Impaler : HediffWithComps_BloodTattoo
    {
        private Mesh boltMesh;

        private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt", -1);
        private int age = 0;
        private int duration = 0;
        private IntVec3 StrikeLoc => this.pawn.PositionHeld;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            ImpalerStrike();
            age = 0;
            duration = Rand.Range(60, 120);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
        }

        public override void Tick()
        {
            base.Tick();
            age++;
            
            if (!Expired)
            Graphics.DrawMesh(this.boltMesh, 
                StrikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), 
                Quaternion.identity, 
                FadedMaterialPool.FadedVersionOf(LightningMat, LightningBrightness), 0);
        }

        public bool Expired
        {
            get => age > duration;
        }

        protected float LightningBrightness
        {
            get
            {
                if (this.age <= 3)
                {
                    return (float)this.age / 3f;
                }
                return 1f - (float)this.age / (float)this.duration;
            }
        }
        
        public void ImpalerStrike()
        {
            this.boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            GenExplosion.DoExplosion(this.StrikeLoc, this.pawn.MapHeld, 1.9f, DamageDefOf.Smoke, null, -1, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
            Vector3 loc = this.StrikeLoc.ToVector3Shifted();
            for (int i = 0; i < 4; i++)
            {
                MoteMaker.ThrowSmoke(loc, this.pawn.MapHeld, 1.5f);
                MoteMaker.ThrowMicroSparks(loc, this.pawn.MapHeld);
                MoteMaker.ThrowLightningGlow(loc, this.pawn.MapHeld, 1.5f);
            }
            SoundInfo info = SoundInfo.InMap(new TargetInfo(this.StrikeLoc, this.pawn.MapHeld, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
           
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref age, "age", 0);
            Scribe_Values.Look(ref duration, "duration", 0);
        }

    }
}