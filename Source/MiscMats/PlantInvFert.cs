using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace MiscMats
{
    class PlantInvFert : Plant
    {
        /*public override bool HarvestableSoon
        {
            get
            {
                if (HarvestableNow) return true;
                if (!base.def.plant.Harvestable) return false;
                float gd = base.def.plant.growDays;
                return (Mathf.Max(1f - Growth, 0f) * gd <= 10f || Mathf.Max(1f - base.def.plant.harvestMinGrowth, 0f) * gd <= 1f) && GRF_Fertility > 0f && GrowthRateFactor_Temperature >= 0f;
            }
        }*/
        public override float GrowthRate
        {
            get
            {
                if (Blighted || (base.Spawned && !PlantUtility.GrowthSeasonNow(base.Position, base.Map))) return 0f;
                return GRF_Fertility * GrowthRateFactor_Light * GrowthRateFactor_Temperature;
            }
        }
        //equiv. to GrowthRateFactor_Fertility and replaces it everywhere originally used. Couldn't override original. Yields high values for low fertility and vice versa. Fertility sensitivity should still be entered as positive values.
        public float GRF_Fertility => Mathf.Max(new float[] { (2 - base.Map.fertilityGrid.FertilityAt(base.Position) * base.def.plant.fertilitySensitivity), 0f}); //Mathf.Abs((base.Map.fertilityGrid.FertilityAt(base.Position) * (1-base.def.plant.fertilitySensitivity)) + base.def.plant.fertilitySensitivity);
        //because this DLL is only used for Stoneseed, remove yield randomness
        public override int YieldNow()
        {
            return CanYieldNow() ? (int)base.def.plant.harvestYield : 0;
        }
    }
}
