using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cthulhu;

public static class Utility
{
    public enum SanLossSev
    {
        None = 0,
        Hidden,
        Initial,
        Minor,
        Major,
        Extreme
    }

    public const string SanityLossDef = "ROM_SanityLoss";
    public const string AltSanityLossDef = "Cults_SanityLoss";

    public static bool modCheck;
    public static bool loadedCosmicHorrors;
    public static bool loadedIndustrialAge;
    public static bool loadedCults;
    public static bool loadedFactions;

    public static string Prefix => $"{ModProps.main} :: {ModProps.mod} {ModProps.version} :: ";


    public static bool IsMorning(Map map)
    {
        return GenLocalDate.HourInteger(map) > 6 && GenLocalDate.HourInteger(map) < 10;
    }

    public static bool IsEvening(Map map)
    {
        return GenLocalDate.HourInteger(map) > 18 && GenLocalDate.HourInteger(map) < 22;
    }

    public static bool IsNight(Map map)
    {
        return GenLocalDate.HourInteger(map) > 22;
    }

    public static T GetMod<T>(string s) where T : Mod
    {
        //Call of Cthulhu - Cosmic Horrors
        T result = default;
        foreach (var ResolvedMod in LoadedModManager.ModHandles)
        {
            if (ResolvedMod.Content.Name == s)
            {
                result = ResolvedMod as T;
            }
        }

        return result;
    }

    public static bool IsCosmicHorror(Pawn thing)
    {
        if (!IsCosmicHorrorsLoaded())
        {
            return false;
        }

        var type = Type.GetType("CosmicHorror.CosmicHorrorPawn");
        if (type == null)
        {
            return false;
        }

        if (thing.GetType() == type)
        {
            return true;
        }

        return false;
    }

    //public static float GetSanityLossRate(PawnKindDef kindDef)
    //{
    //    float sanityLossRate = 0f;
    //    if (kindDef.ToString() == "ROM_StarVampire")
    //        sanityLossRate = 0.04f;
    //    if (kindDef.ToString() == "StarSpawnOfCthulhu")
    //        sanityLossRate = 0.02f;
    //    if (kindDef.ToString() == "DarkYoung")
    //        sanityLossRate = 0.004f;
    //    if (kindDef.ToString() == "DeepOne")
    //        sanityLossRate = 0.008f;
    //    if (kindDef.ToString() == "DeepOneGreat")
    //        sanityLossRate = 0.012f;
    //    if (kindDef.ToString() == "MiGo")
    //        sanityLossRate = 0.008f;
    //    if (kindDef.ToString() == "Shoggoth")
    //        sanityLossRate = 0.012f;
    //    return sanityLossRate;
    //}

    public static bool CapableOfViolence(Pawn pawn, bool allowDowned = false)
    {
        if (pawn == null)
        {
            return false;
        }

        if (pawn.Dead)
        {
            return false;
        }

        if (pawn.Downed && !allowDowned)
        {
            return false;
        }

        return !pawn.story.DisabledWorkTagsBackstoryAndTraits.OverlapsWithOnAnyWorkType(WorkTags.Violent);
    }

    public static bool IsActorAvailable(Pawn preacher, bool downedAllowed = false)
    {
        var s = new StringBuilder();
        s.Append("ActorAvailble Checks Initiated");
        s.AppendLine();
        if (preacher == null)
        {
            return ResultFalseWithReport(s);
        }

        s.Append("ActorAvailble: Passed null Check");
        s.AppendLine();
        //if (!preacher.Spawned)
        //    return ResultFalseWithReport(s);
        //s.Append("ActorAvailble: Passed not-spawned check");
        //s.AppendLine();
        if (preacher.Dead)
        {
            return ResultFalseWithReport(s);
        }

        s.Append("ActorAvailble: Passed not-dead");
        s.AppendLine();
        if (preacher.Downed && !downedAllowed)
        {
            return ResultFalseWithReport(s);
        }

        s.Append($"ActorAvailble: Passed downed check & downedAllowed = {downedAllowed}");
        s.AppendLine();
        if (preacher.Drafted)
        {
            return ResultFalseWithReport(s);
        }

        s.Append("ActorAvailble: Passed drafted check");
        s.AppendLine();
        if (preacher.InAggroMentalState)
        {
            return ResultFalseWithReport(s);
        }

        s.Append("ActorAvailble: Passed drafted check");
        s.AppendLine();
        if (preacher.InMentalState)
        {
            return ResultFalseWithReport(s);
        }

        s.Append("ActorAvailble: Passed InMentalState check");
        s.AppendLine();
        s.Append("ActorAvailble Checks Passed");
        DebugReport(s.ToString());
        return true;
    }

    public static bool ResultFalseWithReport(StringBuilder s)
    {
        s.Append("ActorAvailble: Result = Unavailable");
        DebugReport(s.ToString());
        return false;
    }

