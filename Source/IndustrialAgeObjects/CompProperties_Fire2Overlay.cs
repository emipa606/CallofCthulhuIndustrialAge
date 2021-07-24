using UnityEngine;
using Verse;

namespace RimWorld
{
    /// /////////////Fire Overlay 2
    public class CompProperties_Fire2Overlay : CompProperties
    {
        public float flameSize = 1f;

        public Vector3 offset;

        public CompProperties_Fire2Overlay()
        {
            compClass = typeof(Comp2FireOverlay);
        }
    }
}