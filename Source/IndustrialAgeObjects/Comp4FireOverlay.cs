using UnityEngine;
using Verse;

namespace RimWorld
{
    [StaticConstructorOnStartup]
    public class Comp4FireOverlay : ThingCompCandelabra
    {
        private Graphic FireGraphic4;


        public Comp4FireOverlay fireOverlay;
        public float fireSize_fromXML;


        public CompProperties_Fire4Overlay Props => (CompProperties_Fire4Overlay) props;


        public override void PostSpawnSetup(bool bla)
        {
            fireOverlay = parent.TryGetComp<Comp4FireOverlay>();
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

            FireGraphic4 = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Candle",
                ShaderDatabase.TransparentPostLight, drawSize, Color.white);
            FireGraphic4.Draw(drawPos.RotatedBy(parent.Rotation.AsAngle), Rot4.North, parent);
        }
    }
}