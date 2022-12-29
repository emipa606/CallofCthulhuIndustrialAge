using System.Collections.Generic;
using System.Diagnostics;
using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI;
//using VerseBase;

namespace IndustrialAge.Objects;

public class JobDriver_PlayInstrument : JobDriver
{
    private readonly HediffDef sanityLossHediff;
    private readonly float sanityRestoreRate = 0.1f;

    public override bool TryMakePreToilReservations(bool debug)
    {
        return true;
    }

    [DebuggerHidden]
    public override IEnumerable<Toil> MakeNewToils()
    {
        this.EndOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.A, job.def.joyMaxParticipants);
        if (TargetB != null)
        {
            yield return Toils_Reserve.Reserve(TargetIndex.B);
        }

        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
        var toil = new Toil();
        var soundDef = Find.World.GetComponent<WorldComponent_Tunes>().TuneDefCache
            .FindAll(x => x.instrumentDefs.Contains(TargetThingA.def)).RandomElement();
        toil.PlaySustainerOrSound(soundDef);
        toil.tickAction = delegate
        {
            pawn.rotationTracker.FaceCell(TargetA.Cell);
            pawn.GainComfortFromCellIfPossible();
            var statValue = TargetThingA.GetStatValue(StatDefOf.JoyGainFactor);
            JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, statValue);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Delay;
        toil.defaultDuration = job.def.joyDuration;
        toil.AddFinishAction(delegate
        {
            if (Utility.IsCosmicHorrorsLoaded())
            {
                try
                {
                    if (Utility.HasSanityLoss(pawn))
                    {
                        Utility.ApplySanityLoss(pawn, -sanityRestoreRate);
                        Messages.Message(
                            $"{pawn} has restored some sanity using the {TargetA.Thing.def.label}.",
                            new TargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.NeutralEvent); // .Standard);
                    }
                }
                catch
                {
                    Log.Message("Error loading Sanity Hediff.");
                }
            }

            JoyUtility.TryGainRecRoomThought(pawn);
        });
        yield return toil;
    }
}