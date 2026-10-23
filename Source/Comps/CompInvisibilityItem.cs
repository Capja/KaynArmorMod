using RimWorld;
using Verse;

namespace KaynArmorMod
{
    public class CompInvisibilityItem : CompEnergyToggleBase
    {
        private Hediff invisibilityHediff;
        public bool breakOnAttack = false;

        // Obtiene el Hediff de forma segura sin requerir DLCs obligatorios
        private static HediffDef InvisDef => DefDatabase<HediffDef>.GetNamedSilentFail("Kayn_Invisibilidad") 
                                          ?? DefDatabase<HediffDef>.GetNamedSilentFail("PsychicInvisibility")
                                          ?? DefDatabase<HediffDef>.GetNamedSilentFail("Invisibility");

        protected override void OnActivated()
        {
            Pawn pawn = Wearer;
            if (pawn == null || pawn.Dead) return;

            if (InvisDef != null)
            {
                invisibilityHediff = pawn.health.GetOrAddHediff(InvisDef);
            }
            FleckMaker.ThrowDustPuffThick(pawn.Position.ToVector3Shifted(), pawn.Map, 1.2f, UnityEngine.Color.cyan);
        }

        protected override void OnDeactivated()
        {
            Pawn pawn = Wearer;
            if (pawn != null && invisibilityHediff != null)
            {
                pawn.health.RemoveHediff(invisibilityHediff);
                invisibilityHediff = null;
                if (pawn.Map != null)
                    FleckMaker.ThrowDustPuffThick(pawn.Position.ToVector3Shifted(), pawn.Map, 1.0f, UnityEngine.Color.gray);
            }
        }

        protected override void OnActiveTick()
        {
            Pawn pawn = Wearer;
            if (pawn == null || pawn.Dead)
            {
                Toggle(false);
                return;
            }

            // Si el colono dispara o golpea cuerpo a cuerpo, la invisibilidad se apaga por romper el sigilo
            if (breakOnAttack && pawn.stances?.curStance is Stance_Cooldown cooldown && cooldown.ticksLeft > 10)
            {
                Messages.Message($"{pawn.NameShortColored} rompió el camuflaje al atacar.", pawn, MessageTypeDefOf.CautionInput);
                Toggle(false);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            Toggle(false);
        }
    }
}