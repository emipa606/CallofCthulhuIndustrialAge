using RimWorld;
using Verse;

namespace IndustrialAge.Objects;

/*
 * 
 * Nandonalt's CompHeatPusherRefuelable
 * 
 */
public class CompHeatPusherRefuelable : ThingComp
{
    private const int HeatPushInterval = 60;

    public CompProperties_HeatPusher Props => (CompProperties_HeatPusher)props;

    protected virtual bool ShouldPushHeatNow
    {
        get
        {
            var b = parent.GetComp<CompRefuelable>();
            var f = parent.GetComp<CompFlickable>();

            if (f is not { SwitchIsOn: true })
            {
                return false;
            }

            return b is { HasFuel: true };
        }
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!parent.IsHashIntervalTick(HeatPushInterval) || !ShouldPushHeatNow)
        {
            return;
        }

        var compPropertiesHeatPusher = Props;
        var temperature = parent.Position.GetTemperature(parent.Map);
        if (temperature < compPropertiesHeatPusher.heatPushMaxTemperature &&
            temperature > compPropertiesHeatPusher.heatPushMinTemperature)
        {
            GenTemperature.PushHeat(parent.Position, parent.Map, compPropertiesHeatPusher.heatPerSecond);
        }
    }
}