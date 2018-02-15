using AbilityUser;
using Verse;

namespace CultOfUvhash
{
    public enum BloodMageState
    {
        None,
        Discovery,
        Apprentice,
        Master,
        God
    }
    
    public class CompBloodMage : CompAbilityUser
    {
        private BloodMageState bloodMageState = BloodMageState.None;
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
            get => bloodMageState != BloodMageState.None;
        }

        public BloodMageState BloodMageState
        {
            get => bloodMageState;
            set => bloodMageState = value;
        }

        public override bool TryTransformPawn() => IsBloodMage;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.bloodMageState, "bloodMageState", BloodMageState.None);
            Scribe_Values.Look(ref this.bloodMageLevel, "bloodMageLevel", 0);
            Scribe_Values.Look(ref this.bloodMageLevelCap, "bloodMageLevelCap", 0);
            Scribe_Values.Look(ref this.bloodMageXP, "bloodMageXP", 0);
        }
    }
}