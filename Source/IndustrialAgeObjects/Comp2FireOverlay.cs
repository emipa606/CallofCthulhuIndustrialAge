using UnityEngine;
using Verse;

namespace RimWorld
{
    [StaticConstructorOnStartup]
    public class Comp2FireOverlay : ThingCompCandelabra
    {
        private Graphic FireGraphic2;


        public Comp2FireOverlay fireOverlay;
        public float fireSize_fromXML;


        public CompProperties_Fire2Overlay Props => (CompProperties_Fire2Overlay) props;

        public override void PostSpawnSetup(bool bla)
        {
            fireOverlay = parent.TryGetComp<Comp2FireOverlay>();
        }

        public override void PostDraw()
        {
            base.PostDraw();
            var drawPos = parent.DrawPos;
            var drawSize = new Vector2(fireOverlay.Props.flameSize, fireOverlay.Props.flameSize);

            drawPos += fireOverlay.Props.offset;

            if (!ShouldBeFireNow)
            {
                return;
            }

            FireGraphic2 = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Candle",
                ShaderDatabase.TransparentPostLight, drawSize, Color.white);
            FireGraphic2.Draw(drawPos.RotatedBy(parent.Rotation.AsAngle), Rot4.North, parent);
        }
    }
}