using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
    public class ThoughtWorker_NINeedMana : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (!CommonUtl.IsSuccubus(pawn))
                return ThoughtState.Inactive;

            if (pawn.needs.TryGetNeed<Need_Mana>() is Need_Mana needMana)
            {
                switch (needMana.CurCategory)
                {
                    case ManaAmountCategory.Satisfied:
                        return ThoughtState.Inactive;
                    case ManaAmountCategory.Insatisfied:
                        return ThoughtState.ActiveAtStage(0);
                    case ManaAmountCategory.Lacking:
                        return ThoughtState.ActiveAtStage(1);
                    case ManaAmountCategory.Poverty:
                        {
                            int currentDefficiencyStage = 0;
                            if (pawn.health.hediffSet.GetFirstHediffOfDef(NIHediffDefOf.ManaDeficiency) is Hediff hediff)
                                currentDefficiencyStage = hediff.CurStageIndex;

                            return ThoughtState.ActiveAtStage(2 + currentDefficiencyStage);
                        }
                    default:
                        Log.Error("Not implemented mana need category encountered: " + needMana.CurCategory.ToString() + " This should never happen, setting thoughtstate to inactive");
                        return ThoughtState.Inactive;
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
