using Verse;

namespace KaynArmorMod
{
    public class CompProperties_EnergyToggle : CompProperties
    {
        public float maxEnergy = 100f;
        public float energyRechargeRatePerTick = 0.05f; // Energía recuperada por tick sin usar
        public float energyDrainRatePerTick = 0.2f;     // Energía gastada por tick activo
        public string toggleIconPath = "UI/Commands/ToggleAbility"; // Textura del botón

        public CompProperties_EnergyToggle()
        {
            compClass = typeof(CompEnergyToggleBase);
        }
    }
}