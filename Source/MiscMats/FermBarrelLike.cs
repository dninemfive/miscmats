using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace MiscMats
{
    public class FermBarrelLike : Building
    {
        private int ingCt; //"ingredient count"

        private float progressInt;

        private Material barFilledCachedMat;

        public int MaxCapacity => def.GetModExtension<FBLModExtension>().maxCapacity;

        private int BaseDuration => def.GetModExtension<FBLModExtension>().baseDuration;
        
        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

        private Color BarZeroProgressColor => def.GetModExtension<FBLModExtension>().barColorEmpty;

        private Color BarFermentedColor => def.GetModExtension<FBLModExtension>().barColor;

        private Material BarUnfilledMat => SolidColorMaterials.SimpleSolidColorMaterial(def.GetModExtension<FBLModExtension>().barColorBg, false);

        public float Progress
        {
            get
            {
                return progressInt;
            }
            set
            {
                if (value != progressInt)
                {
                    progressInt = value;
                    barFilledCachedMat = null;
                }
            }
        }

        private Material BarFilledMat
        {
            get
            {
                if ((UnityEngine.Object)barFilledCachedMat == (UnityEngine.Object)null)
                {
                    barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(BarZeroProgressColor, BarFermentedColor, Progress), false);
                }
                return barFilledCachedMat;
            }
        }

        public int SpaceLeftForIngredient
        {
            get
            {
                if (Fermented)
                {
                    return 0;
                }
                return MaxCapacity - ingCt;
            }
        }

        private bool Empty => ingCt <= 0;

        public bool Fermented => !Empty && Progress >= 1f;

        private float CurrentTempProgressSpeedFactor
        {
            get
            {
                CompProperties_TemperatureRuinable compProperties = base.def.GetCompProperties<CompProperties_TemperatureRuinable>();
                float ambientTemperature = base.AmbientTemperature;
                if (ambientTemperature < compProperties.minSafeTemperature)
                {
                    return 0.1f;
                }
                if (ambientTemperature < 7f)
                {
                    return GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, ambientTemperature);
                }
                return 1f;
            }
        }

        private float ProgressPerTickAtCurrentTemp => 2.77777781E-06f * CurrentTempProgressSpeedFactor;

        private int EstimatedTicksLeft => Mathf.Max(Mathf.RoundToInt((1f - Progress) / ProgressPerTickAtCurrentTemp), 0);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ingCt, "ingCt", 0, false);
            Scribe_Values.Look(ref progressInt, "progress", 0f, false);
        }

        public override void TickRare()
        {
            base.TickRare();
            if (!Empty)
            {
                Progress = Mathf.Min(Progress + 250f * ProgressPerTickAtCurrentTemp, 1f);
            }
        }

        public void AddWort(int count)
        {
            base.GetComp<CompTemperatureRuinable>().Reset();
            if (Fermented)
            {
                Log.Warning("Tried to add ingredients to a FermBarrelLike when it was full of products.", false);
            }
            else
            {
                int num = Mathf.Min(count, MaxCapacity - ingCt);
                if (num > 0)
                {
                    Progress = GenMath.WeightedAverage(0f, (float)num, Progress, (float)ingCt);
                    ingCt += num;
                }
            }
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByTemperature")
            {
                Reset();
            }
        }

        private void Reset()
        {
            ingCt = 0;
            Progress = 0f;
        }

        public void AddWort(Thing wort)
        {
            int num = Mathf.Min(wort.stackCount, MaxCapacity - ingCt);
            if (num > 0)
            {
                AddWort(num);
                wort.SplitOff(num).Destroy(DestroyMode.Vanish);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>();
            if (!Empty && !comp.Ruined)
            {
                if (Fermented)
                {
                    stringBuilder.AppendLine("ContainsBeer".Translate(ingCt, MaxCapacity));
                }
                else
                {
                    stringBuilder.AppendLine("ContainsWort".Translate(ingCt, MaxCapacity));
                }
            }
            if (!Empty)
            {
                if (Fermented)
                {
                    stringBuilder.AppendLine("Fermented".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("FermentationProgress".Translate(Progress.ToStringPercent(), EstimatedTicksLeft.ToStringTicksToPeriod()));
                    if (CurrentTempProgressSpeedFactor != 1f)
                    {
                        stringBuilder.AppendLine("FermentationBarrelOutOfIdealTemperature".Translate(CurrentTempProgressSpeedFactor.ToStringPercent()));
                    }
                }
            }
            stringBuilder.AppendLine("Temperature".Translate() + ": " + base.AmbientTemperature.ToStringTemperature("F0"));
            stringBuilder.AppendLine("IdealFermentingTemperature".Translate() + ": " + 7f.ToStringTemperature("F0") + " ~ " + comp.Props.maxSafeTemperature.ToStringTemperature("F0"));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public Thing TakeOutBeer()
        {
            if (!Fermented)
            {
                Log.Warning("Tried to get beer but it's not yet fermented.", false);
                return null;
            }
            Thing thing = ThingMaker.MakeThing(ThingDefOf.Beer, null);
            thing.stackCount = ingCt;
            Reset();
            return thing;
        }

        public override void Draw()
        {
            base.Draw();
            if (!Empty)
            {
                Vector3 drawPos = DrawPos;
                drawPos.y += 0.046875f;
                drawPos.z += 0.25f;
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = drawPos;
                r.size = BarSize;
                r.fillPercent = (float)ingCt / 25f;
                r.filledMat = BarFilledMat;
                r.unfilledMat = BarUnfilledMat;
                r.margin = 0.1f;
                r.rotation = Rot4.North;
                GenDraw.DrawFillableBar(r);
            }
        }

        //copied from https://github.com/josh-m/RW-Decompile/blob/master/RimWorld/Building_FermentingBarrel.cs since ILSpy fucks up when reading things with Gizmos
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode && !this.Empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 1",
                    action = delegate
                    {
                        this.Progress = 1f;
                    }
                };
            }
        }
    }
}