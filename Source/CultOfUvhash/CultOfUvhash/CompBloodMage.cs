using AbilityUser;
using Verse;

namespace CultOfUvhash
{
    public class CompBloodMage : CompAbilityUser
    {
        private bool isBloodMage = false;
        private bool isBloodApprentice;
        private int bloodMageLevelCap;
        private int bloodMageLevel;
        private int bloodMageXP;

        public int BloodMageLevel
        {
            get => bloodMageLevel;
            set
            {
                if (value > bloodMageLevel && value != 0)
                {
                    if (BloodMageXP < value * 600)
                    {
                        BloodMageXP = value * 600;
                    }
                }
                else if (value == 0)
                    BloodMageXP = 0;
                bloodMageLevel = value;
            }
        }

        public int BloodMageXP
        {
            get => bloodMageXP;
            set
            {
                bloodMageXP = value;
                if (bloodMageXP > 0 && bloodMageXP > BloodMageXPTillNextLevel)
                    Notify_LevelUp(true);
            }
        }

        private void Notify_LevelUp(bool showMessage = false)
        {
            
        }

        public float BloodMageXPLastLevel
        {
            get
            {
                float result = 0f;
                if (bloodMageLevel > 0) result = (bloodMageLevel * 600);
                return result;
            }
        }

        public float BloodMageXPTillNextLevelPercent => (float) (bloodMageXP - BloodMageXPLastLevel) / (float) (BloodMageXPTillNextLevel - BloodMageXPLastLevel);
        public int BloodMageXPTillNextLevel => ((bloodMageLevel + 1) * 600);


        public bool IsBloodMage
        {
            get => isBloodMage;
            set => isBloodMage = value;
        }

        public override bool TryTransformPawn() => isBloodMage;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.isBloodMage, "isBloodMage", false);
            Scribe_Values.Look(ref this.isBloodApprentice, "isBloodApprentice", false);
            Scribe_Values.Look(ref this.bloodMageLevel, "bloodMageLevel", 0);
            Scribe_Values.Look(ref this.bloodMageLevelCap, "bloodMageLevelCap", 0);
            Scribe_Values.Look(ref this.bloodMageXP, "bloodMageXP", 0);
        }
    }
}