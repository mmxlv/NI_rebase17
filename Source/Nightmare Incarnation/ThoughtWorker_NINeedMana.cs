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
            CompSuccubusRace comp = pawn.GetComp<CompSuccubusRace>();
            bool isSuccubus = comp != null;
            ThoughtState result;
            if (!isSuccubus)
                return ThoughtState.Inactive;
            List<Need> allNeeds = pawn.needs.AllNeeds;
            Need need = allNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana);
            Need_Mana need_Mana = (need as Need_Mana);
            if (need != null && (need_Mana != null))
            {
                switch (need_Mana.CurCategory)
                {
                    case ManaAmountCategory.Satisfied:
                        return ThoughtState.Inactive;
                    case ManaAmountCategory.Insatisfied:
                        return ThoughtState.ActiveAtStage(0);
                    case ManaAmountCategory.Lacking:
                        return ThoughtState.ActiveAtStage(1);
                    case ManaAmountCategory.Poverty:
                        {
                            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(NIHediffDefOf.ManaDeficiency, false);
                            int currentDefficiencyStage = (firstHediffOfDef != null) ? firstHediffOfDef.CurStageIndex : 0;
                            result = ThoughtState.ActiveAtStage(2 + currentDefficiencyStage);
                            break;
                        }
                    default:
                        Log.Error("Not implemented mana need category encountered: " + need_Mana.CurCategory.ToString() + " This should never happen, setting thoughtstate to inactive");
                        return ThoughtState.Inactive;
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
