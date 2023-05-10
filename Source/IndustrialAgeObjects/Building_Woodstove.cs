using RimWorld;
using UnityEngine;
using Verse;

namespace IndustrialAge.Objects;

public class Building_Woodstove : Building_WorkTable
{
    private readonly int glowRadius = 10;

    private readonly int heatPerSecond = 21;

    private CompBreakdownable breakdownableComp;
    private CompFlickable flickableComp;

    private CompGlower glowerComp;

    private CompHeatPusher heatPusherComp;

    private CompRefuelable refuelableComp;

    private FloatRange smokeSize = new FloatRange(0.25f, 0.5f);

//        public override bool UsableNow
//        {
//            get
//            {
//                return ((this.flickableComp != null && this.flickableComp.SwitchIsOn)) &&
//                       (this.refuelableComp == null || this.refuelableComp.HasFuel) &&
//                       (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
//            }
//        }

    public Building_Woodstove()
    {
        billStack = new BillStack(this);
    }

    public void ResolveGlowerAndHeater()
    {
        if (flickableComp == null)
        {
            return;
        }

        if (refuelableComp == null)
        {
            return;
        }

        if (glowerComp == null)
        {
            return;
        }

        if (heatPusherComp == null)
        {
            return;
        }

        if (flickableComp.SwitchIsOn && refuelableComp.Fuel > 0f)
        {
            heatPusherComp.Props.heatPerSecond = heatPerSecond;
            glowerComp.Props.glowRadius = glowRadius;
        }
        else
        {
            heatPusherComp.Props.heatPerSecond = 0f;
            glowerComp.Props.glowRadius = 0f;
        }

        Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
        Map.glowGrid.RegisterGlower(glowerComp);
    }

    public void ResolveSmoke()
    {
        if (Find.TickManager.TicksGame % 60 != 0)
        {
            return;
        }

        var Dave = Map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p =>
            p.Position == BillInteractionCell);
        if (Dave == null)
        {
            return;
        }

        if (Dave.CurJob.def != JobDefOf.DoBill)
        {
            return;
        }

        var smokePos = Position + GenAdj.CardinalDirections[Rot4.North.AsInt] +
                       GenAdj.CardinalDirections[Rot4.North.AsInt];
        var unused = smokePos.ToVector3();
        var smokePosX = (float)smokePos.x;
        if (Rotation == Rot4.North || Rotation == Rot4.South)
        {
            smokePosX += 0.5f;
        }
        else if (Rotation == Rot4.West)
        {
            smokePosX += 0.75f;
        }
        else if (Rotation == Rot4.East)
        {
            smokePosX += 0.25f;
        }

        if (!smokePos.ShouldSpawnMotesAt(Map))
        {
            return;
        }

        var position = new Vector3(smokePosX + Rand.Range(-0.1f, 0.1f), 0,
            smokePos.z + Rand.Range(-0.25f, 1.0f));
        FleckMaker.ThrowSmoke(position, Map, Rand.Range(1.5f, 2.5f) * smokeSize.RandomInRange);

        //var moteThrown =
        //    (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_Smoke"));
        //moteThrown.Scale = Rand.Range(1.5f, 2.5f) * smokeSize.RandomInRange;
        //moteThrown.exactRotation = Rand.Range(-0.5f, 0.5f);
        //moteThrown.exactPosition = new Vector3(smokePosX + Rand.Range(-0.1f, 0.1f), 0,
        //    smokePos.z + Rand.Range(-0.25f, 1.0f));
        //moteThrown.airTimeLeft = 5000f;
        //moteThrown.SetVelocity(Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
        //GenSpawn.Spawn(moteThrown, smokePos, Map);
    }

    public override void Tick()
    {
        base.Tick();
        ResolveGlowerAndHeater();
        ResolveSmoke();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref billStack, "billStack", this);
    }

    public override void SpawnSetup(Map map, bool bla)
    {
        base.SpawnSetup(map, bla);
        heatPusherComp = GetComp<CompHeatPusher>();
        flickableComp = GetComp<CompFlickable>();
        glowerComp = GetComp<CompGlower>();
        refuelableComp = GetComp<CompRefuelable>();
        breakdownableComp = GetComp<CompBreakdownable>();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        heatPusherComp = null;
        flickableComp = null;
        glowerComp = null;
        refuelableComp = null;
        breakdownableComp = null;
    }

    //public override Map Map()
    //{
    //    return base.Map;
    //}
}