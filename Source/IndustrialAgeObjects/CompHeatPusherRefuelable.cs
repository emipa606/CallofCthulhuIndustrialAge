using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace IndustrialAge.Objects
{
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
                CompRefuelable b = parent.GetComp<CompRefuelable>();
                CompFlickable f = parent.GetComp<CompFlickable>();

                if (f != null && f.SwitchIsOn)
                {
                    if (b != null && b.HasFuel)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(HeatPushInterval) && ShouldPushHeatNow)
            {
                CompProperties_HeatPusher props = Props;
                var temperature = parent.Position.GetTemperature(parent.Map);
                if (temperature < props.heatPushMaxTemperature && temperature > props.heatPushMinTemperature)
                {
                    GenTemperature.PushHeat(parent.Position, parent.Map, props.heatPerSecond);
                }
            }
        }
    }
}
