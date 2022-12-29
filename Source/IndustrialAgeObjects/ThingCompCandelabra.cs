using Verse;

namespace RimWorld;

/// <summary>
///     A nested override.
/// </summary>
public class ThingCompCandelabra : ThingComp
{
    private bool fireOnInt;

    public bool ShouldBeFireNow
    {
        get
        {
            if (!parent.Spawned)
            {
                return false;
            }

            var compRefuelable = parent.TryGetComp<CompRefuelable>();
            if (compRefuelable is { HasFuel: false })
            {
                return false;
            }

            var compFlickable = parent.TryGetComp<CompFlickable>();
            return compFlickable == null || compFlickable.SwitchIsOn;
        }
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref fireOnInt, "fireOn");
    }
}