﻿using System.Collections.Generic;
using ArkhamEstate.SteamPowerSystems.Steam;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArkhamEstate
{
	[StaticConstructorOnStartup]
	public class Building_Boiler : Building
	{
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksToExplode, "ticksToExplode", 0, false);
		}

		public override void Draw()
		{
			base.Draw();
			CompWaterTank comp = base.GetComp<CompWaterTank>();
			GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
			r.center = this.DrawPos + Vector3.up * 0.1f;
			r.size = Building_Boiler.BarSize;
			r.fillPercent = comp.StoredWater / comp.Props.storedWaterMax;
			r.filledMat = Building_Boiler.BatteryBarFilledMat;
			r.unfilledMat = Building_Boiler.BatteryBarUnfilledMat;
			r.margin = 0.15f;
			Rot4 rotation = base.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
			if (this.ticksToExplode > 0 && base.Spawned)
			{
				base.Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
			}
		}

		private static readonly Texture2D SetTargetFuelLevelCommand = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel", true);
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (var g in base.GetGizmos()) yield return g;

			if (compRefuelable != null)
			{
				yield return new Command_SetTargetFuelLevel
				{
					refuelable = compRefuelable,
					defaultLabel = "CommandSetTargetFuelLevel".Translate(),
					defaultDesc = "CommandSetTargetFuelLevelDesc".Translate(),
					icon = Building_Boiler.SetTargetFuelLevelCommand
				};

				yield return new Command_Action
				{
					icon = 
				}
			}
		}
		
		

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			compRefuelable = this.GetComp<CompRefuelable>();
		}

		public override void Tick()
		{
			base.Tick();
			if (this.ticksToExplode > 0)
			{
				if (this.wickSustainer == null)
				{
					this.StartWickSustainer();
				}
				else
				{
					this.wickSustainer.Maintain();
				}
				this.ticksToExplode--;
				if (this.ticksToExplode == 0)
				{
					IntVec3 randomCell = this.OccupiedRect().RandomCell;
					float radius = Rand.Range(0.5f, 1f) * 3f;
					GenExplosion.DoExplosion(randomCell, base.Map, radius, DamageDefOf.Flame, null, -1, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
					base.GetComp<CompSteamTank>().DrawSteam(400f);
				}
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostApplyDamage(dinfo, totalDamageDealt);
			if (!base.Destroyed && this.ticksToExplode == 0 && dinfo.Def == DamageDefOf.Flame && Rand.Value < 0.05f && base.GetComp<CompSteamTank>().StoredSteam > 500f)
			{
				this.ticksToExplode = Rand.Range(70, 150);
				this.StartWickSustainer();
			}
		}

		private void StartWickSustainer()
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			this.wickSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
		}

		private int ticksToExplode;

		private Sustainer wickSustainer;

		private static readonly Vector2 BarSize = new Vector2(1.3f, 0.4f);

		private const float MinSteamToExplode = 500f;

		private const float SteamToLoseWhenExplode = 400f;

		private const float ExplodeChancePerDamage = 0.05f;

		private static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f), false);

		private static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
		private CompRefuelable compRefuelable;
	}
}
