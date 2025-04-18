using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PMP
{
    [StaticConstructorOnStartup]
    public class CompWatermill : CompPowerPlant
    {
        private float spinPosition;
        private bool cacheDirty = true;
        private bool waterUsable;
        private float spinRate = 1f;
        private const float SpinRateFactor = 1f / 150f;
        private const float BladeOffset = 2.36f;
        private const int BladeCount = 9;

        public static readonly Material BladesMat = MaterialPool.MatFrom("Things/Building/Power/WatermillGenerator/WatermillGeneratorBlades");

        protected override float DesiredPowerOutput
        {
            get
            {
                if (cacheDirty)
                {
                    RebuildCache();
                }
                if (!waterUsable)
                {
                    return 0f;
                }
                return base.DesiredPowerOutput;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            spinPosition = Rand.Range(0f, 15f);
            RebuildCache();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref spinPosition, "spinPosition");
        }

        private void ClearCache()
        {
            cacheDirty = true;
        }

        private void RebuildCache()
        {
            waterUsable = true;
            foreach (IntVec3 item in WaterCells())
            {
                if (!item.InBounds(parent.Map) || !parent.Map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.MovingFluid))
                {
                    waterUsable = false;
                    break;
                }
            }

            if (!waterUsable)
            {
                spinRate = 0f;
                return;
            }

            Vector3 zero = Vector3.zero;
            foreach (IntVec3 item in WaterCells())
            {
                zero += parent.Map.waterInfo.GetWaterMovement(item.ToVector3Shifted());
            }
            spinRate = Mathf.Sign(Vector3.Dot(zero, parent.Rotation.Rotated(RotationDirection.Clockwise).FacingCell.ToVector3()));
            spinRate *= Rand.RangeSeeded(0.9f, 1.1f, parent.thingIDNumber * 60509 + 33151);
            cacheDirty = false;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (base.PowerOutput > 0.01f)
            {
                spinPosition = (spinPosition + SpinRateFactor * spinRate + (float)Math.PI * 2f) % ((float)Math.PI * 2f);
            }
        }

        public IEnumerable<IntVec3> WaterCells()
        {
            return WaterCells(parent.Position, parent.Rotation);
        }

        public static IEnumerable<IntVec3> WaterCells(IntVec3 loc, Rot4 rot)
        {
            IntVec3 perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
            yield return loc + rot.FacingCell * 3;
            yield return loc + rot.FacingCell * 3 - perpOffset;
            yield return loc + rot.FacingCell * 3 - perpOffset * 2;
            yield return loc + rot.FacingCell * 3 + perpOffset;
            yield return loc + rot.FacingCell * 3 + perpOffset * 2;
        }

        public CellRect WaterUseRect()
        {
            return WaterUseRect(parent.Position, parent.Rotation);
        }

        public static CellRect WaterUseRect(IntVec3 loc, Rot4 rot)
        {
            int width = rot.IsHorizontal ? 7 : 13;
            int height = rot.IsHorizontal ? 13 : 7;
            return CellRect.CenteredOn(loc + rot.FacingCell * 4, width, height);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            Vector3 center = parent.DrawPos + parent.Rotation.FacingCell.ToVector3() * BladeOffset;
            center.y += Altitudes.AltInc * 2;

            for (int i = 0; i < BladeCount; i++)
            {
                float angle = spinPosition + (float)Math.PI * 2f * i / BladeCount;
                float bladeWidth = Mathf.Abs(4f * Mathf.Sin(angle));
                bool flip = angle % ((float)Math.PI * 2f) < (float)Math.PI;
                
                Matrix4x4 matrix = Matrix4x4.TRS(
                    center,
                    parent.Rotation.AsQuat,
                    new Vector3(bladeWidth, 1f, 1f)
                );
                
                Graphics.DrawMesh(
                    flip ? MeshPool.plane10 : MeshPool.plane10Flip,
                    matrix,
                    BladesMat,
                    0
                );
            }
        }

        public override string CompInspectStringExtra()
        {
            return ""; // Suppress default power output string
        }
    }
}
