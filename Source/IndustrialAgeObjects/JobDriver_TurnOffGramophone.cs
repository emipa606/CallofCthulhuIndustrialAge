using System.Collections.Generic;
using Verse.AI;
//using VerseBase;
//using RimWorld.Planet;
//using RimWorld.SquadAI;


namespace IndustrialAge.Objects;

public class JobDriver_TurnOffGramophone : JobDriver
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

    //What should we do?
    public override IEnumerable<Toil> MakeNewToils()
    {
        //Check it out. Can we go there?
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        if (job.targetA.Thing is Building_Radio)
        {
            report = "Turning off radio.";
        }

        // Toil 1:
        // Reserve Target (TargetPack A is selected (It has the info where the target cell is))
        yield return Toils_Reserve.Reserve(TargetIndex.A);

        // Toil 2:
        // Go to the thing.
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        // Toil 3:
        // Turn off music.

        var toilStopMusic = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                var gramophone = job.targetA.Thing as Building_Gramophone;
                gramophone?.StopMusic();
            }
        };
        yield return toilStopMusic;
    }
}