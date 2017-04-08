using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfUvhash
{
    public class Building_BloodHub : Building
    {
        public override void Draw()
        {
            base.Draw();
            //CompPowerBattery comp = base.GetComp<CompPowerBattery>();
            //GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            //r.center = this.DrawPos + Vector3.up * 0.1f;
            //r.size = Building_Battery.BarSize;
            //r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            //r.filledMat = Building_Battery.BatteryBarFilledMat;
            //r.unfilledMat = Building_Battery.BatteryBarUnfilledMat;
            //r.margin = 0.15f;
            //Rot4 rotation = base.Rotation;
            //rotation.Rotate(RotationDirection.Clockwise);
            //r.rotation = rotation;
            //GenDraw.DrawFillableBar(r);
            //if (this.ticksToExplode > 0 && base.Spawned)
            //{
            //    base.Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
            //}
        }

        public override void Tick()
        {
            base.Tick();
        }
        
    }
}
