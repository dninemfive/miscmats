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
    class HediffComp_Trip : HediffComp
    {
        public HediffCompProperties_Trip Props => (HediffCompProperties_Trip)base.props;
        public float B => -Mathf.Log(0.1f) / (Props.hoursForTenPercentMoodWeight * GenDate.TicksPerHour);
        // if negative, bad trip; if positive, good trip; if 0 or close to it, neutral trip
        public float tripValue;
        private int hashOffset = 0;
        public bool IsCheapIntervalTick(int interval) => (int)(Find.TickManager.TicksGame + hashOffset) % interval == 0;
        public int Stage
        {
            get
            {
                if (tripValue < (-Props.neutralityThreshold)) return 0;
                if (tripValue > Props.neutralityThreshold) return 2;
                return 1;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            List<Thought> thoughts = new List<Thought>();
            base.parent.pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
            tripValue = 0;
            // todo: move this into its own method; recalculate every so often
            foreach(Thought th in thoughts)
            {
                if (th is Thought_Memory thm)
                {
                    // exponential decay of mood effect, so (by default) six-hour-old thoughts are worth 10% as much as fresh ones. Basically, current mindset -> trip quality
                    tripValue += thm.MoodOffset() * Mathf.Pow((float)Math.E, B * thm.age);
                }
                else
                {
                    // thoughts without an age are simply weighted at 10% their strength
                    tripValue += th.MoodOffset() * 0.1f;
                }
            }
            hashOffset = parent.pawn.thingIDNumber.HashOffset();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            // effects:
            //  - chance for inspiration based on trip quality (0 for neutral or less; increasing as it gets better)
            //  - negative mental states with a negative trip quality
            // - transient thoughts? would be selected by weight from a small list, or perhaps with a RulePackDef or smth
            //  - stat offsets: reduced consciousness; increased blood filtration, psychic sensitivity, (Royalty) psycasting recovery rate
            if (IsCheapIntervalTick(Props.tickInterval))
            {
                if(tripValue > Props.neutralityThreshold)
                {
                    if (Rand.Value < Props.baseInspirationChancePerInterval * Mathf.Sqrt(tripValue))
                        parent.pawn.mindState.inspirationHandler.TryStartInspiration_NewTemp(GetRandomAvailableInspirationDef(), "D9MM_InspirationFromTrip".Translate());
                }
                else if(tripValue < (-Props.neutralityThreshold))
                {
                    if (Rand.Value < Props.baseMentalStateChancePerInterval * Mathf.Sqrt(-tripValue))
                        parent.pawn.mindState.mentalStateHandler.TryStartMentalState(GetRandomAvailableMentalStateDef(), 
                            "D9MM_MentalStateFromTrip".Translate(), 
                            causedByMood: false, 
                            transitionSilently: Props.transitionMentalStateSilently);
                }
            }
        }

        // Tynan, there is no reason this should be private
        public InspirationDef GetRandomAvailableInspirationDef()
        {
            return (from x in DefDatabase<InspirationDef>.AllDefsListForReading
                    where x.Worker.InspirationCanOccur(parent.pawn)
                    select x).RandomElementByWeightWithFallback((InspirationDef x) => x.Worker.CommonalityFor(parent.pawn), null);
        }
        public MentalStateDef GetRandomAvailableMentalStateDef()
        {
            return (from x in DefDatabase<MentalStateDef>.AllDefsListForReading
                    where x.category != MentalStateCategory.Aggro && x.Worker.StateCanOccur(parent.pawn)
                    select x).RandomElement();
        }
    }
    class HediffCompProperties_Trip : HediffCompProperties
    {
#pragma warning disable CS0649
        public float hoursForTenPercentMoodWeight = 6f;
        public int neutralityThreshold = 5;
        public int tickInterval = 250;
        public float baseInspirationChancePerInterval = 0.01f;
        public float baseMentalStateChancePerInterval = 0.1f;
        public bool transitionMentalStateSilently = true;
#pragma warning restore CS0649
        public HediffCompProperties_Trip()
        {
            base.compClass = typeof(HediffComp_Trip);
        }
    }
}
