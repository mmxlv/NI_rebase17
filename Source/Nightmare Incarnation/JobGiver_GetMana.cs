using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    /// <summary>
    /// SuccJobGiver
    /// </summary>
    public class JobGiver_GetMana : ThinkNode_JobGiver
    {

        private int nextTryableTick = -1;

        private static List<Pawn> tmpCandidates = new List<Pawn>();

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GetMana jobGiver_GetMana = (JobGiver_GetMana)base.DeepCopy(resolve);
            jobGiver_GetMana.nextTryableTick = this.nextTryableTick;
            return jobGiver_GetMana;
        }


        public override float GetPriority(Pawn pawn)
        {
            if (pawn.needs.TryGetNeed<Need_Mana>() is Need_Mana needMana
                && commonUtl.IsSuccubus(pawn)
                && !FoodUtility.ShouldBeFedBySomeone(pawn))
            {
                if (needMana.CurCategory > ManaAmountCategory.Satisfied)
                    return 9.5f;
                if (needMana.CurLevelPercentage < 0.4f)
                    return 7f;
                if (needMana.CurLevelPercentage < 0.8f)
                    return 2f;
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompSuccubusRace comp = pawn.GetComp<CompSuccubusRace>();
            if (comp == null)
            {
                Log.Error("Job_NIManaSqueeze is unique to the Succubus races.", false);
                return null;
            }

            float priority = GetPriority(pawn);
            if (priority <= 0f)
                return null;

            if (priority > 6f)
            {
                comp.tmpJoyMode = false;
            }
            else
            {
                if (pawn.needs.joy.CurLevelPercentage >= comp.JoyThreshold)
                {
                    return null;
                }
                float succsessChance = 0.5f
                                       * (comp.JoyThreshold - pawn.needs.joy.CurLevelPercentage)
                                       * Mathf.Clamp01(0.5f
                                                       * pawn.needs.joy.tolerances.JoyFactorFromTolerance(NIJoyKindDefOf.JoyKind_NISuccubusWork)
                                                       / comp.JoyTolerancesFactor);
                if (!Rand.Chance(succsessChance))
                {
                    return null;
                }
                comp.tmpJoyMode = true;
            }

            for (int i = 0; i < comp.policy.plans.Count; i++)
            {
                WorkPlan currentPlan = comp.policy.plans[i];
                if (!currentPlan.Suspended)
                {
                    if (currentPlan.targetType == RelationType.Meal && priority > 6f)
                    {
                        commonUtl.TryFindBestManaSourceFor(pawn, pawn, out Thing manaSource, out _);
                        if (manaSource != null)
                        {
                            return new Job(NIJobDefOf.Job_NIIngestManaItem, manaSource)
                            {
                                count = commonUtl.EstimateIngestCountOf(pawn, manaSource)
                            };
                        }
                    }
                    // not meal, so only pawn sources remaining
                    else if (!comp.tmpJoyMode || currentPlan.AllowJoy)
                    {
                        Pawn foodSourcePawn = currentPlan.MainDishPawn;
                        if (foodSourcePawn != null
                            && foodSourcePawn.Faction == Faction.OfPlayer
                            && ((currentPlan.targetType == RelationType.Colonist && foodSourcePawn.IsColonist)
                                || (currentPlan.targetType == RelationType.Prisoner && foodSourcePawn.IsPrisonerOfColony))
                            && !commonUtl.IsSuccubus(foodSourcePawn)
                            && commonUtl.IsManaConformant(foodSourcePawn)
                            && commonUtl.IsPassive(foodSourcePawn)
                            && commonUtl.HasEnoughtMana(foodSourcePawn, currentPlan, comp.tmpJoyMode ? 0.2f : 0f)
                            && ReservationUtility.CanReserveAndReach(pawn, foodSourcePawn, PathEndMode.OnCell, Danger.Deadly))
                        {
                            comp.tmpTargetType = currentPlan.targetType;
                            return new Job(NIJobDefOf.Job_NIManaSqueeze, foodSourcePawn);
                        }
                        // no pawn specified in workPlan, find one.
                        tmpCandidates.Clear();
                        tmpCandidates = commonUtl.GetPawnsByManaPolicy(pawn, currentPlan, comp.tmpJoyMode ? 0.2f : 0f);
                        if (tmpCandidates.Count > 0)
                        {
                            float func(Pawn x) => commonUtl.GetPreferWeight(x, pawn);
                            foodSourcePawn = GenCollection.RandomElementByWeight(tmpCandidates, func);
                            comp.tmpTargetType = currentPlan.targetType;
                            return new Job(NIJobDefOf.Job_NIManaSqueeze, foodSourcePawn);
                        }
                    }
                }
            }
            return null;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            float priority = GetPriority(pawn);
            if (priority <= 0f)
            {
                return ThinkResult.NoJob;
            }
            Job job = null;
            int ticksGame = Find.TickManager.TicksGame;

            if (priority > 9f && ticksGame > nextTryableTick && job == null)
            {
                nextTryableTick = ticksGame + 30;
                job = TryGiveJob(pawn);
            }
            if (priority > 6f && ticksGame > nextTryableTick && job == null)
            {
                nextTryableTick = ticksGame + 60;
                job = TryGiveJob(pawn);
            }
            if (priority > 0f && ticksGame > nextTryableTick && job == null)
            {
                nextTryableTick = ticksGame + 300;
                job = TryGiveJob(pawn);
            }
            if (job != null)
                return new ThinkResult(job, this, new JobTag?(JobTag.SatisfyingNeeds), false);
            return ThinkResult.NoJob;
        }

    }
}
