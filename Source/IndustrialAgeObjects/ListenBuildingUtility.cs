using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace IndustrialAge.Objects;

public class ListenBuildingUtility
{
    public static bool TryFindBestListenCell(Thing toListen, Pawn pawn, bool desireSit, out IntVec3 result,
        out Building chair)
    {
        var unused = IntVec3.Invalid;
        if (toListen is Building_Gramophone musicBuilding)
        {
            var cells = musicBuilding.ListenableCells;
            var random = new Random();
            IEnumerable<IntVec3> cellsRandom = cells.OrderBy(_ => random.Next()).ToList();

            foreach (var current in cellsRandom)
            {
                var isSittable = false;
                Building building = null;
                if (desireSit)
                {
                    building = current.GetEdifice(pawn.Map);
                    if (building != null && building.def.building.isSittable && pawn.CanReserve(building))
                    {
                        isSittable = true;
                    }
                }
                else if (!current.IsForbidden(pawn) && pawn.CanReserve(current))
                {
                    isSittable = true;
                }

                if (!isSittable)
                {
                    continue;
                }

                result = current;
                chair = building;
                return true;
            }
        }

        result = IntVec3.Invalid;
        chair = null;
        return false;
    }

    // RimWorld.WatchBuildingUtility
    public static bool CanListenFromBed(Pawn pawn, Building_Bed bed, Thing toListen)
    {
        if (!pawn.Position.Standable(pawn.Map) || pawn.Position.GetEdifice(pawn.Map) is Building_Bed)
        {
            return false;
        }

        if (toListen is not Building_Gramophone musicBuilding)
        {
            return false;
        }

        var cells = musicBuilding.ListenableCells;
        foreach (var current in cells)
        {
            if (current == pawn.Position)
            {
                return true;
            }
        }

        return false;
    }
}