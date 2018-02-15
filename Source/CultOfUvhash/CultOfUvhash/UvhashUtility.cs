using Verse;

namespace CultOfUvhash
{
    [StaticConstructorOnStartup]
    public static class UvhashUtility
    {
        public static bool IsBloodMage(this Pawn p)
        {
            return p?.TryGetComp<CompBloodMage>() is CompBloodMage bm && (bm?.IsBloodMage ?? false);
        }
        
        public static bool IsBloodApprentice(this Pawn p)
        {
            return p?.TryGetComp<CompBloodMage>() is CompBloodMage bm && (bm?.BloodMageState == BloodMageState.Apprentice);
        }
        
        public static MoteProlonged ThrowMagicMote(float scaleMod, Thing attachee)
        {
            var moteThrown = (MoteProlonged) ThingMaker.MakeThing(UvhashDefOf.Uvhash_MoteBloodMagicCasting, null);
            moteThrown.Scale = 2.5f * scaleMod;
            moteThrown.rotationRate = 4f;
            moteThrown.Attach(attachee);
            moteThrown.exactPosition = attachee.DrawPos;
            moteThrown.exactPosition.y = Altitudes.AltitudeFor(AltitudeLayer.ItemImportant);
            GenSpawn.Spawn(moteThrown, attachee.PositionHeld, attachee.MapHeld);
            return moteThrown;
        }

        public static void ShowMessageBox(string dialogDesc, string dialogButton)
        {
            Find.WindowStack.Add(new Dialog_MessageBox(dialogDesc, dialogButton));
        }
        
    }
}