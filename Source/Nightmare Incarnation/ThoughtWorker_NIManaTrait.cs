using System;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
    public class ThoughtWorker_NIManaTrait : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
                return false;
            if (RelationsUtility.IsDisfigured(other))
                return false;
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
                return false;
            if (other.story == null || !commonUtl.IsSuccubus(other))
                return false;
            if (commonUtl.IsSuccubus(pawn) && commonUtl.IsSuccubus(other))
                return ThoughtState.ActiveAtStage(4);
            switch (other.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute))
            {
                case 1:
                    return ThoughtState.ActiveAtStage(0);
                case 2:
                case 3:
                    return ThoughtState.ActiveAtStage(1);
                case 4:
                case 5:
                    return ThoughtState.ActiveAtStage(2);
                case 6:
                    return ThoughtState.ActiveAtStage(3);
                default:
                    return false;
            }
        }
    }
}
