using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    /// <summary>
    /// Collection of Utility methods for Succubus interactions
    /// </summary>

    public static class CommonUtl
    {
        /// <summary>
        /// Checks if a given pawn is passive
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool IsPassive(Pawn pawn)
        {
            return !pawn.InAggroMentalState
                   && ((pawn.Faction == Faction.OfPlayer && !pawn.Drafted)
                       || pawn.IsPrisonerOfColony
                       || (pawn.Faction != Faction.OfPlayer && pawn.Downed));
        }


        /// <summary>
        /// Gets the weight of how much a succubus wants to succ the target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="succubus"></param>
        /// <returns></returns>
        public static float GetPreferWeight(Pawn target, Pawn succubus)
        {
            if (!IsSuccubus(succubus)
                || target.story == null
                || succubus.story == null)
            {
                return 0f;
            }
            if (IsInRomanticRelationship(target, succubus))
            {
                return 3f;
            }
            if (succubus.story.traits.HasTrait(TraitDefOf.DislikesMen) && target.gender == Gender.Male)
            {
                return 0.01f;
            }
            if (succubus.story.traits.HasTrait(TraitDefOf.DislikesWomen) && target.gender == Gender.Female)
            {
                return 0.01f;
            }
            if (succubus.story.traits.HasTrait(TraitDefOf.Gay))
            {
                return (target.gender == succubus.gender) ? 2f : 0.1f;
            }
            else
            {
                return 1f;
            }
        }

        private static bool IsInRomanticRelationship(Pawn fistPawn, Pawn otherPawn)
        {
            return PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Lover
                   || PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Fiance
                   || PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Spouse;
        }

        public static bool IsSuccubus(Pawn pawn) => pawn.def.defName == "NI_Race";
    }
}
