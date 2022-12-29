using UnityEngine;
using Verse;

namespace RimWorld;

/// /////////////Fire Overlay 3
public class CompProperties_Fire3Overlay : CompProperties
{
    public float flameSize = 1f;

    public Vector3 offset;

    public CompProperties_Fire3Overlay()
    {
        compClass = typeof(Comp3FireOverlay);
    }
}