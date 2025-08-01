using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace IndustrialAge.Objects;

public class CompBetterRottable : CompRottable
{
    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        switch (Stage)
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

        var num = PropsRot.TicksToRotStart - RotProgress;
        if (!(num > 0f))
        {
            return sb.ToString().TrimEndNewlines();
        }

        var num2 = GenTemperature.GetTemperatureForCell(parent.PositionHeld, parent.Map);
        var thingList = parent.PositionHeld.GetThingList(parent.Map);
        foreach (var thing in thingList)
        {
            if (thing is not Building_Refrigerator buildingRefrigerator)
            {
                continue;
            }

            num2 = buildingRefrigerator.CurrentTemp;
            break;
        }

        num2 = Mathf.RoundToInt(num2);
        var num3 = GenTemperature.RotRateAtTemperature(num2);
        var ticksUntilRotAtCurrentTemp = TicksUntilRotAtCurrentTemp;
        if (num3 < 0.001f)
        {
            sb.Append("CurrentlyFrozen".Translate() + ".");
        }
        else
        {
            if (num3 < 0.999f)
            {
                sb.Append(
                    "CurrentlyRefrigerated".Translate(ticksUntilRotAtCurrentTemp.ToStringTicksToPeriodVague()) +
                    ".");
            }
            else
            {
                sb.Append(
                    "NotRefrigerated".Translate(ticksUntilRotAtCurrentTemp.ToStringTicksToPeriodVague()) + ".");
            }
        }

        return sb.ToString().TrimEndNewlines();
    }

    public override void CompTickRare()
    {
        if (parent.MapHeld == null || parent.Map == null)
        {
            return;
        }

        var rotProgress = RotProgress;
        var num = 1f;
        var temperatureForCell =
            GenTemperature.GetTemperatureForCell(parent.PositionHeld, parent.MapHeld);
        var list = parent.MapHeld.thingGrid.ThingsListAtFast(parent.PositionHeld);
        foreach (var thing in list)
        {
            if (thing is not Building_Refrigerator bf)
            {
                continue;
            }

            temperatureForCell = bf.CurrentTemp;
            break;
        }

        num *= GenTemperature.RotRateAtTemperature(temperatureForCell);
        RotProgress += Mathf.Round(num * 250f);
        if (Stage == RotStage.Rotting && PropsRot.rotDestroys)
        {
            if (parent.IsInAnyStorage() && parent.SpawnedOrAnyParentSpawned)
            {
                Messages.Message("MessageRottedAwayInStorage".Translate(parent.Label).CapitalizeFirst(),
                    new TargetInfo(parent.PositionHeld, parent.MapHeld),
                    MessageTypeDefOf.NegativeEvent);
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.SpoilageAndFreezers,
                    OpportunityType.GoodToKnow);
            }

            parent.Destroy();
            return;
        }

        if (Mathf.FloorToInt(rotProgress / 60000f) == Mathf.FloorToInt(RotProgress / 60000f) ||
            !shouldTakeRotDamage())
        {
            return;
        }

        switch (Stage)
        {
            case RotStage.Rotting when PropsRot.rotDamagePerDay > 0f:
                parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting,
                    GenMath.RoundRandom(PropsRot.rotDamagePerDay)));
                break;
            case RotStage.Dessicated when PropsRot.dessicatedDamagePerDay > 0f:
                parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting,
                    GenMath.RoundRandom(PropsRot.dessicatedDamagePerDay)));
                break;
        }
    }

    private bool shouldTakeRotDamage()
    {
        return parent.ParentHolder is not Thing thing || thing.def.category != ThingCategory.Building ||
               !thing.def.building.preventDeteriorationInside;
    }
}