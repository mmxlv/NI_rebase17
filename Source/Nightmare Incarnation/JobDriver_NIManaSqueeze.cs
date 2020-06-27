using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	// Token: 0x0200000C RID: 12
	public class JobDriver_NIManaSqueeze : JobDriver
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00004B18 File Offset: 0x00002D18
		protected Pawn Victim
		{
			get
			{
				return (Pawn)this.job.targetA.Thing;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000041 RID: 65 RVA: 0x00004B40 File Offset: 0x00002D40
		protected CompSuccubusRace CompRace
		{
			get
			{
				return this.pawn.GetComp<CompSuccubusRace>();
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00004B60 File Offset: 0x00002D60
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.JoyMode, "JoyMode", false, false);
			Scribe_References.Look<WorkPlan>(ref this.workPlan, "workPlan", false);
			Scribe_Values.Look<float>(ref this.SqueezeProgress, "SqueezeProgress", 0f, false);
			Scribe_Values.Look<float>(ref this.totalGainMana, "totalGainMana", 0f, false);
			Scribe_Values.Look<float>(ref this.manaGainPerTick, "manaGainPerTick", 0.001f, false);
			Scribe_Values.Look<float>(ref this.joyGainPerTick, "joyGainPerTick", 0.001f, false);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00004BF8 File Offset: 0x00002DF8
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo localTargetInfo = this.Victim;
			Job job = this.job;
			bool flag = !ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				CompSuccubusRace compRace;
				bool flag2 = (compRace = pawn.GetComp<CompSuccubusRace>()) != null;
				if (flag2)
				{
					this.JoyMode = compRace.tmpJoyMode;
					this.workPlan = compRace.policy.plans.Find((WorkPlan x) => x.targetType == compRace.tmpTargetType);
					Pawn_StoryTracker story = pawn.story;
					int? num = (story != null) ? new int?(story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute)) : null;
					bool flag3 = pawn.story != null && (pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) || pawn.story.traits.HasTrait(TraitDefOf.Psychopath) || (num != null && (num == 5 || num == 6))) && commonUtl.HasEnoughtMana(this.Victim, this.workPlan, 0f);
					this.SqueezeAmount = ((this.JoyMode && !flag3) ? 0.2f : this.workPlan.SqueezeValue);
					this.manaGainPerTick = (this.JoyMode ? 0.8f : 1f) * this.SqueezeAmount / (float)this.job.def.joyDuration;
					this.joyGainPerTick = 0.25f * compRace.JoyTolerancesFactor / (float)this.job.def.joyDuration;
					this.joyGainPerTickOffset = 0.25f / (compRace.JoyTolerancesFactor * (float)this.job.def.joyDuration);
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00004E18 File Offset: 0x00003018
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Need_Mana pNeed = this.pawn.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
			Need_Mana tNeed = this.Victim.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
			ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_NIManaSqueeze>(this,TargetIndex.A);
			ToilFailConditions.FailOn<JobDriver_NIManaSqueeze>(this, () => !commonUtl.IsPassive(this.Victim) || this.workPlan.Suspended || (this.JoyMode && !this.workPlan.AllowJoy) || (!this.workPlan.AllowOverSqueeze && commonUtl.GetMana(this.Victim) <= 0f) || (!this.workPlan.AllowDown && (this.Victim.Downed || this.Victim.health.HasHediffsNeedingTendByPlayer(false) || commonUtl.GetNetMana(this.Victim) < -0.79f)) || (!this.workPlan.AllowKill && commonUtl.GetNetMana(this.Victim) < -0.95f));
			ToilFailConditions.FailOnAggroMentalState<JobDriver_NIManaSqueeze>(this, TargetIndex.A);
			base.AddFinishAction(delegate()
			{
				bool flag = this.Victim != this.pawn && this.Victim.CurJobDef == JobDefOf.Wait_MaintainPosture;
				if (flag)
				{
					this.Victim.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				}
				bool flag2 = this.totalGainMana > 0f;
				if (flag2)
				{
					this.ApplyJobThought(this.pawn, this.Victim);
					this.CompRace.TotalManaGained = Mathf.Min(this.CompRace.TotalManaGained + (float)Mathf.RoundToInt(this.totalGainMana / 0.01f) * 0.01f, 9999f);
				}
			});
			PathEndMode interactionCell =PathEndMode.OnCell;
			Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, interactionCell);
			yield return gotoToil;
			yield return Toils_General.Do(delegate()
			{
			});
			Toil work = new Toil();
			work.initAction = delegate()
			{
				Pawn actor = work.actor;
				Pawn pawn = (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
				actor.pather.StopDead();
				PawnUtility.ForceWait(pawn, this.job.def.joyDuration, null, true);
			};
			work.tickAction = delegate()
			{
				this.totalGainMana += this.manaGainPerTick;
				pNeed.CurLevel += this.manaGainPerTick;
				float num = tNeed.CurLevel - this.manaGainPerTick;
				tNeed.CurLevel = ((num < 0f) ? 0f : num);
				bool flag = num < 0f;
				if (flag)
				{
					HealthUtility.AdjustSeverity(this.Victim, NIHediffDefOf.VigorRavage, -num);
				}
				int ticksGame = Find.TickManager.TicksGame;
				bool flag2 = ticksGame > this.moteInterval + this.motethrownTick;
				if (flag2)
				{
					this.motethrownTick = ticksGame;
					MoteMaker.ThrowMetaIcon(this.Victim.Position, this.pawn.Map, ThingDefOf.Mote_Heart);
				}
				bool joyMode = this.JoyMode;
				if (joyMode)
				{
					this.pawn.skills.Learn(SkillDefOf.Social, 0.02f, false);
					this.pawn.needs.joy.GainJoy(this.joyGainPerTick, NIJoyKindDefOf.JoyKind_NISuccubusWork);
					this.pawn.needs.joy.CurLevel += this.joyGainPerTickOffset;
					bool flag3 = this.pawn.needs.joy.CurLevelPercentage >= 1f;
					if (flag3)
					{
						this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
					}
				}
				else
				{
					this.pawn.skills.Learn(SkillDefOf.Social, 0.01f, false);
				}
				bool flag4 = this.totalGainMana > this.SqueezeAmount;
				if (flag4)
				{
					bool flag5 = !this.JoyMode;
					if (flag5)
					{
						this.pawn.needs.joy.CurLevel += 0.1f;
					}
					this.pawn.needs.food.CurLevel += 0.05f;
					this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
				}
			};
			work.defaultCompleteMode =ToilCompleteMode.Never;
			ToilEffects.PlaySustainerOrSound(ToilEffects.WithProgressBar(ToilFailConditions.FailOnCannotTouch<Toil>(work, TargetIndex.A, interactionCell), TargetIndex.A, new Func<float>(this.GetProgress), false, -0.5f), NISoundDefOf.Recipe_Surgery);
			work.activeSkill = (() => SkillDefOf.Social);
			yield return work;
			yield break;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00004E28 File Offset: 0x00003028
		public override string GetReport()
		{
			bool joyMode = this.JoyMode;
			string result;
			if (joyMode)
			{
				result = TranslatorFormattedStringExtensions.Translate("NItxt_Teasing", this.Victim);
			}
			else
			{
				result = base.GetReport();
			}
			return result;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00004E64 File Offset: 0x00003064
		private void ApplyJobThought(Pawn pawn, Pawn target)
		{
			bool flag = !target.Dead;
			if (flag)
			{
				bool flag2 = PawnRelationUtility.GetRelations(pawn, target) == PawnRelationDefOf.Lover || PawnRelationUtility.GetRelations(pawn, target) == PawnRelationDefOf.Fiance || PawnRelationUtility.GetRelations(pawn, target) == PawnRelationDefOf.Spouse;
				if (flag2)
				{
					JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 4);
					JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterOpLover, pawn, target, 0);
				}
				else
				{
					bool flag3 = pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Gay) && target.gender == pawn.gender;
					if (flag3)
					{
						JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 3);
					}
					else
					{
						bool flag4 = pawn.story != null && (pawn.story.traits.HasTrait(TraitDefOf.Kind) || pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 1 || target.story.traits.HasTrait(TraitDefOf.Ascetic));
						if (flag4)
						{
							JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 5);
						}
						else
						{
							bool flag5 = pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.DislikesMen) && target.gender == Gender.Male;
							if (flag5)
							{
								JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 2);
							}
							else
							{
								bool flag6 = pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.DislikesWomen) && target.gender == Gender.Female;
								if (flag6)
								{
									JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 1);
								}
								else
								{
									bool flag7 = pawn.story != null && (pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) || pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 5);
									if (flag7)
									{
										JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 0);
									}
									else
									{
										JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 0);
									}
								}
							}
						}
					}
				}
			}
			else
			{
				bool flag8 = PawnRelationUtility.GetRelations(pawn, target) == PawnRelationDefOf.Lover || PawnRelationUtility.GetRelations(pawn, target) == PawnRelationDefOf.Fiance || PawnRelationUtility.GetRelations(pawn, target) == PawnRelationDefOf.Spouse;
				if (flag8)
				{
					JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillLover, pawn, target, 0);
				}
				else
				{
					bool flag9 = pawn.story != null && (pawn.story.traits.HasTrait(TraitDefOf.Kind) || pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 1);
					if (flag9)
					{
						JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillKind, pawn, target, 0);
					}
					else
					{
						bool flag10 = pawn.story != null && (pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) || pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 5);
						if (flag10)
						{
							JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillEvil, pawn, target, 0);
						}
						else
						{
							bool flag11 = pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Psychopath);
							if (!flag11)
							{
								JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKill, pawn, target, 0);
							}
						}
					}
				}
			}
			bool dead = target.Dead;
			if (dead)
			{
				bool isColonist = target.IsColonist;
				if (isColonist)
				{
					List<Pawn> list = (from x in Find.WorldPawns.AllPawnsAlive
					where x.IsColonist && x != pawn
					select x).ToList<Pawn>();
					//Func<Pawn, bool> <>9__1;
						Func<Pawn, bool> predicate = (Pawn x) => !x.Dead && x.IsColonist && x != pawn;
					foreach (Map map in Find.Maps)
					{
						List<Pawn> list2 = list;
						IEnumerable<Pawn> allPawns = map.mapPawns.AllPawns;
						//Func<Pawn, bool> predicate;
						//if ((predicate = <>9__1) == null)
						//{
						//}
						list2.AddRange(allPawns.Where(predicate));
					}
					foreach (Pawn pawn2 in list)
					{
						JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledColonist, pawn2, pawn, 0);
					}
				}
				else
				{
					bool isPrisoner = target.IsPrisoner;
					if (isPrisoner)
					{
						List<Pawn> list3 = (from x in Find.WorldPawns.AllPawnsAlive
						where (x.IsColonist || x.IsPrisoner) && x != pawn
						select x).ToList<Pawn>();
						//Func<Pawn, bool> <>9__3;
						Func<Pawn, bool> predicate2 = (Pawn x) => !x.Dead && (x.IsColonist || x.IsPrisoner) && x != pawn;
						foreach (Map map2 in Find.Maps)
						{
							List<Pawn> list4 = list3;
							IEnumerable<Pawn> allPawns2 = map2.mapPawns.AllPawns;
							//Func<Pawn, bool> predicate2;
							//if ((predicate2 = <>9__3) == null)
							//{
							//	predicate2 = (<>9__3 = ((Pawn x) => !x.Dead && (x.IsColonist || x.IsPrisoner) && x != pawn));
							//}
							list4.AddRange(allPawns2.Where(predicate2));
						}
						foreach (Pawn pawn3 in list3)
						{
							JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledPrisoner, pawn3, pawn, 0);
						}
					}
				}
				Pawn lover = this.GetLover(target);
				bool flag12 = lover != null && lover != pawn;
				if (flag12)
				{
					JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledLover, lover, pawn, 0);
				}
			}
			else
			{
				bool flag13 = PawnRelationUtility.GetRelations(target, pawn) == PawnRelationDefOf.Lover || PawnRelationUtility.GetRelations(target, pawn) == PawnRelationDefOf.Fiance || PawnRelationUtility.GetRelations(target, pawn) == PawnRelationDefOf.Spouse;
				if (flag13)
				{
					JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodLover, target, pawn, 0);
					JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpLover, target, pawn, 0);
				}
				else
				{
					bool flag14 = target.story != null && target.story.traits.HasTrait(TraitDefOf.Ascetic);
					if (flag14)
					{
						JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodAscetic, target, pawn, 0);
						JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
					}
					else
					{
						bool flag15 = target.story != null && target.story.traits.HasTrait(TraitDefOf.Gay) && pawn.gender == target.gender;
						if (flag15)
						{
							JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodLes, target, pawn, 0);
							JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpDesire, target, pawn, 0);
						}
						else
						{
							bool flag16 = target.story != null && target.story.traits.HasTrait(NITraitDefOf.Masochist);
							if (flag16)
							{
								JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodMaso, target, pawn, 0);
								JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpDesire, target, pawn, 0);
							}
							else
							{
								bool flag17 = target.story != null && target.story.traits.HasTrait(TraitDefOf.DislikesMen) && pawn.gender == Gender.Male;
								if (flag17)
								{
									JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodDislikeMen, target, pawn, 0);
									JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
								}
								else
								{
									bool flag18 = target.story != null && target.story.traits.HasTrait(TraitDefOf.DislikesWomen) && pawn.gender == Gender.Female;
									if (flag18)
									{
										JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodDislikeFem, target, pawn, 0);
										JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
									}
									else
									{
										JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMood, target, pawn, 0);
										JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
									}
								}
							}
						}
					}
				}
				Pawn lover2 = this.GetLover(target);
				bool flag19 = lover2 != null && lover2 != pawn;
				if (flag19)
				{
					bool flag20 = lover2.story != null && lover2.story.traits.HasTrait(NITraitDefOf.Jealous);
					if (flag20)
					{
						JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodJealousy, lover2, pawn, 1);
					}
					else
					{
						JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodJealousy, lover2, pawn, 0);
					}
					JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpJealousy, lover2, pawn, 0);
				}
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x000057C8 File Offset: 0x000039C8
		private static void AddSucJobThought(ThoughtDef thoughtDef, Pawn pawn, Pawn affecter, int forcedStage = 0)
		{
			float statValue = StatExtension.GetStatValue(affecter, StatDefOf.SocialImpact, true);
			Thought_Memory thought_Memory = ThoughtMaker.MakeThought(thoughtDef, forcedStage);
			bool flag = affecter.story != null && affecter.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 1;
			bool flag2 = flag;
			if (flag2)
			{
				thought_Memory.moodPowerFactor = ((thought_Memory.MoodOffset() < 0f) ? 0.5f : 1f);
			}
			Thought_MemorySocial thought_MemorySocial = thought_Memory as Thought_MemorySocial;
			bool flag3 = thought_MemorySocial != null && affecter.GetComp<CompSuccubusRace>() != null;
			if (flag3)
			{
				bool flag4 = thought_MemorySocial.opinionOffset < 0f;
				if (flag4)
				{
					thought_MemorySocial.opinionOffset /= Mathf.Clamp(statValue, 1f, 2f);
					bool flag5 = flag;
					if (flag5)
					{
						thought_MemorySocial.opinionOffset *= 0.5f;
					}
				}
				else
				{
					thought_MemorySocial.opinionOffset *= Mathf.Clamp(statValue, 1f, 2f);
				}
			}
			pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, affecter);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000058DC File Offset: 0x00003ADC
		private Pawn GetLover(Pawn pawn)
		{
			return LovePartnerRelationUtility.ExistingMostLikedLovePartner(pawn, false);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000058F8 File Offset: 0x00003AF8
		private float GetProgress()
		{
			return this.totalGainMana / this.SqueezeAmount;
		}

		// Token: 0x0400005C RID: 92
		private bool JoyMode;

		// Token: 0x0400005D RID: 93
		private WorkPlan workPlan;

		// Token: 0x0400005E RID: 94
		private float SqueezeAmount;

		// Token: 0x0400005F RID: 95
		private float SqueezeProgress;

		// Token: 0x04000060 RID: 96
		private int moteInterval = 90;

		// Token: 0x04000061 RID: 97
		private int motethrownTick = -1;

		// Token: 0x04000062 RID: 98
		private float totalGainMana;

		// Token: 0x04000063 RID: 99
		private float manaGainPerTick;

		// Token: 0x04000064 RID: 100
		private float joyGainPerTick;

		// Token: 0x04000065 RID: 101
		private float joyGainPerTickOffset;
	}
}
