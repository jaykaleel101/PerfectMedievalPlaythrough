using RimWorld;
using Verse;
using Verse.AI;

namespace PMP
{
    [DefOf]
    public static class PMP_DefOf
    {
        static PMP_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PMP_DefOf));
        }

        // Apiary
        public static ThingDef DankPyon_Apiary;
        public static JobDef PMP_TakeHoneyOutOfApiary;
        public static JobDef PMP_TendToApiary;
        public static HediffDef PMP_Sting;
        public static ThoughtDef PMP_StingMoodDebuff;
        public static ThingDef DankPyon_Honeycomb;
        public static DamageDef PMP_DamageSting;

        // Medieval Overhaul compatibility
        public static ThingDef DankPyon_MedievalSmithy;
        public static ThingDef DankPyon_MedievalStove;
        
        // [Optional] Add null-check helpers
        public static bool ApiaryExists => DankPyon_Apiary != null;
        public static bool MedievalSmithyExists => DankPyon_MedievalSmithy != null;
    }
}
