using UnityEngine;
using Verse;

namespace RimWorld;

/// /////////////Fire Overlay 4
public class CompProperties_Fire4Overlay : CompProperties
{
    public float flameSize = 1f;

    public Vector3 offset;

    public CompProperties_Fire4Overlay()
    {
        compClass = typeof(Comp4FireOverlay);
    }
}