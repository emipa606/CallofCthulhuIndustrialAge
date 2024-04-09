using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
//using VerseBase;
//using RimWorld.Planet;
//using RimWorld.SquadAI;


namespace IndustrialAge.Objects;

public class JobDriver_AutoPlayListenGramophone : JobDriver
{
    public TargetIndex BedIndex = TargetIndex.C;
    public TargetIndex ChairIndex = TargetIndex.B;

    public TargetIndex GramophoneIndex = TargetIndex.A;

    private string report = "";

    public Building_Gramophone Gramophone
    {
        get
        {
            if (pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing is not Building_Gramophone result)
            {
                throw new InvalidOperationException("Gramophone is missing.");
            }

            return result;
        }
    }

    protected int Duration { get; } = 400;

    public override bool TryMakePreToilReservations(bool debug)
    {
        return true;
    }

    public override string GetReport()
    {
        return report != "" ? base.ReportStringProcessed(report) : base.GetReport();
    }

    //What should we do?
    public override IEnumerable<Toil> MakeNewToils()
    {
        //Check it out. Can we go there?
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        //Wait a minute, is this thing already playing?
        if (!Gramophone.IsOn())
        {
            if (job.targetA.Thing is Building_Radio)
            {
                report = "CCIA.Playing".Translate();
            }

            // Toil 1:
            // Reserve Target (TargetPack A is selected (It has the info where the target cell is))
            yield return Toils_Reserve.Reserve(TargetIndex.A);

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
            wind.WithProgressBarToilDelay(TargetIndex.A);
            wind.PlaySustainerOrSound(job.targetA.Thing is Building_Radio
                ? DefDatabase<SoundDef>.GetNamed("Estate_RadioSeeking")
                : DefDatabase<SoundDef>.GetNamed("Estate_GramophoneWindup"));

            wind.initAction = delegate { Gramophone.StopMusic(); };
            yield return wind;

            // Toil 4:
            // Play music.

            var toilPlayMusic = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
                initAction = delegate { Gramophone.PlayMusic(pawn); }
            };
            yield return toilPlayMusic;
        }

        Toil toil;
        if (TargetC is { HasThing: true, Thing: Building_Bed bed }) //If we have a bed, lie in bed to listen.
        {
            this.KeepLyingDown(TargetIndex.C);
            yield return Toils_Reserve.Reserve(TargetIndex.C, bed.SleepingSlotsCount);
            yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C);
            yield return Toils_Bed.GotoBed(TargetIndex.C);
            toil = Toils_LayDown.LayDown(TargetIndex.C, true, false);
            toil.AddFailCondition(() => !pawn.Awake());
        }
        else
        {
            if (TargetC.HasThing)
            {
                yield return Toils_Reserve.Reserve(TargetIndex.C);
            }

            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            toil = new Toil();
        }

        toil.AddPreTickAction(delegate
        {
            ListenTickAction();
            if (job.targetA.Thing is Building_Radio)
            {
                report = "CCIA.Listening".Translate();
            }
        });
        toil.AddFinishAction(delegate { JoyUtility.TryGainRecRoomThought(pawn); });
        toil.defaultCompleteMode = ToilCompleteMode.Delay;
        toil.defaultDuration = job.def.joyDuration;
        yield return toil;
    }

    protected virtual void ListenTickAction()
    {
        if (!Gramophone.IsOn())
        {
            EndJobWith(JobCondition.Incompletable);
            return;
        }

        pawn.rotationTracker.FaceCell(TargetA.Cell);
        pawn.GainComfortFromCellIfPossible();
        var statValue = TargetThingA.GetStatValue(StatDefOf.JoyGainFactor);
        JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, statValue);
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