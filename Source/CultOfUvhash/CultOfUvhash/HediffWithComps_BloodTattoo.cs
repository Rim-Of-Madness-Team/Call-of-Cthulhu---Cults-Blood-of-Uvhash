using System;
using System.Text;
using Verse;

namespace CultOfUvhash
{
    public class HediffWithComps_BloodTattoo : HediffWithComps
    {
        public override string TipStringExtra => this.def.description + "\n" + base.TipStringExtra;
    }

    public class HediffWithComps_BloodTattoo_BloodMage : HediffWithComps_BloodTattoo
    {
        public override string TipStringExtra
        {
            get
            {
                var s = new StringBuilder();
                s.Append(this.def.description);
                var bmComp = this.pawn.TryGetComp<CompBloodMage>();
                if (bmComp != null)
                {
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (bmComp.BloodMageState)
                    {
                        case BloodMageState.None:
                            break;
                        case BloodMageState.Discovery:
                            s.AppendLine("\t" + "BloodMageHediff_HearsWhispers".Translate());
                            break;
                        case BloodMageState.Apprentice:
                            s.AppendLine("\t" + "BloodMageHediff_Apprentice".Translate());
                            break;
                        case BloodMageState.Master:
                            s.AppendLine("\t" + "BloodMageHediff_Master".Translate());
                            break;
                    }
                }
                
                var modState = Find.World.GetComponent<WorldComponent_Uvhash>();
                if (modState != null && modState.CurrentCrystalStage > CrystalStage.Discovered)
                {
                    s.AppendLine("\t" + "BloodMageHediff_Bonded".Translate());
                }

                return s.ToString();
            }
        }
    }
}