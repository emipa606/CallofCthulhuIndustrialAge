using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using System.Linq;
using UnityEngine;

namespace IndustrialAge.Objects
{
    public class Building_Woodstove : Building_WorkTable
    {
        private CompFlickable flickableComp;

        private CompGlower glowerComp;

        private CompRefuelable refuelableComp;

        private CompBreakdownable breakdownableComp;

        private CompHeatPusher heatPusherComp;

        private readonly int heatPerSecond = 21;

        private readonly int glowRadius = 10;

        private FloatRange smokeSize = new FloatRange(0.25f, 0.5f);

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
            Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
            Map.glowGrid.RegisterGlower(glowerComp);
        }

        public void ResolveSmoke()
        {
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                Pawn Dave = Map.mapPawns.FreeColonistsSpawned.FirstOrDefault((Pawn p) =>
                    p.Position == BillInteractionCell);
                if (Dave != null)
                {
                    if (Dave.CurJob.def == JobDefOf.DoBill)
                    {
                        IntVec3 smokePos = Position + GenAdj.CardinalDirections[Rot4.North.AsInt] +
                                           GenAdj.CardinalDirections[Rot4.North.AsInt];
                        var smokePosV3 = smokePos.ToVector3();
                        var smokePosX = (float) smokePos.x;
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
                        if (GenView.ShouldSpawnMotesAt(smokePos, Map))
                        {
                            var moteThrown =
                                (MoteThrown) ThingMaker.MakeThing(ThingDef.Named("Mote_Smoke"), null);
                            moteThrown.Scale = Rand.Range(1.5f, 2.5f) * smokeSize.RandomInRange;
                            moteThrown.exactRotation = Rand.Range(-0.5f, 0.5f);
                            moteThrown.exactPosition = new Vector3(smokePosX + Rand.Range(-0.1f, 0.1f), 0,
                                (float) smokePos.z + Rand.Range(-0.25f, 1.0f));
                            moteThrown.airTimeLeft = 5000f;
                            moteThrown.SetVelocity((float) Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
                            GenSpawn.Spawn(moteThrown, smokePos, Map);
                        }
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            ResolveGlowerAndHeater();
            ResolveSmoke();
        }

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref billStack, "billStack", new object[]
            {
                this
            });
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
            heatPusherComp = base.GetComp<CompHeatPusher>();
            flickableComp = base.GetComp<CompFlickable>();
            glowerComp = base.GetComp<CompGlower>();
            refuelableComp = base.GetComp<CompRefuelable>();
            breakdownableComp = base.GetComp<CompBreakdownable>();
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
}