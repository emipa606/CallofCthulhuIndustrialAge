using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace IndustrialAge.Objects
{
    public class CompBetterRottable : CompRottable
    {
        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            switch (base.Stage)
            {
                case RotStage.Fresh:
                    sb.AppendLine("RotStateFresh".Translate());
                    break;
                case RotStage.Rotting:
                    sb.AppendLine("RotStateRotting".Translate());
                    break;
                case RotStage.Dessicated:
                    sb.AppendLine("RotStateDessicated".Translate());
                    break;
            }
            var num = (float) PropsRot.TicksToRotStart - base.RotProgress;
            if (num > 0f)
            {
                var num2 = GenTemperature.GetTemperatureForCell(parent.PositionHeld, parent.Map);
                List<Thing> thingList = GridsUtility.GetThingList(parent.PositionHeld, parent.Map);
                for (var i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is Building_Refrigerator)
                    {
                        var building_Refrigerator = thingList[i] as Building_Refrigerator;
                        num2 = building_Refrigerator.CurrentTemp;
                        break;
                    }
                }
                num2 = (float) Mathf.RoundToInt(num2);
                var num3 = GenTemperature.RotRateAtTemperature(num2);
                var ticksUntilRotAtCurrentTemp = base.TicksUntilRotAtCurrentTemp;
                if (num3 < 0.001f)
                {
                    sb.Append("CurrentlyFrozen".Translate() + ".");
                }
                else
                {
                    if (num3 < 0.999f)
                    {
                        sb.Append("CurrentlyRefrigerated".Translate(new object[]
                        {
                            ticksUntilRotAtCurrentTemp.ToStringTicksToPeriodVague()
                        }) + ".");
                    }
                    else
                    {
                        sb.Append("NotRefrigerated".Translate(new object[]
                        {
                            ticksUntilRotAtCurrentTemp.ToStringTicksToPeriodVague()
                        }) + ".");
                    }
                }
            }
            return sb.ToString().TrimEndNewlines();
        }

        public override void CompTickRare()
        {
            if (parent.MapHeld != null && parent.Map != null)
            {
                var rotProgress = RotProgress;
                var num = 1f;
                var temperatureForCell =
                    GenTemperature.GetTemperatureForCell(parent.PositionHeld, parent.MapHeld);
                List<Thing> list = parent.MapHeld.thingGrid.ThingsListAtFast(parent.PositionHeld);
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] is Building_Refrigerator)
                    {
                        var bf = list[i] as Building_Refrigerator;
                        temperatureForCell = bf.CurrentTemp;
                        break;
                    }
                }

                num *= GenTemperature.RotRateAtTemperature(temperatureForCell);
                RotProgress += Mathf.Round(num * 250f);
                if (Stage == RotStage.Rotting && PropsRot.rotDestroys)
                {
                    if (parent.IsInAnyStorage() && parent.SpawnedOrAnyParentSpawned)
                    {
                        Messages.Message("MessageRottedAwayInStorage".Translate(new object[]
                            {
                                parent.Label
                            }).CapitalizeFirst(), new TargetInfo(parent.PositionHeld, parent.MapHeld, false),
                            MessageTypeDefOf.NegativeEvent, true);
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.SpoilageAndFreezers,
                            OpportunityType.GoodToKnow);
                    }
                    parent.Destroy(DestroyMode.Vanish);
                    return;
                }
                var flag = Mathf.FloorToInt(rotProgress / 60000f) != Mathf.FloorToInt(RotProgress / 60000f);
                if (flag && ShouldTakeRotDamage())
                {
                    if (Stage == RotStage.Rotting && PropsRot.rotDamagePerDay > 0f)
                    {
                        parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting,
                            (float) GenMath.RoundRandom(PropsRot.rotDamagePerDay), 0f, -1f, null, null, null,
                            DamageInfo.SourceCategory.ThingOrUnknown, null));
                    }
                    else if (Stage == RotStage.Dessicated && PropsRot.dessicatedDamagePerDay > 0f)
                    {
                        parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting,
                            (float) GenMath.RoundRandom(PropsRot.dessicatedDamagePerDay), 0f, -1f, null, null,
                            null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    }
                }
            }
        }

        private bool ShouldTakeRotDamage()
        {
            return !(parent.ParentHolder is Thing thing) || thing.def.category != ThingCategory.Building ||
                   !thing.def.building.preventDeteriorationInside;
        }
    }
}