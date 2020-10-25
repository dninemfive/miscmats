using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace MiscMats
{
    /// <summary>
    /// Changes the quality of the parent Thing when it is created, then removes itself.
    /// </summary>
    public class CompQualityChange : ThingComp
    {
        public CompProperties_QualityChange Props => (CompProperties_QualityChange)base.props;

        public override void PostPostMake()
        {
            CompQuality comp = base.parent.GetComp<CompQuality>();
            QualityCategory offsetQuality = (QualityCategory)Mathf.Clamp((int)comp.Quality + Props.qualityChange, (int)QualityCategory.Awful, (int)QualityCategory.Legendary);
            // Will regenerate art in colony context in the MakeRecipeProducts patch
            comp.SetQuality(offsetQuality, ArtGenerationContext.Outsider);
            base.parent.AllComps.Remove(this);
        }
    }
    public class CompProperties_QualityChange : CompProperties
    {
#pragma warning disable CS0649
        public int qualityChange = 1;
#pragma warning restore CS0649

        public CompProperties_QualityChange()
        {
            base.compClass = typeof(CompQualityChange);
        }
    }
}
