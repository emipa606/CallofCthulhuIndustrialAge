using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;
//using VerseBase;

namespace IndustrialAge.Objects;

public class JobDriver_ListenToGramophone : JobDriver
{
    private string report = "";

    public override bool TryMakePreToilReservations(bool debug)
    {
        return true;
    }

    public override string GetReport()
    {
        return report != "" ? base.ReportStringProcessed(report) : base.GetReport();
    }

    [DebuggerHidden]
    public override IEnumerable<Toil> MakeNewToils()
    {
        //Fail Checks

        this.EndOnDespawnedOrNull(TargetIndex.A); //If we don't exist, exit.

        if (job.targetA.Thing is Building_Radio)
        {
            report = "CCIA.Listening".Translate();
        }


        //yield return Toils_Reserve.Reserve(TargetIndex.A, base.CurJob.def.joyMaxParticipants); //Can we reserve?

        //yield return Toils_Reserve.Reserve(TargetIndex.B, 1);   //Reserve

        Toil toil;
        if (TargetC is { HasThing: true, Thing: Building_Bed }) //If we have a bed, do something else.
        {
            this.KeepLyingDown(TargetIndex.C);
            yield return Toils_Reserve.Reserve(TargetIndex.C, ((Building_Bed)TargetC.Thing).SleepingSlotsCount);
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

        toil.AddPreTickIntervalAction(delegate(int delta)
        {
            if (job.targetA.Thing is Building_Radio)
            {
                report = "CCIA.Listening".Translate();
            }

            ListenTickAction(delta);
        });
        toil.AddFinishAction(delegate { JoyUtility.TryGainRecRoomThought(pawn); });
        toil.defaultCompleteMode = ToilCompleteMode.Delay;
        toil.defaultDuration = job.def.joyDuration * 2;
        yield return toil;
    }

    protected virtual void ListenTickAction(int delta)
    {
        if (TargetA.Thing is Building_Gramophone gramo && !gramo.IsOn())
        {
            EndJobWith(JobCondition.Incompletable);
            return;
        }

        pawn.rotationTracker.FaceCell(TargetA.Cell);
        pawn.GainComfortFromCellIfPossible(delta);
        var statValue = TargetThingA.GetStatValue(StatDefOf.JoyGainFactor);
        JoyUtility.JoyTickCheckEnd(pawn, delta, JoyTickFullJoyAction.EndJob, statValue);
    }
}