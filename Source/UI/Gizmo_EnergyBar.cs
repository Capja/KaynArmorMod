using UnityEngine;
using Verse;

namespace KaynArmorMod
{
    [StaticConstructorOnStartup]
    public class Gizmo_EnergyBar : Gizmo
    {
        public CompEnergyToggleBase comp;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.7f, 0.9f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.6f));

        public Gizmo_EnergyBar(CompEnergyToggleBase comp)
        {
            this.comp = comp;
        }

        public override float GetWidth(float maxWidth) => 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(rect);

            Rect contentRect = rect.ContractedBy(6f);
            
            // Texto superior
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(contentRect.x, contentRect.y, contentRect.width, 18f), "Energía del Módulo");

            // Barra de progreso
            Rect barRect = new Rect(contentRect.x, contentRect.y + 22f, contentRect.width, 24f);
            float fillPct = comp.currentEnergy / comp.Props.maxEnergy;
            Widgets.FillableBar(barRect, fillPct, FullBarTex, EmptyBarTex, false);

            // Texto numérico central
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, $"{comp.currentEnergy:F0} / {comp.Props.maxEnergy:F0}");
            Text.Anchor = TextAnchor.UpperLeft;

            // Estado (Recargando / Activo)
            string status = comp.isActive ? "Drenando..." : (comp.currentEnergy >= comp.Props.maxEnergy ? "Cargado" : "Recargando...");
            Widgets.Label(new Rect(contentRect.x, contentRect.y + 48f, contentRect.width, 16f), status);

            return new GizmoResult(GizmoState.Clear);
        }
    }
}