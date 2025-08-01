using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace IndustrialAge.Objects;

public class Building_Refrigerator : Building_Storage, IStoreSettingsParent
{
    private const float IdealTempDefault = -10f;


    private const float lowPowerConsumptionFactor = 0.1f;
    private const float temperatureChangeRate = 0.116923077f;
    private const float energyPerSecond = 12f;

    private float currentTemp = float.MinValue;
    private StorageSettings curStorageSettings;
    private float idealTemp = float.MinValue;
    private bool operatingAtHighPower;

    private CompPowerTrader PowerTrader { get; set; }

    private CompGlower Glower { get; set; }

    public float IdealTemp
    {
        get
        {
            if (idealTemp == float.MinValue)
            {
                idealTemp = IdealTempDefault;
            }

            return idealTemp;
        }
        set => idealTemp = value;
    }

    public float CurrentTemp
    {
        get
        {
            if (currentTemp == float.MinValue)
            {
                currentTemp = PositionHeld.GetTemperature(MapHeld);
            }

            return currentTemp;
        }
        private set => currentTemp = value;
    }

    private float BasePowerConsumption => -PowerTrader.Props.basePowerConsumption;

    StorageSettings IStoreSettingsParent.GetParentStoreSettings()
    {
        return curStorageSettings;
    }

