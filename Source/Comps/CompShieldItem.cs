using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace KaynArmorMod
{
    [StaticConstructorOnStartup]
    public class CompShieldItem : CompEnergyToggleBase
    {
        public float shieldRadius = 3.9f;
        public float energyCostPerAbsorb = 12f;

        private static readonly Material ShieldBubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        protected override void OnActivated()
        {
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld));
        }

        protected override void OnDeactivated()
        {
            SoundDef.Named("EnergyShield_Broken").PlayOneShot(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld));
        }

        protected override void OnActiveTick()
        {
            Pawn pawn = Wearer;
            if (pawn == null || pawn.Map == null || pawn.Dead)
            {
                Toggle(false);
                return;
            }

            // Cada 3 ticks revisamos proyectiles para no sobrecargar la CPU
            if (pawn.IsHashIntervalTick(3))
            {
                InterceptProjectiles(pawn);
            }
        }

        private void InterceptProjectiles(Pawn pawn)
        {
            Vector3 center = pawn.DrawPos;
            List<Thing> projectiles = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile);

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                Projectile proj = projectiles[i] as Projectile;
                if (proj == null || !proj.Spawned) continue;

                // Si el proyectil lo disparó el propio usuario del escudo, se ignora (puede disparar hacia fuera)
                if (proj.Launcher == pawn) continue;

                float dist = Vector3.Distance(proj.ExactPosition, center);
                if (dist <= shieldRadius)
                {
                    // Interceptar el proyectil
                    SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(proj.Position, pawn.Map));
                    FleckMaker.ThrowMicroSparks(proj.ExactPosition, pawn.Map);

                    proj.Destroy(DestroyMode.Vanish);

                    // Consumir energía por el impacto
                    currentEnergy -= energyCostPerAbsorb;
                    if (currentEnergy <= 0f)
                    {
                        currentEnergy = 0f;
                        Toggle(false);
                        break;
                    }
                }
            }
        }

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (isActive && Wearer != null)
            {
                Vector3 drawPos = Wearer.Drawer.DrawPos;
                // Cambiado AltitudeGroup por AltitudeLayer:
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();

                Matrix4x4 matrix = default;
                matrix.SetTRS(drawPos, Quaternion.identity, new Vector3(shieldRadius * 2f, 1f, shieldRadius * 2f));
                Graphics.DrawMesh(MeshPool.plane10, matrix, ShieldBubbleMat, 0);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            Toggle(false);
        }
    }
}