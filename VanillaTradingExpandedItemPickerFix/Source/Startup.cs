using Verse;

namespace VanillaTradingExpandedItemPickerFix
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            ItemPickerPatch.Install();
        }
    }
}
