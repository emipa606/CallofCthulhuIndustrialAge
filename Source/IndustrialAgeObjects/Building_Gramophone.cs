// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace IndustrialAge.Objects;

/// <summary>
///     This is the main class for the Gramophone.
///     Major coding credits go to mrofa and Haplo.
///     I am but an amateur working on the shoulders of
///     giants.
/// </summary>
/// <author>Jecrell</author>
/// <permission>Free to use by all.</permission>
public class Building_Gramophone : Building
{
    //Is our music player on or off?
    public enum State
    {
        off = 0,
        on
    }

    private const float ListenRadius = 7.9f;

    // Variables to set a specific value
    private const int counterWoundMax = 20000; // Active-Time
    private static List<IntVec3> listenableCells = [];

    private readonly SoundDef songCharleston = SoundDef.Named("Estate_GS_Charleston");
    private readonly SoundDef songInTheMood = SoundDef.Named("Estate_GS_InTheMood");
    private readonly SoundDef songKingPorterStomp = SoundDef.Named("Estate_GS_KingPorterStomp");
    private readonly WorldComponent_Tunes tuneScape = Find.World.GetComponent<WorldComponent_Tunes>();
    private readonly string txtOff = "CCIA.Off".Translate();
    private readonly string txtOn = "CCIA.On".Translate();


    private readonly string txtPlaying = "CCIA.NowPlaying".Translate();

    // Component references (will be set in 'SpawnSetup()')
    // CompMusicPlayer  - This makes it possible for your building to play music. You can start and stop the music.
    //private CompMusicPlayer musicComp;

    private readonly string txtStatus = "CCIA.Status".Translate();

    private bool autoPlay;
    // ===================== Variables =====================

    // Work variable
    private int counter; // 60Ticks = 1s // 20000Ticks = 1 Day
    private TuneDef currentTuneDef;

    // Destroyed flag. Most of the time not really needed, but sometimes...
    private bool destroyedFlag;
    private float duration = -1f;
    public bool isRadio;
    private TuneDef nextTuneDef;

    protected Sustainer playingSong;
    private List<TuneDef> playlist = [];
    private CompPowerTrader powerTrader;
    private TuneDef prevTuneDef;
    private int rareTickWorker = 250;
    private State state = State.off; // Actual phase
    private State stateOld = State.on; // Save-variable

    public TuneDef CurrentTune
    {
        get => currentTuneDef;
        set => currentTuneDef = value;
    }

    public TuneDef NextTune
    {
        get => nextTuneDef;
        set => nextTuneDef = value;
    }

    public TuneDef PreviousTune
    {
        get => prevTuneDef;
        set => prevTuneDef = value;
    }

    public State CurrentState
    {
        get => state;
        set => state = value;
    }


    public IEnumerable<IntVec3> ListenableCells => ListenableCellsAround(Position, Map);

    public bool IsOn()
    {
        return state == State.on;
    }

    /// <summary>
    ///     Do something after the object is spawned
    /// </summary>
    public override void SpawnSetup(Map map, bool flip)
    {
        // Do the work of the base class (Building)
        base.SpawnSetup(map, flip);

        // Get refferences to the components CompPowerTrader and CompGlower
        //SetMusicPlayer();
        listenableCells = ListenableCellsAround(Position, map);
    }

    /// <summary>
    ///     To save and load actual values (savegame-data)
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        // Save and load the work variables, so they don't default after loading
        Scribe_Values.Look(ref isRadio, "isRadio");
        Scribe_Values.Look(ref autoPlay, "autoPlay");
        Scribe_Values.Look(ref state, "state");
        Scribe_Values.Look(ref counter, "counter");
        Scribe_Defs.Look(ref prevTuneDef, "prevTuneDef");
        Scribe_Defs.Look(ref currentTuneDef, "currentTuneDef");
        Scribe_Defs.Look(ref nextTuneDef, "nextTuneDef");
        Scribe_Collections.Look(ref playlist, "playlist", LookMode.Def);

        // Set the old value to the phase value
        stateOld = state;

