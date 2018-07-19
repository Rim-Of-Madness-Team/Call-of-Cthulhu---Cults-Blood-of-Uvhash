using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfUvhash
{
    public class Building_BloodHub : Building
    {
        private Graphic _graphic;

        public override Graphic Graphic
        {
            get
            {
                if (CompBloodTank.Props.graphicDatas.NullOrEmpty()) return base.Graphic;
                if (_graphic == null || Find.TickManager.TicksGame % 250 == 0)
                {
                    _graphic = CompBloodTank.Props.graphicDatas[0].GraphicColoredFor(this);
                    var divisor = CompBloodTank.Props.graphicDatas.Count;
                    for (var i = 0; i < divisor; i++)
                    {
                        if (CompBloodTank.StoredBlood >= (CompBloodTank.Props.storedBloodMax / (divisor + i)))
                        {
                            _graphic = CompBloodTank.Props.graphicDatas[i].GraphicColoredFor(this);
                        }
                    }   
                }
                return _graphic;
            }
        }

        private CompBloodTank CompBloodTank
        {
            get { return this.GetComp<CompBloodTank>(); }
        }

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
