using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using UnityEngine;
//using VerseBase;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace IndustrialAge.Objects
{
    public class JobDriver_PlayInstrument : JobDriver
    {
        private readonly HediffDef sanityLossHediff;
        private readonly float sanityRestoreRate = 0.1f;

        public override bool TryMakePreToilReservations(bool debug)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(TargetIndex.A, base.job.def.joyMaxParticipants);
            if (TargetB != null)
            {
                yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            }

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            var toil = new Toil();
            var soundDef = Find.World.GetComponent<WorldComponent_Tunes>().TuneDefCache.FindAll(x => x.instrumentDefs.Contains(TargetThingA.def)).RandomElement();
            toil.PlaySustainerOrSound(soundDef);
            toil.tickAction = delegate
            {
                pawn.rotationTracker.FaceCell(TargetA.Cell);
                pawn.GainComfortFromCellIfPossible();
                var statValue = TargetThingA.GetStatValue(StatDefOf.JoyGainFactor, true);
                var extraJoyGainFactor = statValue;
                JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = base.job.def.joyDuration;
            toil.AddFinishAction(delegate
            {
                if (Cthulhu.Utility.IsCosmicHorrorsLoaded())
                {
                    try
                    {
                        if (Cthulhu.Utility.HasSanityLoss(pawn))
                        {
                            Cthulhu.Utility.ApplySanityLoss(pawn, -sanityRestoreRate, 1);
                            Messages.Message(pawn.ToString() + " has restored some sanity using the " + TargetA.Thing.def.label + ".", new TargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.NeutralEvent);// .Standard);
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
            yield break;
        }
    }

}
