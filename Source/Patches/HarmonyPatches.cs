using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace KaynArmorMod
{
    // 1. ENTREGA LOS BOTONES DEL RIFLE AL COLONO
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.GetGizmos))]
    public static class Patch_EquipmentTracker_GetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_EquipmentTracker __instance)
        {
            foreach (var g in __result) yield return g;

            if (__instance.Primary != null)
            {
                var comp = __instance.Primary.GetComp<CompEnergyToggleBase>();
                if (comp != null)
                {
                    foreach (var g in comp.GetAllGizmos())
                    {
                        yield return g;
                    }
                }
            }
        }
    }

    // 2. ACTUALIZA LA ENERGÍA DEL RIFLE CADA TICK
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.EquipmentTrackerTick))]
    public static class Patch_EquipmentTracker_Tick
    {
        public static void Postfix(Pawn_EquipmentTracker __instance)
        {
            if (__instance.Primary != null)
            {
                var comp = __instance.Primary.GetComp<CompEnergyToggleBase>();
                comp?.CompTick();
            }
        }
    }

    // 3. INTERCEPTOR FÍSICO DE BALAS PARA EL ESCUDO
    public static class ShieldInterceptorHelper
    {
        public static bool TryIntercept(Projectile proj)
        {
            if (proj == null || !proj.Spawned || proj.Destroyed) return false;

            foreach (var shield in CompShieldItem.ActiveShields)
            {
                if (shield == null || !shield.isActive || shield.Wearer == null || shield.Wearer.Dead || shield.Wearer.Map != proj.Map) 
                    continue;

                // Si el tirador está DENTRO de la cúpula (el portador o un aliado disparando desde dentro),
                // la bala NO se intercepta y sale sin problemas hacia fuera:
                if (proj.Launcher != null && Vector3.Distance(proj.Launcher.DrawPos, shield.Wearer.DrawPos) <= shield.shieldRadius)
                {
                    continue;
                }

                // Si la bala viene desde FUERA y entra al radio del escudo, se desintegra:
                float dist = Vector3.Distance(proj.ExactPosition, shield.Wearer.DrawPos);
                if (dist <= shield.shieldRadius)
                {
                    shield.AbsorbProjectile(proj);
                    return true; // Interceptada
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Projectile), "Tick")]
    public static class Patch_Projectile_Tick
    {
        public static bool Prefix(Projectile __instance)
        {
            // Si entra al escudo en pleno vuelo, se destruye
            return !ShieldInterceptorHelper.TryIntercept(__instance);
        }
    }

    [HarmonyPatch(typeof(Projectile), "ImpactSomething")]
    public static class Patch_Projectile_ImpactSomething
    {
        public static bool Prefix(Projectile __instance)
        {
            // Si va a impactar dentro del radio, se destruye antes de hacer daño
            return !ShieldInterceptorHelper.TryIntercept(__instance);
        }
    }
}