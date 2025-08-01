using System.Collections.Generic;
using Verse;
using Verse.AI;
//using VerseBase;
//using RimWorld.Planet;
//using RimWorld.SquadAI;


namespace IndustrialAge.Objects;

public class JobDriver_PlayGramophone : JobDriver
{
    private string report = "";

    private int Duration { get; } = 400;

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
        var toil = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = Duration
        };
        toil.WithProgressBarToilDelay(TargetIndex.A);
        toil.PlaySustainerOrSound(job.targetA.Thing is Building_Radio
            ? DefDatabase<SoundDef>.GetNamed("Estate_RadioSeeking")
            : DefDatabase<SoundDef>.GetNamed("Estate_GramophoneWindup"));

        toil.initAction = delegate
        {
            var gramophone = job.targetA.Thing as Building_Gramophone;
            gramophone?.StopMusic();
        };
        yield return toil;

        // Toil 4:
        // Play music.

        var toilPlayMusic = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                var gramophone = job.targetA.Thing as Building_Gramophone;
                gramophone?.PlayMusic(pawn);
            }
        };
        yield return toilPlayMusic;
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