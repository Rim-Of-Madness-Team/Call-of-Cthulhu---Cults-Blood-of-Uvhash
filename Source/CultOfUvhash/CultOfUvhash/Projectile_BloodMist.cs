using System;
using Verse;
using Verse.Sound;
using RimWorld;

namespace CultOfUvhash
{
    public class Projectile_BloodMist : Projectile
    {
        public float bloodAmount = 0.0f;

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            if (hitThing != null)
            {
                if (hitThing is Building_BloodHub)
                {
                    Building_BloodHub hub = hitThing as Building_BloodHub;
                    CompBloodTank compBloodTank = hub.GetComp<CompBloodTank>();
                    if (compBloodTank != null)
                    {
                        compBloodTank.AddBlood(bloodAmount);
                    }
                }
                //int damageAmountBase = this.def.projectile.damageAmountBase;
                //ThingDef equipmentDef = this.equipmentDef;
                //DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.ExactRotation.eulerAngles.y, this.launcher, null, equipmentDef);
                //hitThing.TakeDamage(dinfo);

            }
            else
            {
                //SoundDefOf.BulletImpactGround.PlayOneShot(new TargetInfo(base.Position, map, false));
                //MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.bloodAmount, "bloodAmount", 0.0f);
        }
    }
}
