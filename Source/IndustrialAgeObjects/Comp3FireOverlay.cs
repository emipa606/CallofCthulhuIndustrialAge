using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Comp3FireOverlay : ThingCompCandelabra
{
    private Graphic FireGraphic3;


    public Comp3FireOverlay fireOverlay;
    public float fireSize_fromXML;

    public CompProperties_Fire3Overlay Props => (CompProperties_Fire3Overlay)props;


    public override void PostSpawnSetup(bool bla)
    {
        fireOverlay = parent.TryGetComp<Comp3FireOverlay>();
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

        FireGraphic3 = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Candle",
            ShaderDatabase.TransparentPostLight, drawSize, Color.white);
        FireGraphic3.Draw(drawPos.RotatedBy(parent.Rotation.AsAngle), Rot4.North, parent);
    }
}