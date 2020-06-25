using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	// Token: 0x0200000B RID: 11
	public class JobGiver_GetMana : ThinkNode_JobGiver
	{
		// Token: 0x0600003A RID: 58 RVA: 0x00004468 File Offset: 0x00002668
		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetMana jobGiver_GetMana = (JobGiver_GetMana)base.DeepCopy(resolve);
			jobGiver_GetMana.nextTryableTick = this.nextTryableTick;
			return jobGiver_GetMana;
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00004494 File Offset: 0x00002694
		public override float GetPriority(Pawn pawn)
		{
			Need_Mana need_Mana = pawn.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
			bool flag = need_Mana == null;
			float result;
			if (flag)
			{
				result = 0f;
			}
			else
			{
				bool flag2 = !GenCollection.Any<ThingComp>(pawn.AllComps, (ThingComp x) => x is CompSuccubusRace);
				if (flag2)
				{
					result = 0f;
				}
				else
				{
					bool flag3 = FoodUtility.ShouldBeFedBySomeone(pawn);
					if (flag3)
					{
						result = 0f;
					}
					else
					{
						bool flag4 = need_Mana.CurCategory > ManaAmountCategory.Satisfied;
						if (flag4)
						{
							result = 9.5f;
						}
						else
						{
							bool flag5 = need_Mana.CurLevelPercentage < 0.4f;
							if (flag5)
							{
								result = 7f;
							}
							else
							{
								bool flag6 = need_Mana.CurLevelPercentage < 0.8f;
								if (flag6)
								{
									result = 2f;
								}
								else
								{
									result = 0f;
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00004594 File Offset: 0x00002794
		protected override Job TryGiveJob(Pawn pawn)
		{
			CompSuccubusRace comp = pawn.GetComp<CompSuccubusRace>();
			bool flag = comp == null;
			Job result;
			if (flag)
			{
				Log.Message("Job_NIManaSqueeze is unique to the Succubus races.", false);
				result = null;
			}
			else
			{
				float priority = this.GetPriority(pawn);
				bool flag2 = priority > 6f;
				if (flag2)
				{
					comp.tmpJoyMode = false;
				}
				else
				{
					bool flag3 = priority > 0f && pawn.needs.joy.CurLevelPercentage < comp.JoyThreshold;
					if (!flag3)
					{
						return null;
					}
					float num = 0.5f * (comp.JoyThreshold - pawn.needs.joy.CurLevelPercentage) * Mathf.Clamp01(0.5f * pawn.needs.joy.tolerances.JoyFactorFromTolerance(NIJoyKindDefOf.JoyKind_NISuccubusWork) / comp.JoyTolerancesFactor);
					bool flag4 = Rand.Chance(num);
					if (!flag4)
					{
						return null;
					}
					comp.tmpJoyMode = true;
				}
				Func<Pawn, float> <>9__0;
				for (int i = 0; i < comp.policy.plans.Count; i++)
				{
					bool suspended = comp.policy.plans[i].Suspended;
					if (!suspended)
					{
						bool flag5 = comp.policy.plans[i].targetType == RelationType.Meal && priority > 6f;
						if (flag5)
						{
							Thing thing = null;
							ThingDef thingDef = null;
							bool flag6 = commonUtl.TryFindBestManaSourceFor(pawn, pawn, out thing, out thingDef, true, false, false);
							bool flag7 = thing != null;
							if (flag7)
							{
								return new Job(NIJobDefOf.Job_NIIngestManaItem, thing)
								{
									count = commonUtl.EstimateIngestCountOf(pawn, thing)
								};
							}
						}
						else
						{
							bool flag8 = comp.tmpJoyMode && !comp.policy.plans[i].AllowJoy;
							if (!flag8)
							{
								bool flag9 = comp.policy.plans[i].MainDishPawn != null;
								Pawn pawn2;
								if (flag9)
								{
									pawn2 = comp.policy.plans[i].MainDishPawn;
									bool flag10 = pawn2.Faction != null && pawn2.Faction == Faction.OfPlayer && pawn2 != pawn && ((comp.policy.plans[i].targetType == RelationType.Colonist && pawn2.IsColonist) || (comp.policy.plans[i].targetType == RelationType.Prisoner && pawn2.IsPrisonerOfColony)) && pawn2.GetComp<CompSuccubusRace>() == null && commonUtl.IsManaConformant(pawn2) && commonUtl.IsPassive(pawn2) && commonUtl.HasEnoughtMana(pawn2, comp.policy.plans[i], comp.tmpJoyMode ? 0.2f : 0f) && ReservationUtility.CanReserveAndReach(pawn, pawn2, 1, 3, 1, -1, null, false);
									bool flag11 = flag10;
									if (flag11)
									{
										comp.tmpTargetType = comp.policy.plans[i].targetType;
										return new Job(NIJobDefOf.Job_NIManaSqueeze, pawn2);
									}
								}
								JobGiver_GetMana.tmpCandidates.Clear();
								JobGiver_GetMana.tmpCandidates = commonUtl.GetPawnsByManaPolicy(pawn, comp.policy.plans[i], comp.tmpJoyMode ? 0.2f : 0f);
								Pawn pawn3;
								if (JobGiver_GetMana.tmpCandidates.Count <= 0)
								{
									pawn3 = null;
								}
								else
								{
									IEnumerable<Pawn> enumerable = JobGiver_GetMana.tmpCandidates;
									Func<Pawn, float> func;
									if ((func = <>9__0) == null)
									{
										func = (<>9__0 = ((Pawn x) => commonUtl.GetPreferWeight(x, pawn)));
									}
									pawn3 = GenCollection.RandomElementByWeight<Pawn>(enumerable, func);
								}
								pawn2 = pawn3;
								bool flag12 = pawn2 != null;
								if (flag12)
								{
									comp.tmpTargetType = comp.policy.plans[i].targetType;
									return new Job(NIJobDefOf.Job_NIManaSqueeze, pawn2);
								}
							}
						}
					}
				}
				result = null;
			}
			return result;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000049C8 File Offset: 0x00002BC8
		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			float priority = this.GetPriority(pawn);
			bool flag = priority <= 0f;
			ThinkResult noJob;
			if (flag)
			{
				noJob = ThinkResult.NoJob;
			}
			else
			{
				int ticksGame = Find.TickManager.TicksGame;
				bool flag2 = priority > 9f && ticksGame > this.nextTryableTick;
				if (flag2)
				{
					this.nextTryableTick = ticksGame + 30;
					Job job = this.TryGiveJob(pawn);
					bool flag3 = job != null;
					if (flag3)
					{
						return new ThinkResult(job, this, new JobTag?(5), false);
					}
				}
				bool flag4 = priority > 6f && ticksGame > this.nextTryableTick;
				if (flag4)
				{
					this.nextTryableTick = ticksGame + 60;
					Job job2 = this.TryGiveJob(pawn);
					bool flag5 = job2 != null;
					if (flag5)
					{
						return new ThinkResult(job2, this, new JobTag?(5), false);
					}
				}
				bool flag6 = priority > 0f && ticksGame > this.nextTryableTick;
				if (flag6)
				{
					this.nextTryableTick = ticksGame + 300;
					Job job3 = this.TryGiveJob(pawn);
					bool flag7 = job3 != null;
					if (flag7)
					{
						return new ThinkResult(job3, this, new JobTag?(5), false);
					}
				}
				noJob = ThinkResult.NoJob;
			}
			return noJob;
		}

		// Token: 0x0400005A RID: 90
		private int nextTryableTick = -1;

		// Token: 0x0400005B RID: 91
		private static List<Pawn> tmpCandidates = new List<Pawn>();
	}
}
