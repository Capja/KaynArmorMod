using HarmonyLib;
using Verse;

namespace KaynArmorMod
{
    [StaticConstructorOnStartup]
    public static class KaynArmorModInit
    {
        static KaynArmorModInit()
        {
            var harmony = new Harmony("Kayn.KaynArmorMod");
            harmony.PatchAll();
            Log.Message("[KaynArmorMod] Harmony cargado con éxito.");
        }
    }
}