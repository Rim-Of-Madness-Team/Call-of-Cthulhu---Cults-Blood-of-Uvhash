using Verse;

namespace CultOfUvhash
{
    public class MoteProlonged : Mote
    {
        private float lifeSpanOffset = 0f;
        public float LifeSpanOffset { get => lifeSpanOffset; set => lifeSpanOffset = value; }

        public override void Draw()
        {
            base.Draw();
            if (Find.TickManager.Paused)
                LifeSpanOffset++;
        }

        protected override float LifespanSecs { get => base.LifespanSecs + lifeSpanOffset; }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            this.exactRotation += this.rotationRate * deltaTime;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lifeSpanOffset, "lifeSpanOffset");
        }
    }
}