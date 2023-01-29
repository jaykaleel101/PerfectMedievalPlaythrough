using UnityEngine;
using Verse;

namespace PMP
{
    public class PMPMod : Mod
    {
        public static PMPSettings settings;
        public PMPMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<PMPSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return this.Content.Name;
        }
    }

    public class PMPSettings : ModSettings
    {
        public bool enableAlternativeApiary;
        public bool enableAlternativeMeatDrying;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableAlternativeApiary, "enableAlternativeApiary");
            Scribe_Values.Look(ref enableAlternativeMeatDrying, "enableAlternativeMeatDrying");
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            var ls = new Listing_Standard();
            ls.Begin(rect);
            ls.CheckboxLabeled("PMP.EnableAlternativeApiary".Translate(), ref enableAlternativeApiary);
            ls.CheckboxLabeled("PMP.EnableAlternativeMeatDrying".Translate(), ref enableAlternativeMeatDrying);
            ls.End();
        }
    }
}
