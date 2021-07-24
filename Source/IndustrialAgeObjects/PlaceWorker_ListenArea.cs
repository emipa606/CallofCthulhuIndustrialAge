using UnityEngine;
using Verse;

namespace IndustrialAge.Objects
{
    public class PlaceWorker_ListenArea : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var visibleMap = Find.CurrentMap;
            GenDraw.DrawFieldEdges(Building_Gramophone.ListenableCellsAround(center, visibleMap));
        }
    }
}