        // Get refferences to the components CompPowerTrader and CompGlower
        //SetMusicPlayer();
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        autoPlay = false;
        StopMusic();
    }


    // ===================== Destroy =====================

    /// <summary>
    ///     Clean up when this is destroyed
    /// </summary>
    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        StopMusic();
        // block further ticker work
        destroyedFlag = true;

        base.Destroy(mode);
    }

    // ===================== Inspections =====================

    /// <summary>
    ///     This string will be shown when the object is selected (focus)
    /// </summary>
    /// <returns></returns>
    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();

        // Add the inspections string from the base
        if (base.GetInspectString() != "")
        {
            stringBuilder.Append(base.GetInspectString());
        }

        // Add your own strings (caution: string shouldn't be more than 5 lines (including base)!)
        //stringBuilder.Append("Power output: " + powerComp.powerOutput + " W");
        //stringBuilder.AppendLine();
        if (stringBuilder.Length > 0)
        {
            stringBuilder.AppendLine();
        }

        stringBuilder.Append($"{txtStatus} "); // <= TRANSLATION

        // State -> Off: Add text 'Off' (Translation from active language file)
        if (state == State.off)
        {
            stringBuilder.Append(txtOff); // <= TRANSLATION
        }

        // State -> On: Add text 'On' (Translation from active language file)
        if (state == State.on)
        {
            stringBuilder.Append(txtOn); // <= TRANSLATION

            stringBuilder.AppendLine();
            stringBuilder.Append($"{txtPlaying} ");

            stringBuilder.Append(currentTuneDef);
        }

        // return the complete string
        var result = stringBuilder.ToString().TrimEndNewlines();
        return result;
    }


    // ===================== Pawn Actions =====================

    //Pawn-activated music event
    public virtual void PlayMusic(Pawn activator)
    {
        //No one there? We can't start without an audience.
        if (activator == null)
        {
            return;
        }

        //If there is no tuneDef set to play, then let's randomly select one from the library.
        if (currentTuneDef == null)
        {
            currentTuneDef = tuneScape.TuneDefCache.Where(x => !x.instrumentOnly).RandomElement();
        }

        //We're off? Let's change that.
        if (state != State.off)
        {
            return;
        }

        state = State.on;

        StartMusic();
    }

    // ============= COMPS ===================//

    //We need a power trader, so let's see if we have a comp.
    private bool TryResolvePowerTrader()
    {
        //Do we need to set up a power trader?
        if (powerTrader != null)
        {
            return true;
        }

        powerTrader = this.TryGetComp<CompPowerTrader>();
        return powerTrader != null;
    }

    // ============= PLAYLIST CONTROLS ================= //

    private void SwitchTracks()
    {
        //Let's declare some variables.

        //Let's pick out the next tune.
        if (!TryResolveNextTrack(out var resolvedNextTune))
        {
            Log.Error("Could not resolve next track.");
            return;
        }

        NextTune = resolvedNextTune;

        //Let's seal up the current variables in  nice clean envelope.
        var curTune = CurrentTune;
        var nextTune = NextTune;

        //Stop the music.
        StopMusic();

        //Let's switch tracks.
        PreviousTune = curTune;
        CurrentTune = nextTune;

        //Start the music.
        StartMusic();
    }

    /// <summary>
    ///     Triggered from resolve next track, if no playlist exists, this method creates it.
    /// </summary>
    /// <returns></returns>
    private bool TryCreatePlaylist()
    {
        if (tuneScape.TuneDefCache == null)
        {
            return false;
        }

        if (!tuneScape.TuneDefCache.Any(x => !x.instrumentOnly))
        {
            return false;
        }

        var tempList = tuneScape.TuneDefCache.ToList();
        playlist = [..tempList.InRandomOrder()];
        return true;
    }

    /// <summary>
    ///     Handles the upcoming track.
    /// </summary>
    /// <param name="tuneDef"></param>
    /// <returns></returns>
    private bool TryResolveNextTrack(out TuneDef tuneDef)
    {
        tuneDef = null;
        if (playlist.Count == 0)
        {
            if (!TryCreatePlaylist())
            {
                Log.Error("Unable to create playlist!");
                return false;
            }
        }

        TuneDef result = null;
        for (var i = 0; i < 999; i++)
        {
            if (!playlist.TryRandomElement(out result))
            {
                continue;
            }

            if (result == CurrentTune)
            {
                continue;
            }

            break;
        }

        if (result == null)
        {
            return false;
        }

        tuneDef = result;
        return true;
    }

    public virtual void StartMusic(TuneDef parmDef = null)
    {
        if (state == State.off)
        {
            state = State.on;
        }

        //Establish duration
        //Cthulhu.Utility.DebugReport("Cur Time:" + Time.time.ToString());
        duration = Time.time + currentTuneDef.durationTime;
        //Cthulhu.Utility.DebugReport(currentTuneDef.ToString() + " Fin Time:" + duration.ToString());

        //Clear old song
        playingSong = null;

        //Put on new song
        var soundInfo = SoundInfo.InMap(this);
        var soundDef = currentTuneDef as SoundDef;
        if (parmDef != null)
        {
            soundDef = parmDef;
        }

        playingSong = soundDef.TrySpawnSustainer(soundInfo);
    }

    public void StopMusic()
    {
        //Probably want to change songs right? Well let's turn off our current song.
        if (state != State.on)
        {
            return;
        }

        state = State.off;
        duration = -1f;
        //Let's stop the music.
        //Music command.
        playingSong?.End();
    }

    /// <summary>
    ///     Checks for cells in a 7.9 radius around for listening.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public static List<IntVec3> ListenableCellsAround(IntVec3 pos, Map map)
    {
        //Erase all the cells and recheck.
        listenableCells.Clear();
        if (!pos.InBounds(map))
        {
            return listenableCells;
        }

        var region = pos.GetRegion(map);
        if (region == null)
        {
            return listenableCells;
        }

        RegionTraverser.BreadthFirstTraverse(region, (_, r) => r.door == null, delegate(Region r)
        {
            foreach (var current in r.Cells)
            {
                if (current.InHorDistOf(pos, ListenRadius)) //Check within a 7.9 radius
                {
                    listenableCells.Add(current);
                }
            }

            return false;
        }, 12);
        return listenableCells; //Return the cells we find.
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        using var enumerator = base.GetGizmos().GetEnumerator();
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            yield return current;
        }

        if (!isRadio || powerTrader == null)
        {
            yield break;
        }

        var toggleDef = new Command_Toggle
        {
            hotKey = KeyBindingDefOf.Command_TogglePower,
            icon = ContentFinder<Texture2D>.Get("UI/Icons/Commands/Autoplay"),
            defaultLabel = "CCIA.Autoplay".Translate(),
            defaultDesc = "CCIA.AutoplayTT".Translate(),
            isActive = () => autoPlay,
            toggleAction = delegate { autoPlay = !autoPlay; },
            disabled = true
        };
        if (powerTrader.PowerOn)
        {
            toggleDef.disabled = false;
        }

        yield return toggleDef;
    }

    /// <summary>
    ///     All the menu options for the Gramophone.
    /// </summary>
    /// <param name="myPawn"></param>
    /// <returns></returns>
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
    {
        if (!myPawn.CanReserve(this, 16))
        {
            var item = new FloatMenuOption("CannotUseReserved".Translate(), null);
            return new List<FloatMenuOption>
            {
                item
            };
        }

        if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
        {
            var item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
            return new List<FloatMenuOption>
            {
                item2
            };
        }

        if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
        {
            var item3 = new FloatMenuOption(
                "CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Manipulation.label)),
                null);
            return new List<FloatMenuOption>
            {
                item3
            };
        }


        var list = new List<FloatMenuOption>();
        if (IsOn())
        {
            void action0()
            {
                Job job = null;
                if (ListenBuildingUtility.TryFindBestListenCell(this, myPawn, true, out var vec, out var t2))
                {
                    job = new Job(DefDatabase<JobDef>.GetNamed("ListenToGramophone"), this, vec, t2);
                }
                else if (ListenBuildingUtility.TryFindBestListenCell(this, myPawn, false, out vec, out t2))
                {
                    job = new Job(DefDatabase<JobDef>.GetNamed("ListenToGramophone"), this, vec, t2);
                }

                if (job == null)
                {
                    return;
                }

                job.targetB = vec;
                job.targetC = t2;
                if (myPawn.jobs.TryTakeOrderedJob(job))
                {
                    //Lala
                }
            }

            list.Add(new FloatMenuOption("CCIA.ListenTo".Translate(Label), action0));

            void action0a()
            {
                var job = new Job(DefDatabase<JobDef>.GetNamed("TurnOffGramophone"), this)
                {
                    targetA = this
                };
                if (myPawn.jobs.TryTakeOrderedJob(job))
                {
                    //Lala
                }
            }

            list.Add(new FloatMenuOption("CCIA.TurnOff".Translate(Label), action0a));
        }


        if (tuneScape == null)
        {
            return list;
        }

        var tuneDefs = tuneScape.TuneDefCache.Where(x => !x.instrumentOnly);
        if (!tuneDefs.Any())
        {
            return list;
        }

        foreach (var tuneDef in tuneDefs)
        {
            list.Add(new FloatMenuOption("CCIA.Play".Translate(tuneDef.LabelCap), actionDef));
            continue;

            void actionDef()
            {
                var job = new Job(DefDatabase<JobDef>.GetNamed("PlayGramophone"), this)
                {
                    targetA = this
                };
                currentTuneDef = tuneDef;
                if (myPawn.jobs.TryTakeOrderedJob(job))
                {
                    //Lala
                }
            }
        }

        return list;
    }

    // ===================== Ticker =====================

    /// <summary>
    ///     This is used, when the Ticker in the XML is set to 'Rare'
    ///     This is a tick that's done once every 250 normal Ticks
    /// </summary>
    public override void TickRare()
    {
        if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
        {
            return;
        }

        // Don't forget the base work
        base.TickRare();

        // Call work function
        DoTickerWork(250);
    }


    /// <summary>
    ///     This is used, when the Ticker in the XML is set to 'Normal'
    ///     This Tick is done often (60 times per second)
    /// </summary>
    public override void Tick()
    {
        if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
        {
            return;
        }

        base.Tick();

        // Call work function
        DoTickerWork(1);
    }

    // ===================== Main Work Function =====================

    /// <summary>
    ///     This will be called from one of the Ticker-Functions.
    /// </summary>
    /// <param name="tickerAmount"></param>
    private void DoTickerWork(int tickerAmount)
    {
        // set the old variable
        stateOld = state;

        rareTickWorker -= 1;
        if (isRadio && rareTickWorker <= 0)
        {
            rareTickWorker = 250;
            if (!TryResolvePowerTrader())
            {
                Log.Error("Radio Error: Cannot resolve power trader comp.");
                return;
            }

            if (!powerTrader.PowerOn)
            {
                //Cthulhu.Utility.DebugReport("Radio: Power Off Called");
                StopMusic();
            }
        }

        if (duration == -1f)
        {
            return; //If duration isn't initialized, don't bother checking.
        }


        //Are we on? Okay. Let's play!
        if (state != State.on)
        {
            return;
        }

        //Should we turn off?
        if (!(Time.time >= duration))
        {
            return;
        }

        //Are we a radio?
        if (isRadio)
        {
            if (!TryResolvePowerTrader())
            {
                Log.Error("Radio Error: Cannot resolve power trader comp.");
                return;
            }

            if (powerTrader.PowerOn)
            {
                //Do we have autoplay on?
                if (autoPlay)
                {
                    SwitchTracks();
                }
                else
                {
                    StopMusic();
                }
            }
            else
            {
                if (autoPlay)
                {
                    autoPlay = false;
                }

                StopMusic();
            }
        }
        else
        {
            StopMusic();
        }
    }

    //What song are we playing?
    private enum Song
    {
        Stop = 0,
        Charleston,
        InTheMood,
        KingPorterStomp
    }
}