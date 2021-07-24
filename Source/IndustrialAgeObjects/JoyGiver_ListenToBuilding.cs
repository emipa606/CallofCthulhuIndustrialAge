using RimWorld;
using Verse;
using Verse.AI;

namespace IndustrialAge.Objects
{
    public class JoyGiver_ListenToBuilding : JoyGiver_InteractBuilding
    {
        protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
        {
            if (!base.CanInteractWith(pawn, t, inBed))
            {
                return false;
            }

            if (!inBed)
            {
                return true;
            }

            var layingDownBed = pawn.CurrentBed();

            return ListenBuildingUtility.CanListenFromBed(pawn, layingDownBed, t);
        }

        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            if (!ListenBuildingUtility.TryFindBestListenCell(t, pawn, def.desireSit, out var vec, out var t2))
            {
                if (!ListenBuildingUtility.TryFindBestListenCell(t, pawn, false, out vec, out t2))
                {
                    return null;
                }
            }

            if (t2 == null)
            {
                return new Job(def.jobDef, t, vec, (Building) null);
            }

            if (vec != t2.Position)
            {
                return new Job(def.jobDef, t, vec, t2);
            }

            if (!pawn.Map.reservationManager.CanReserve(pawn, t2))
            {
                return null;
            }

            return new Job(def.jobDef, t, vec, t2);
        }
    }
}