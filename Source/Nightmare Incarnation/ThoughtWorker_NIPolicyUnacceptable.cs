using System;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
    public class ThoughtWorker_NIPolicyUnacceptable : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (!CommonUtl.IsSuccubus(pawn))
                return ThoughtState.Inactive;

            CompSuccubusRace succubusComp = pawn.GetComp<CompSuccubusRace>();
            if (succubusComp == null)
            {
                Log.Error("Succubus without Comp");
                return ThoughtState.Inactive;
            }

            int manaAttributeDegree = pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute);

            if (succubusComp.policy.plans.Any(x => x.AllowJoy)
                && (manaAttributeDegree == 1
                    || pawn.story.traits.HasTrait(TraitDefOf.Kind)
                    || pawn.story.traits.HasTrait(TraitDefOf.Ascetic)))
            {
                return ThoughtState.ActiveAtStage(0);
            }

            if (manaAttributeDegree == 3)
            {
                if (!succubusComp.policy.plans.Any(x => x.AllowJoy))
                    return ThoughtState.ActiveAtStage(1);
                if (succubusComp.policy.plans.Any(x => x.MainDishPawn != null))
                    return ThoughtState.ActiveAtStage(2);
            }

            if (manaAttributeDegree == 5 && !succubusComp.policy.plans.Any(x => x.AllowOverSqueeze))
                return ThoughtState.ActiveAtStage(3);

            if (pawn.story.traits.HasTrait(TraitDefOf.Bloodlust)
                && !succubusComp.policy.plans.Any(x => x.targetType != RelationType.Colonist && x.AllowKill))
            {
                return ThoughtState.ActiveAtStage(4);
            }

            return ThoughtState.Inactive;
        }
    }
}
