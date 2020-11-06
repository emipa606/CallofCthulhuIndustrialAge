using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
//using VerseBase;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;
//using RimWorld.SquadAI;


namespace IndustrialAge.Objects
{

    public class JobDriver_AutoPlayListenGramophone : JobDriver
    {

        public override bool TryMakePreToilReservations(bool debug)
        {
            return true;
        }

        public Building_Gramophone Gramophone
        {
            get
            {
                if (!(pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing is Building_Gramophone result))
                {
                    throw new InvalidOperationException("Gramophone is missing.");
                }
                return result;
            }
        }

        public TargetIndex GramophoneIndex = TargetIndex.A;
        public TargetIndex ChairIndex = TargetIndex.B;
        public TargetIndex BedIndex = TargetIndex.C;

        protected int Duration { get; } = 400;

        private string report = "";
        public override string GetReport()
        {
            if (report != "")
            {
                return base.ReportStringProcessed(report);
            }
            return base.GetReport();
        }

        //What should we do?
        protected override IEnumerable<Toil> MakeNewToils()
        {

            //Check it out. Can we go there?
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            //Wait a minute, is this thing already playing?
            if (!Gramophone.IsOn())
            {
                if (job.targetA.Thing is Building_Radio)
                {
                    report = "playing the radio.";
                }

                // Toil 1:
                // Reserve Target (TargetPack A is selected (It has the info where the target cell is))
                yield return Toils_Reserve.Reserve(TargetIndex.A, 1);

                // Toil 2:
                // Go to the thing.
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

                // Toil 3:
                // Wind up the gramophone
                var wind = new Toil
                {
                    defaultCompleteMode = ToilCompleteMode.Delay,
                    defaultDuration = Duration
                };
                wind.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
                if (job.targetA.Thing is Building_Radio)
                {
                    wind.PlaySustainerOrSound(DefDatabase<SoundDef>.GetNamed("Estate_RadioSeeking"));
                }
                else
                {
                    wind.PlaySustainerOrSound(DefDatabase<SoundDef>.GetNamed("Estate_GramophoneWindup"));
                }
                wind.initAction = delegate
                {
                    Gramophone.StopMusic();
                };
                yield return wind;

                // Toil 4:
                // Play music.

                var toilPlayMusic = new Toil
                {
                    defaultCompleteMode = ToilCompleteMode.Instant,
                    initAction = delegate
                    {
                        Gramophone.PlayMusic(pawn);
                    }
                };
                yield return toilPlayMusic;

            }

            Toil toil;
            if (base.TargetC.HasThing && base.TargetC.Thing is Building_Bed bed)   //If we have a bed, lie in bed to listen.
            {
                this.KeepLyingDown(TargetIndex.C);
                yield return Toils_Reserve.Reserve(TargetIndex.C, bed.SleepingSlotsCount);
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C, TargetIndex.None);
                yield return Toils_Bed.GotoBed(TargetIndex.C);
                toil = Toils_LayDown.LayDown(TargetIndex.C, true, false, true, true);
                toil.AddFailCondition(() => !pawn.Awake());

            }
            else
            {
                if (base.TargetC.HasThing)
                {
                    yield return Toils_Reserve.Reserve(TargetIndex.C, 1);
                }
                yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
                toil = new Toil();

            }
            toil.AddPreTickAction(delegate
            {
                ListenTickAction();
                if (job.targetA.Thing is Building_Radio)
                {
                    report = "Listening to the radio.";
                }
            });
            toil.AddFinishAction(delegate
            {
                JoyUtility.TryGainRecRoomThought(pawn);
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = base.job.def.joyDuration;
            yield return toil;
            yield break;
        }

        protected virtual void ListenTickAction()
        {
            if (!Gramophone.IsOn())
            {
                base.EndJobWith(JobCondition.Incompletable);
                return;
            }
            pawn.rotationTracker.FaceCell(base.TargetA.Cell);
            pawn.GainComfortFromCellIfPossible();
            var statValue = base.TargetThingA.GetStatValue(StatDefOf.JoyGainFactor, true);
            var extraJoyGainFactor = statValue;
            JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
        }

    }
}




/*

This is the needed XML file to make a real Job from the JobDriver
     
<?xml version="1.0" encoding="utf-8" ?>
<JobDefs>
<!--========= Job ============-->
	<JobDef>
	<defName>PlayGramophone</defName>
	<driverClass>ArkhamEstate.JobDriver_PlayGramophone</driverClass>
	<reportString>Winding up gramophone.</reportString>
	</JobDef>
</JobDefs>
     
*/
