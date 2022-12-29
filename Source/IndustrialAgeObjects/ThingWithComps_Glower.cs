using RimWorld;
using Verse;

namespace IndustrialAge.Objects;

internal class ThingWithComps_Glower : ThingWithComps
{
    public Building_StreetLamp master;

    public override void Draw()
    {
    }

    public override void Tick()
    {
        base.Tick();
        CheckNeedsDestruction();
        CheckNeedsFlick();
    }

    public void CheckNeedsDestruction()
    {
        if (master == null || !Spawned)
        {
            return;
        }

        if (!master.Spawned)
        {
            Destroy();
        }
    }

    public void CheckNeedsFlick()
    {
        if (master == null)
        {
            return;
        }

        var masterflickable = master.TryGetComp<CompFlickable>();
        var flickable = this.TryGetComp<CompFlickable>();

        if (masterflickable.SwitchIsOn != flickable.SwitchIsOn)
        {
            flickable.DoFlick();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref master, "master");
    }
}