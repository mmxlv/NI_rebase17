using System;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
    public class ThoughtWorker_NIPolicyUnacceptable : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (pawn.story == null)
                return ThoughtState.Inactive;
            
            CompSuccubusRace succubusComp = pawn.GetComp<CompSuccubusRace>();
            if (succubusComp == null)
                return ThoughtState.Inactive;
            int manaAttributeDegree = pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute);
            if (manaAttributeDegree == 1 || pawn.story.traits.HasTrait(TraitDefOf.Kind) || pawn.story.traits.HasTrait(TraitDefOf.Ascetic))
                if (GenCollection.Any<WorkPlan>(succubusComp.policy.plans, (WorkPlan x) => x.AllowJoy))
                    return ThoughtState.ActiveAtStage(0);
            if (manaAttributeDegree == 3)
                if (!GenCollection.Any<WorkPlan>(succubusComp.policy.plans, (WorkPlan x) => x.AllowJoy))
                    return ThoughtState.ActiveAtStage(1);
                if (GenCollection.Any<WorkPlan>(succubusComp.policy.plans, (WorkPlan x) => x.MainDishPawn != null))
                    return ThoughtState.ActiveAtStage(2);
            if (manaAttributeDegree == 5)
                if (!GenCollection.Any<WorkPlan>(succubusComp.policy.plans, (WorkPlan x) => x.AllowOverSqueeze))
                    return ThoughtState.ActiveAtStage(3);
            if (pawn.story.traits.HasTrait(TraitDefOf.Bloodlust))
                if (!GenCollection.Any<WorkPlan>(succubusComp.policy.plans, (WorkPlan x) => x.targetType != RelationType.Colonist && x.AllowKill))
                    return ThoughtState.ActiveAtStage(4);
            return ThoughtState.Inactive;
        }
    }
}