    public override void SpawnSetup(Map map, bool bla)
    {
        base.SpawnSetup(map, bla);
        PowerTrader = GetComp<CompPowerTrader>();
        Glower = GetComp<CompGlower>();
        curStorageSettings = new StorageSettings();
        curStorageSettings.CopyFrom(def.building.fixedStorageSettings);
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.HasComp(typeof(CompRottable))))
        {
            if (!curStorageSettings.filter.Allows(thingDef))
            {
                curStorageSettings.filter.SetAllow(thingDef, true);
            }
        }
    }

    public override void TickRare()
    {
        base.TickRare();
        MakeAllHeldThingsBetterCompRottable();
        ResolveTemperature();
    }

    private void ResolveTemperature()
    {
        if (!Spawned || PowerTrader is not { PowerOn: true })
        {
            EqualizeWithRoomTemperature();
            return;
        }

        Glower.UpdateLit(MapHeld);

        var intVec = PositionHeld;
        var moddedTemperatureChangeRate = temperatureChangeRate;
        var energyLimit = energyPerSecond * moddedTemperatureChangeRate * 4.16666651f;
        var usingHighPower = IsUsingHighPower(energyLimit, out var energyUsed);
        if (usingHighPower)
        {
            GenTemperature.PushHeat(intVec, MapHeld, -energyLimit * 1.25f);
            energyUsed += BasePowerConsumption;
            moddedTemperatureChangeRate *= 0.8f;
        }
        else
        {
            energyUsed =
                BasePowerConsumption * lowPowerConsumptionFactor;
            moddedTemperatureChangeRate *= 1.1f;
        }

        if (!Mathf.Approximately(CurrentTemp, IdealTemp))
        {
            CurrentTemp += CurrentTemp > IdealTemp ? -moddedTemperatureChangeRate : moddedTemperatureChangeRate;
        }

        if (CurrentTemp.ToStringTemperature("F0") == IdealTemp.ToStringTemperature("F0"))
        {
            usingHighPower = false;
        }

        operatingAtHighPower = usingHighPower;
        PowerTrader.PowerOutput = energyUsed;
    }

    private void EqualizeWithRoomTemperature()
    {
        var roomTemperature = PositionHeld.GetTemperature(MapHeld);
        if (CurrentTemp > roomTemperature)
        {
            CurrentTemp += -temperatureChangeRate;
        }
        else if (CurrentTemp < roomTemperature)
        {
            CurrentTemp += temperatureChangeRate;
        }
    }

    private bool IsUsingHighPower(float energyLimit, out float energyUsed)
    {
        var a = IdealTemp - CurrentTemp;
        if (energyLimit > 0f)
        {
            energyUsed = Mathf.Min(a, energyLimit);
            energyUsed = Mathf.Max(energyUsed, 0f);
        }
        else
        {
            energyUsed = Mathf.Max(a, energyLimit);
            energyUsed = Mathf.Min(energyUsed, 0f);
        }

        return Mathf.Approximately(energyUsed, 0f);
    }

    private void MakeAllHeldThingsBetterCompRottable()
    {
        foreach (var thing in PositionHeld.GetThingList(Map))
        {
            if (thing is not ThingWithComps thingWithComps)
            {
                continue;
            }

            var rottable = thing.TryGetComp<CompRottable>();
            if (rottable == null || rottable is CompBetterRottable)
            {
                continue;
            }

            var newRot = new CompBetterRottable();
            thingWithComps.AllComps.Remove(rottable);
            thingWithComps.AllComps.Add(newRot);
            newRot.props = rottable.props;
            newRot.parent = thingWithComps;
            newRot.RotProgress = rottable.RotProgress;
        }
    }


    private float RoundedToCurrentTempModeOffset(float celsiusTemp)
    {
        var num = GenTemperature.CelsiusToOffset(celsiusTemp, Prefs.TemperatureMode);
        num = Mathf.RoundToInt(num);
        return GenTemperature.ConvertTemperatureOffset(num, Prefs.TemperatureMode, TemperatureDisplayMode.Celsius);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var c in base.GetGizmos())
        {
            yield return c;
        }

        var offset2 = RoundedToCurrentTempModeOffset(-10f);
        yield return new Command_Action
        {
            action = delegate { InterfaceChangeTargetTemperature(offset2); },
            defaultLabel = offset2.ToStringTemperatureOffset("F0"),
            defaultDesc = "CommandLowerTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc5,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower")
        };
        var offset3 = RoundedToCurrentTempModeOffset(-1f);
        yield return new Command_Action
        {
            action = delegate { InterfaceChangeTargetTemperature(offset3); },
            defaultLabel = offset3.ToStringTemperatureOffset("F0"),
            defaultDesc = "CommandLowerTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc4,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower")
        };
        yield return new Command_Action
        {
            action = delegate
            {
                idealTemp = 21f;
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                ThrowCurrentTemperatureText();
            },
            defaultLabel = "CommandResetTemp".Translate(),
            defaultDesc = "CommandResetTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc1,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/TempReset")
        };
        var offset4 = RoundedToCurrentTempModeOffset(1f);
        yield return new Command_Action
        {
            action = delegate { InterfaceChangeTargetTemperature(offset4); },
            defaultLabel = $"+{offset4.ToStringTemperatureOffset("F0")}",
            defaultDesc = "CommandRaiseTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc2,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise")
        };
        var offset = RoundedToCurrentTempModeOffset(10f);
        yield return new Command_Action
        {
            action = delegate { InterfaceChangeTargetTemperature(offset); },
            defaultLabel = $"+{offset.ToStringTemperatureOffset("F0")}",
            defaultDesc = "CommandRaiseTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc3,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise")
        };
    }

    private void InterfaceChangeTargetTemperature(float offset)
    {
        if (offset > 0f)
        {
            SoundDefOf.Designate_PlanAdd.PlayOneShotOnCamera();
        }
        else
        {
            SoundDefOf.Designate_PlanRemove.PlayOneShotOnCamera();
        }

        idealTemp += offset;
        idealTemp = Mathf.Clamp(idealTemp, -270f, 2000f);
        ThrowCurrentTemperatureText();
    }

    private void ThrowCurrentTemperatureText()
    {
        MoteMaker.ThrowText(this.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), MapHeld,
            idealTemp.ToStringTemperature("F0"), Color.white);
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("Temperature".Translate() + ": ");
        stringBuilder.AppendLine(CurrentTemp.ToStringTemperature("F0"));
        stringBuilder.Append("TargetTemperature".Translate() + ": ");
        stringBuilder.AppendLine(IdealTemp.ToStringTemperature("F0"));
        stringBuilder.Append("PowerConsumptionMode".Translate() + ": ");
        stringBuilder.Append(operatingAtHighPower
            ? "PowerConsumptionHigh".Translate()
            : "PowerConsumptionLow".Translate());

        return stringBuilder.ToString();
    }


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref currentTemp, "currentTemp", float.MinValue);
        Scribe_Values.Look(ref idealTemp, "idealTemp", float.MinValue);
    }
}