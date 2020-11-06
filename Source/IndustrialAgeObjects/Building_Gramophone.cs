// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace IndustrialAge.Objects
{
    /// <summary>
    /// This is the main class for the Gramophone.
    /// Major coding credits go to mrofa and Haplo.
    /// I am but an amateur working on the shoulders of
    /// giants.
    /// </summary>
    /// <author>Jecrell</author>
    /// <permission>Free to use by all.</permission>
    public class Building_Gramophone : Building
    {
        // ===================== Variables =====================

        // Work variable
        private int counter = 0;                  // 60Ticks = 1s // 20000Ticks = 1 Day
        private float duration = -1f;
        private State state = State.off;      // Actual phase
        private State stateOld = State.on;    // Save-variable
        private const float ListenRadius = 7.9f;
        private static List<IntVec3> listenableCells = new List<IntVec3>();
        private TuneDef prevTuneDef;
        private TuneDef currentTuneDef;
        private TuneDef nextTuneDef;
        private CompPowerTrader powerTrader;
        private List<TuneDef> playlist = new List<TuneDef>();
        private readonly WorldComponent_Tunes tuneScape = Find.World.GetComponent<WorldComponent_Tunes>();
        public bool isRadio = false;
        private bool autoPlay = false;
        private int rareTickWorker = 250;

        protected Sustainer playingSong;

        // Variables to set a specific value
        private const int counterWoundMax = 20000;  // Active-Time

        //Is our music player on or off?
        public enum State
        {
            off = 0,
            on
        }

        public TuneDef CurrentTune { get => currentTuneDef; set => currentTuneDef = value; }
        public TuneDef NextTune { get => nextTuneDef; set => nextTuneDef = value; }
        public TuneDef PreviousTune { get => prevTuneDef; set => prevTuneDef = value; }
        public State CurrentState { get => state; set => state = value; }

        public bool IsOn()
        {
            if (state == State.on)
            {
                return true;
            }
            return false;
        }


        public IEnumerable<IntVec3> ListenableCells => ListenableCellsAround(base.Position, base.Map);

        //What song are we playing?
        private enum Song
        {
            Stop = 0,
            Charleston,
            InTheMood,
            KingPorterStomp,
        }

        private readonly SoundDef songCharleston = SoundDef.Named("Estate_GS_Charleston");
        private readonly SoundDef songInTheMood = SoundDef.Named("Estate_GS_InTheMood");
        private readonly SoundDef songKingPorterStomp = SoundDef.Named("Estate_GS_KingPorterStomp");

        // Component references (will be set in 'SpawnSetup()')
        // CompMusicPlayer  - This makes it possible for your building to play music. You can start and stop the music.
        //private CompMusicPlayer musicComp;

        private readonly string txtStatus = "Status";
        private readonly string txtOff = "Off";
        private readonly string txtOn = "On";


        private readonly string txtPlaying = "Now Playing:";

        // Destroyed flag. Most of the time not really needed, but sometimes...
        private bool destroyedFlag = false;

        /// <summary>
        /// Do something after the object is spawned
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
        /// To save and load actual values (savegame-data)
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            // Save and load the work variables, so they don't default after loading
            Scribe_Values.Look(ref isRadio, "isRadio", false);
            Scribe_Values.Look(ref autoPlay, "autoPlay", false);
            Scribe_Values.Look(ref state, "state", State.off);
            Scribe_Values.Look(ref counter, "counter", 0);
            Scribe_Defs.Look(ref prevTuneDef, "prevTuneDef");
            Scribe_Defs.Look(ref currentTuneDef, "currentTuneDef");
            Scribe_Defs.Look(ref nextTuneDef, "nextTuneDef");
            Scribe_Collections.Look(ref playlist, "playlist", LookMode.Def, new object[0]);

            // Set the old value to the phase value
            stateOld = state;

            // Get refferences to the components CompPowerTrader and CompGlower
            //SetMusicPlayer();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                autoPlay = false;
                StopMusic();
            }
        }


        // ===================== Destroy =====================

        /// <summary>
        /// Clean up when this is destroyed
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            StopMusic();
            // block further ticker work
            destroyedFlag = true;

            base.Destroy(mode);
        }

        #region Ticker
        // ===================== Ticker =====================

        /// <summary>
        /// This is used, when the Ticker in the XML is set to 'Rare'
        /// This is a tick thats done once every 250 normal Ticks
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
        /// This is used, when the Ticker in the XML is set to 'Normal'
        /// This Tick is done often (60 times per second)
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
        /// This will be called from one of the Ticker-Functions.
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
            if (state == State.on)
            {

                //Should we turn off?
                if (Time.time >= duration) //Is the current time greater than the duration?
                {
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
            }

        }

        #endregion Ticker

        // ===================== Inspections =====================

        /// <summary>
        /// This string will be shown when the object is selected (focus)
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

            stringBuilder.Append(txtStatus + " ");  // <= TRANSLATION

            // State -> Off: Add text 'Off' (Translation from active language file)
            if (state == State.off)
            {
                stringBuilder.Append(txtOff);   // <= TRANSLATION
            }

            // State -> On: Add text 'On' (Translation from active language file)
            if (state == State.on)
            {
                stringBuilder.Append(txtOn);    // <= TRANSLATION

                stringBuilder.AppendLine();
                stringBuilder.Append(txtPlaying + " ");

                stringBuilder.Append(currentTuneDef.ToString());
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
            if (activator == null || this == null)
            {
                return;
            }

            //If there is no tuneDef set to play, then let's randomly select one from the library.
            if (currentTuneDef == null)
            {
                currentTuneDef = tuneScape.TuneDefCache.Where(x => !x.instrumentOnly).RandomElement();
            }

            //We're off? Let's change that.
            if (state == State.off)
            {

                state = State.on;

                StartMusic();
            }
        }

        // ============= COMPS ===================//

        //We need a power trader, so let's see if we have a comp.
        private bool TryResolvePowerTrader()
        {
            //Do we need to set up a power trader?
            if (powerTrader == null)
            {
                powerTrader = this.TryGetComp<CompPowerTrader>();
                if (powerTrader != null)
                {
                    return true;
                }

                return false;
            }
            return true;
        }

        // ============= PLAYLIST CONTROLS ================= //

        private void SwitchTracks()
        {
            //Let's declare some variables.
            TuneDef curTune;
            TuneDef nextTune;

            //Let's pick out the next tune.
            if (!TryResolveNextTrack(out TuneDef resolvedNextTune))
            {
                Log.Error("Could not resolve next track.");
                return;
            }
            NextTune = resolvedNextTune;

            //Let's seal up the current variables in  nice clean envelope.
            curTune = CurrentTune;
            nextTune = NextTune;

            //Stop the music.
            StopMusic();

            //Let's switch tracks.
            PreviousTune = curTune;
            CurrentTune = nextTune;

            //Start the music.
            StartMusic();


        }

        /// <summary>
        /// Triggered from resolve next track, if no playlist exists, this method creates it.
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
            playlist = new List<TuneDef>(tempList.InRandomOrder());
            return true;
        }

        /// <summary>
        /// Handles the upcoming track.
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        private bool TryResolveNextTrack(out TuneDef def)
        {
            def = null;
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
                if (playlist.TryRandomElement(out result))
                {
                    if (result == CurrentTune)
                    {
                        continue;
                    }

                    break;
                }
            }
            if (result != null)
            {
                def = result;
                return true;
            }
            return false;
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
            var soundInfo = SoundInfo.InMap(this, MaintenanceType.None);
            var soundDef = currentTuneDef as SoundDef;
            if (parmDef != null)
            {
                soundDef = parmDef as SoundDef;
            }

            playingSong = SoundStarter.TrySpawnSustainer(soundDef, soundInfo);
        }

        public void StopMusic()
        {
            //Probably want to change songs right? Well let's turn off our current song.
            if (state == State.on)
            {
                state = State.off;
                duration = -1f;
                //Let's stop the music.
                //Music command.
                if (playingSong != null)
                {
                    playingSong.End();
                }
            }
        }

        /// <summary>
        /// Checks for cells in a 7.9 radius around for listening.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static List<IntVec3> ListenableCellsAround(IntVec3 pos, Map map)
        {
            //Erase all the cells and recheck.
            listenableCells.Clear();
            if (!pos.InBounds(map))
            {
                return listenableCells;
            }
            Region region = pos.GetRegion(map);
            if (region == null)
            {
                return listenableCells;
            }
            RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null, delegate (Region r)
            {
                foreach (IntVec3 current in r.Cells)
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
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }

            if (isRadio && powerTrader != null)
            {
                var toggleDef = new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Command_TogglePower,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Commands/Autoplay", true),
                    defaultLabel = "Autoplay",
                    defaultDesc = "Enables automatic playing of music through the radio.",
                    isActive = () => autoPlay,
                    toggleAction = delegate
                    {
                        autoPlay = !autoPlay;
                    },
                    disabled = true
                };
                if (powerTrader.PowerOn)
                {
                    toggleDef.disabled = false;
                }

                yield return toggleDef;
            }
            yield break;
        }

        /// <summary>
        /// All the menu options for the Gramophone.
        /// </summary>
        /// <param name="myPawn"></param>
        /// <returns></returns>
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!myPawn.CanReserve(this, 16))
            {
                var item = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null);
                return new List<FloatMenuOption>
                {
                    item
                };
            }
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn))
            {
                var item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null);
                return new List<FloatMenuOption>
                {
                    item2
                };
            }
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                var item3 = new FloatMenuOption("CannotUseReason".Translate(new object[]
                {
                    "IncapableOfCapacity".Translate(new object[]
                    {
                        PawnCapacityDefOf.Manipulation.label
                    })
                }), null, MenuOptionPriority.Default, null, null, 0f, null);
                return new List<FloatMenuOption>
                {
                    item3
                };
            }


            var list = new List<FloatMenuOption>();
            IntVec3 vec = myPawn.Position;
            Building t2 = null;
            if (IsOn() == true)
            {
                void action0()
                {
                    Job job = null;
                    if (ListenBuildingUtility.TryFindBestListenCell(this, myPawn, true, out vec, out t2))
                    {
                        job = new Job(DefDatabase<JobDef>.GetNamed("ListenToGramophone"), this, vec, t2);
                    }
                    else if (ListenBuildingUtility.TryFindBestListenCell(this, myPawn, false, out vec, out t2))
                    {
                        job = new Job(DefDatabase<JobDef>.GetNamed("ListenToGramophone"), this, vec, t2);
                    }
                    if (job != null)
                    {
                        job.targetB = vec;
                        job.targetC = t2;
                        if (myPawn.jobs.TryTakeOrderedJob(job))
                        {
                            //Lala
                        }
                    }
                }
                list.Add(new FloatMenuOption("Listen to " + Label, action0, MenuOptionPriority.Default, null, null, 0f, null));

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
                list.Add(new FloatMenuOption("Turn off " + Label, action0a, MenuOptionPriority.Default, null, null, 0f, null));
            }


            if (tuneScape != null)
            {
                var tuneDefs = tuneScape.TuneDefCache.Where(x => !x.instrumentOnly);
                if (tuneDefs.Any())
                {
                    foreach (TuneDef def in tuneDefs)
                    {
                        void actionDef()
                        {
                            var job = new Job(DefDatabase<JobDef>.GetNamed("PlayGramophone"), this)
                            {
                                targetA = this
                            };
                            currentTuneDef = def;
                            if (myPawn.jobs.TryTakeOrderedJob(job))
                            {
                                //Lala
                            }
                        }
                        list.Add(new FloatMenuOption("Play " + def.LabelCap, actionDef, MenuOptionPriority.Default, null, null, 0f, null));
                    }
                }
            }
            return list;
        }

    }
}
