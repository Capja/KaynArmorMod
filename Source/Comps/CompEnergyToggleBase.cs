using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace KaynArmorMod
{
    public abstract class CompEnergyToggleBase : ThingComp
    {
        public CompProperties_EnergyToggle Props => (CompProperties_EnergyToggle)props;

        public float currentEnergy;
        public bool isActive;

        public Pawn Wearer
        {
            get
            {
                if (parent.ParentHolder is Pawn_EquipmentTracker eq) return eq.pawn;
                if (parent.ParentHolder is Pawn_ApparelTracker ap) return ap.pawn;
                return null;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                currentEnergy = Props.maxEnergy;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentEnergy, "currentEnergy", 0f);
            Scribe_Values.Look(ref isActive, "isActive", false);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (isActive)
            {
                currentEnergy -= Props.energyDrainRatePerTick;
                OnActiveTick();

                if (currentEnergy <= 0f)
                {
                    currentEnergy = 0f;
                    Toggle(false);
                }
            }
            else
            {
                if (currentEnergy < Props.maxEnergy)
                {
                    currentEnergy = Mathf.Min(Props.maxEnergy, currentEnergy + Props.energyRechargeRatePerTick);
                }
            }
        }

        public virtual void Toggle(bool state)
        {
            if (state && currentEnergy <= 0f) return;

            isActive = state;
            if (isActive) OnActivated();
            else OnDeactivated();
        }

        // Métodos que implementará cada habilidad específica
        protected abstract void OnActivated();
        protected abstract void OnDeactivated();
        protected abstract void OnActiveTick();

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Wearer != null && Wearer.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "Activar / Desactivar",
                    defaultDesc = $"Activa o desactiva la función. Energía: {currentEnergy:F0}/{Props.maxEnergy:F0}",
                    icon = ContentFinder<Texture2D>.Get(Props.toggleIconPath, true),
                    isActive = () => isActive,
                    toggleAction = () => Toggle(!isActive)
                };
            }
            yield return new Gizmo_EnergyBar(this);
        }
    }
}