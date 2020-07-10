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
            Need_Mana need_Mana = pawn.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
            if (need_Mana != null)
            {
                if(GenCollection.Any<ThingComp>(pawn.AllComps, (ThingComp x) => x is CompSuccubusRace))
                {
                    
                    if (!FoodUtility.ShouldBeFedBySomeone(pawn))
                    {
                        if (need_Mana.CurCategory > ManaAmountCategory.Satisfied)
                            return 9.5f;
                        if (need_Mana.CurLevelPercentage < 0.4f)
                             return 7f;
                        if (need_Mana.CurLevelPercentage < 0.8f)
                            return 2f;
                    }
                }
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompSuccubusRace comp = pawn.GetComp<CompSuccubusRace>();
            if (comp == null)
            {
                Log.Message("Job_NIManaSqueeze is unique to the Succubus races.", false);
                return null;
            }
            else
            {
                float priority = this.GetPriority(pawn);
                if (priority > 6f)
                {
                    comp.tmpJoyMode = false;
                }
                else
                {
                    bool canDoJob = priority > 0f && pawn.needs.joy.CurLevelPercentage < comp.JoyThreshold;
                    if (canDoJob)
                    {
                        float succsessChance = 0.5f * (comp.JoyThreshold - pawn.needs.joy.CurLevelPercentage) * Mathf.Clamp01(0.5f * pawn.needs.joy.tolerances.JoyFactorFromTolerance(NIJoyKindDefOf.JoyKind_NISuccubusWork) / comp.JoyTolerancesFactor);
                        if (!Rand.Chance(succsessChance))
                        {
                            return null;
                        }
                        comp.tmpJoyMode = true;
                    }
                        return null;
                }

                for (int i = 0; i < comp.policy.plans.Count; i++)
                {
                    bool suspended = comp.policy.plans[i].Suspended;
                    if (!suspended)
                    {
                        if (comp.policy.plans[i].targetType == RelationType.Meal && priority > 6f)
                        {
                            Thing thing = null;
                            ThingDef thingDef = null;
                            commonUtl.TryFindBestManaSourceFor(pawn, pawn, out thing, out thingDef, true, false, false);
                            if (thing != null)
                            {
                                return new Job(NIJobDefOf.Job_NIIngestManaItem, thing)
                                {
                                    count = commonUtl.EstimateIngestCountOf(pawn, thing)
                                };
                            }
                        }
                        else
                        {
                            bool joyAllowed = comp.tmpJoyMode && !comp.policy.plans[i].AllowJoy;
                            if (!joyAllowed)
                            {
                                bool foodSourcePawnExists = comp.policy.plans[i].MainDishPawn != null;
                                Pawn foodSourcePawn;
                                if (foodSourcePawnExists)
                                {
                                    foodSourcePawn = comp.policy.plans[i].MainDishPawn;
                                    bool pawnIsValidFoodSource = foodSourcePawn.Faction != null && foodSourcePawn.Faction == Faction.OfPlayer && foodSourcePawn != pawn && ((comp.policy.plans[i].targetType == RelationType.Colonist && foodSourcePawn.IsColonist) || (comp.policy.plans[i].targetType == RelationType.Prisoner && foodSourcePawn.IsPrisonerOfColony)) && foodSourcePawn.GetComp<CompSuccubusRace>() == null && commonUtl.IsManaConformant(foodSourcePawn) && commonUtl.IsPassive(foodSourcePawn) && commonUtl.HasEnoughtMana(foodSourcePawn, comp.policy.plans[i], comp.tmpJoyMode ? 0.2f : 0f) && ReservationUtility.CanReserveAndReach(pawn, foodSourcePawn,PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false);
                                    if (pawnIsValidFoodSource)
                                    {
                                        comp.tmpTargetType = comp.policy.plans[i].targetType;
                                        return new Job(NIJobDefOf.Job_NIManaSqueeze, foodSourcePawn);
                                    }
                                }
                                JobGiver_GetMana.tmpCandidates.Clear();
                                JobGiver_GetMana.tmpCandidates = commonUtl.GetPawnsByManaPolicy(pawn, comp.policy.plans[i], comp.tmpJoyMode ? 0.2f : 0f);
                                if (JobGiver_GetMana.tmpCandidates.Count <= 0)
                                {
                                    foodSourcePawn = null;
                                }
                                else
                                {
                                    IEnumerable<Pawn> enumerable = JobGiver_GetMana.tmpCandidates;
                                    Func<Pawn, float> func = (Pawn x) => commonUtl.GetPreferWeight(x, pawn);
                                    foodSourcePawn = GenCollection.RandomElementByWeight<Pawn>(enumerable, func);
                                }
                                if (foodSourcePawn != null)
                                {
                                    comp.tmpTargetType = comp.policy.plans[i].targetType;
                                    return new Job(NIJobDefOf.Job_NIManaSqueeze, foodSourcePawn);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            float priority = this.GetPriority(pawn);
            ThinkResult noJob;
            if (priority <= 0f)
            {
                return ThinkResult.NoJob;
            }
            else
            {
                Job job=null;
                int ticksGame = Find.TickManager.TicksGame;
                if (priority > 9f && ticksGame > this.nextTryableTick && job == null)
                {
                    this.nextTryableTick = ticksGame + 30;
                    job = this.TryGiveJob(pawn);
                }
                if (priority > 6f && ticksGame > this.nextTryableTick && job == null)
                {
                    this.nextTryableTick = ticksGame + 60;
                    job = this.TryGiveJob(pawn);
                }
                if (priority > 0f && ticksGame > this.nextTryableTick && job == null)
                {
                    this.nextTryableTick = ticksGame + 300;
                    Job job3 = this.TryGiveJob(pawn);
                }
                if(job!=null)
                    return new ThinkResult(job, this, new JobTag?(JobTag.SatisfyingNeeds), false);
            }
            return ThinkResult.NoJob;
        }

    }
}
