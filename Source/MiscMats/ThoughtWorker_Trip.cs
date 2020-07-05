using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace MiscMats
{
    public class ThoughtWorker_Trip : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(base.def.hediff);
            HediffComp_Trip trip;
            if((trip = hediff?.TryGetComp<HediffComp_Trip>()) != null)
            {
                return ThoughtState.ActiveAtStage(trip.Stage);
            }
            return ThoughtState.Inactive;
        }
    }
}