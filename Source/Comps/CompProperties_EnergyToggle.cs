using Verse;

namespace KaynArmorMod
{
    public class CompProperties_EnergyToggle : CompProperties
    {
        public float maxEnergy = 100f;
        public float energyRechargeRatePerTick = 0.05f;
        public float energyDrainRatePerTick = 0.15f;
        
        public string toggleLabel = "Habilidad";
        public string toggleDesc = "Activa o desactiva la función.";
        public string toggleIconPath = "UI/Abilities/Invisibility";

        public CompProperties_EnergyToggle()
        {
            compClass = typeof(CompEnergyToggleBase);
        }
    }
}