using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CultOfUvhash
{
    public enum UvhashState : int
    {
        ChalkDrawn = 0,
        Active = 1,
        Unleashed = 2
    }
    public class Building_Uvhash : Building
    {
        public UvhashState state = UvhashState.ChalkDrawn;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;
            
            switch (state)
            {
                case UvhashState.ChalkDrawn:
                    Command_Action summon = new Command_Action();
                    summon.defaultLabel = "";
                    summon.defaultDesc = "";
                    summon.action = delegate
                    {

                    };
                    yield return summon;
                    break;
                case UvhashState.Active:
                    Command_Action dismiss = new Command_Action();
                    dismiss.defaultLabel = "";
                    dismiss.defaultDesc = "";
                    dismiss.action = delegate
                    {

                    };
                    yield return dismiss;
                    break;
            }
            
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<UvhashState>(ref this.state, "state", UvhashState.ChalkDrawn);
        }
    }
}
