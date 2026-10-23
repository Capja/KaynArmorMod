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
        public static readonly HashSet<CompShieldItem> ActiveShields = new HashSet<CompShieldItem>();

        public float shieldRadius = 3.9f;
        public float energyCostPerAbsorb = 4f;

        private static readonly Material ShieldBubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        protected override void OnActivated()
        {
            ActiveShields.Add(this);
            SoundDef.Named("EnergyShield_Reset").PlayOneShot(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld));
        }

        protected override void OnDeactivated()
        {
            ActiveShields.Remove(this);
            SoundDef.Named("EnergyShield_Broken").PlayOneShot(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld));
        }



        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            ActiveShields.Remove(this);
            Toggle(false);
        }

        protected override void OnActiveTick()
        {
            if (Wearer == null || Wearer.Dead)
            {
                Toggle(false);
            }
        }

        // Método que ejecuta Harmony al chocar la bala
        public void AbsorbProjectile(Projectile proj)
        {
            SoundDef.Named("EnergyShield_Reset").PlayOneShot(new TargetInfo(proj.Position, Wearer.Map));
            FleckMaker.ThrowMicroSparks(proj.ExactPosition, Wearer.Map);
            proj.Destroy(DestroyMode.Vanish);

            currentEnergy -= energyCostPerAbsorb;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                Toggle(false);
            }
        }

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (isActive && Wearer != null)
            {
                Vector3 drawPos = Wearer.Drawer.DrawPos;
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();

                Matrix4x4 matrix = default;
                matrix.SetTRS(drawPos, Quaternion.identity, new Vector3(shieldRadius * 2f, 1f, shieldRadius * 2f));
                Graphics.DrawMesh(MeshPool.plane10, matrix, ShieldBubbleMat, 0);
            }
        }
    }
}