    public static float CurrentSanityLoss(Pawn pawn)
    {
        var sanityLossDef = AltSanityLossDef;
        if (IsCosmicHorrorsLoaded())
        {
            sanityLossDef = SanityLossDef;
        }

        var pawnSanityHediff =
            pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(sanityLossDef));
        return pawnSanityHediff?.Severity ?? 0f;
    }


    public static void ApplyTaleDef(string defName, Map map)
    {
        var randomPawn = map.mapPawns.FreeColonists.RandomElement();
        var taleToAdd = TaleDef.Named(defName);
        TaleRecorder.RecordTale(taleToAdd, randomPawn);
    }

    public static void ApplyTaleDef(string defName, Pawn pawn)
    {
        var taleToAdd = TaleDef.Named(defName);
        if ((pawn.IsColonist || pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
        {
            TaleRecorder.RecordTale(taleToAdd, pawn);
        }
    }


    public static bool HasSanityLoss(Pawn pawn)
    {
        var sanityLossDef = !IsCosmicHorrorsLoaded() ? AltSanityLossDef : SanityLossDef;
        var pawnSanityHediff =
            pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(sanityLossDef));

        return pawnSanityHediff != null;
    }

    public static void ApplySanityLoss(Pawn pawn, float sanityLoss = 0.3f, float sanityLossMax = 1.0f)
    {
        if (pawn == null)
        {
            return;
        }

        var sanityLossDef = !IsCosmicHorrorsLoaded() ? AltSanityLossDef : SanityLossDef;

        var pawnSanityHediff =
            pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(sanityLossDef));
        if (pawnSanityHediff != null)
        {
            if (pawnSanityHediff.Severity > sanityLossMax)
            {
                sanityLossMax = pawnSanityHediff.Severity;
            }

            var result = pawnSanityHediff.Severity;
            result += sanityLoss;
            result = Mathf.Clamp(result, 0.0f, sanityLossMax);
            pawnSanityHediff.Severity = result;
        }
        else if (sanityLoss > 0)
        {
            var sanityLossHediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed(sanityLossDef), pawn);

            sanityLossHediff.Severity = sanityLoss;
            pawn.health.AddHediff(sanityLossHediff);
        }
    }


    public static int GetSocialSkill(Pawn p)
    {
        return p.skills.GetSkill(SkillDefOf.Social).Level;
    }

    public static int GetResearchSkill(Pawn p)
    {
        return p.skills.GetSkill(SkillDefOf.Intellectual).Level;
    }

    public static bool IsCosmicHorrorsLoaded()
    {
        if (!modCheck)
        {
            ModCheck();
        }

        return loadedCosmicHorrors;
    }


    public static bool IsIndustrialAgeLoaded()
    {
        if (!modCheck)
        {
            ModCheck();
        }

        return loadedIndustrialAge;
    }


    public static bool IsCultsLoaded()
    {
        if (!modCheck)
        {
            ModCheck();
        }

        return loadedCults;
    }

    public static bool IsRandomWalkable8WayAdjacentOf(IntVec3 cell, Map map, out IntVec3 resultCell)
    {
        if (cell != IntVec3.Invalid)
        {
            _ = cell.RandomAdjacentCell8Way();
            if (map != null)
            {
                for (var i = 0; i < 100; i++)
                {
                    var temp = cell.RandomAdjacentCell8Way();
                    if (!temp.Walkable(map))
                    {
                        continue;
                    }

                    resultCell = temp;
                    return true;
                }
            }
        }

        resultCell = IntVec3.Invalid;
        return false;
    }

    public static void ModCheck()
    {
        loadedCosmicHorrors = false;
        loadedIndustrialAge = false;
        foreach (var ResolvedMod in LoadedModManager.RunningMods)
        {
            if (loadedCosmicHorrors && loadedIndustrialAge && loadedCults)
            {
                break; //Save some loading
            }

            if (ResolvedMod.Name.Contains("Call of Cthulhu - Cosmic Horrors"))
            {
                DebugReport("Loaded - Call of Cthulhu - Cosmic Horrors");
                loadedCosmicHorrors = true;
            }

            if (ResolvedMod.Name.Contains("Call of Cthulhu - Industrial Age"))
            {
                DebugReport("Loaded - Call of Cthulhu - Industrial Age");
                loadedIndustrialAge = true;
            }

            if (ResolvedMod.Name.Contains("Call of Cthulhu - Cults"))
            {
                DebugReport("Loaded - Call of Cthulhu - Cults");
                loadedCults = true;
            }

            if (ResolvedMod.Name.Contains("Call of Cthulhu - Factions"))
            {
                DebugReport("Loaded - Call of Cthulhu - Factions");
                loadedFactions = true;
            }
        }

        modCheck = true;
    }

    public static void DebugReport(string x)
    {
        if (Prefs.DevMode && DebugSettings.godMode)
        {
            Log.Message(Prefix + x);
        }
    }

    public static void ErrorReport(string x)
    {
        Log.Error(Prefix + x);
    }
}