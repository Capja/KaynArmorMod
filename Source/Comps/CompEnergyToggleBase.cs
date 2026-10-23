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

        // Detección universal del portador (arma o ropa) compatible con cualquier versión de RimWorld
        public Pawn Wearer
        {
            get
            {
                if (parent.ParentHolder is Pawn p) return p;
                if (parent.ParentHolder is Pawn_EquipmentTracker eq) return eq.pawn;
                if (parent.ParentHolder is Pawn_ApparelTracker ap) return ap.pawn;
                if (parent.holdingOwner?.Owner is Pawn p2) return p2;
                if (parent.holdingOwner?.Owner is Pawn_EquipmentTracker eq2) return eq2.pawn;
                if (parent.holdingOwner?.Owner is Pawn_ApparelTracker ap2) return ap2.pawn;
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
                // Solo se recarga pasivamente cuando está APAGADO
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

        protected abstract void OnActivated();
        protected abstract void OnDeactivated();
        protected abstract void OnActiveTick();

        public IEnumerable<Gizmo> GetAllGizmos()
        {
            if (Wearer != null && Wearer.Faction == Faction.OfPlayer)
            {
                yield return new Gizmo_EnergyBar(this);

                Texture2D iconTex = ContentFinder<Texture2D>.Get(Props.toggleIconPath, false) 
                                   ?? ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", false) 
                                   ?? BaseContent.BadTex;

                yield return new Command_Toggle
                {
                    defaultLabel = Props.toggleLabel,
                    defaultDesc = $"{Props.toggleDesc}\n\nEnergía: {currentEnergy:F0} / {Props.maxEnergy:F0}",
                    icon = iconTex,
                    isActive = () => isActive,
                    toggleAction = () => Toggle(!isActive)
                };
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in GetAllGizmos()) yield return gizmo;
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (var gizmo in GetAllGizmos()) yield return gizmo;
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            currentEnergy = Props.maxEnergy;
        }

        
    }
}