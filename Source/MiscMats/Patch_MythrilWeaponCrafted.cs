using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;

namespace MiscMats
{
    [StaticConstructorOnStartup]
    static class Patch_MythrilWeaponCrafted
    {
        private static ThingDef Mythril;

        static Patch_MythrilWeaponCrafted()
        {
            new Harmony("com.dninemfive.miscmats").PatchAll();
            Mythril = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.defName == "D9MM_Mythril").First();
        }

        [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
        public class GenRecipePostProcessProductPatch
        {
            [HarmonyPostfix]
            public static void PostProcessProductPostfix(Thing product)
            {
                if (product.Stuff != Mythril) return;
                CompQuality comp = product.TryGetComp<CompQuality>();
                if (comp == null) return;
                QualityCategory offsetQuality = (QualityCategory)Mathf.Clamp((int)comp.Quality + 1, (int)QualityCategory.Awful, (int)QualityCategory.Legendary);
                comp.SetQuality(offsetQuality, ArtGenerationContext.Colony);
                Messages.Message("D9MM_ThingMadeOfMythril".Translate(product.Label, offsetQuality.ToString()), product, MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}
