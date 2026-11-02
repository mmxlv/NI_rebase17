using System;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
    public class IngestionOutcomeDoer_RestoreMana : IngestionOutcomeDoer
    {
        public float manaFraction;
        public float foodPoisoningSeverity;
        public ThoughtDef moodThought;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (moodThought != null && pawn.needs?.mood?.thoughts?.memories != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(moodThought);
            }

            if (!CommonUtl.IsSuccubus(pawn))
            {
                if (foodPoisoningSeverity > 0f)
                {
                    HealthUtility.AdjustSeverity(pawn, HediffDefOf.FoodPoisoning, foodPoisoningSeverity);
                }
                return;
            }

            if (pawn.needs != null && pawn.needs.TryGetNeed<Need_Mana>() is Need_Mana manaNeed)
            {
                manaNeed.CurLevel = Math.Min(manaNeed.MaxLevel, manaNeed.CurLevel + manaNeed.MaxLevel * manaFraction * ingestedCount);
            }
        }
    }
}
