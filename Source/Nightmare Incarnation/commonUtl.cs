using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	// Token: 0x02000007 RID: 7
	public static class commonUtl
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002AE0 File Offset: 0x00000CE0
		public static List<Pawn> GetPawnsByManaPolicy(Pawn pawn, WorkPlan workPlan, float forcedValue = 0f)
		{
			RelationType targetType = workPlan.targetType;
			List<Pawn> result;
			if (targetType != RelationType.Colonist)
			{
				if (targetType != RelationType.Prisoner)
				{
					if (targetType != RelationType.Hostile)
					{
						result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll((Pawn x) => x.Faction != Faction.OfPlayer && (x.Faction == null || (x.Faction != null && FactionUtility.HostileTo(x.Faction, Faction.OfPlayer))) && !x.IsPrisonerOfColony && x != pawn && x.GetComp<CompSuccubusRace>() == null && commonUtl.IsManaConformant(x) && commonUtl.IsPassive(x) && commonUtl.HasEnoughtMana(pawn, workPlan, forcedValue) && ReservationUtility.CanReserveAndReach(pawn, x, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false));
					}
					else
					{
						result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll((Pawn x) => x.Faction != null && FactionUtility.HostileTo(x.Faction, Faction.OfPlayer) && !x.IsPrisonerOfColony && x != pawn && x.GetComp<CompSuccubusRace>() == null && commonUtl.IsManaConformant(x) && commonUtl.IsPassive(x) && commonUtl.HasEnoughtMana(pawn, workPlan, forcedValue) && ReservationUtility.CanReserveAndReach(pawn, x, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false));
					}
				}
				else
				{
					result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll((Pawn x) => x.IsPrisonerOfColony && x != pawn && x.GetComp<CompSuccubusRace>() == null && commonUtl.IsManaConformant(x) && commonUtl.IsPassive(x) && commonUtl.HasEnoughtMana(x, workPlan, forcedValue) && ReservationUtility.CanReserveAndReach(pawn, x, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false));
				}
			}
			else
			{
				result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll((Pawn x) => x.Faction != null && x.Faction == Faction.OfPlayer && x.IsColonist && x != pawn && !x.IsPrisonerOfColony && x.GetComp<CompSuccubusRace>() == null && commonUtl.IsManaConformant(x) && commonUtl.IsPassive(x) && commonUtl.HasEnoughtMana(x, workPlan, forcedValue) && ReservationUtility.CanReserveAndReach(pawn, x, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false));
			}
			return result;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002BCC File Offset: 0x00000DCC
		public static bool IsManaConformant(Pawn pawn)
		{
			bool result;
			if (pawn != null && pawn.Spawned)
			{
				result = (pawn.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) != null);
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002C20 File Offset: 0x00000E20
		public static bool IsPassive(Pawn pawn)
		{
			return !pawn.InAggroMentalState && ((pawn.Faction == Faction.OfPlayer && !pawn.Drafted) || pawn.IsPrisonerOfColony || (pawn.Faction != Faction.OfPlayer && pawn.Downed));
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002C74 File Offset: 0x00000E74
		public static float GetMana(Pawn pawn)
		{
			Need_Mana need_Mana;
			bool flag = (need_Mana = (pawn.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana)) != null;
			float result;
			if (flag)
			{
				result = need_Mana.CurLevel;
			}
			else
			{
				result = -1f;
			}
			return result;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002CD4 File Offset: 0x00000ED4
		public static float GetVigorRavageLv(Pawn pawn)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			Hediff hediff = (hediffSet != null) ? hediffSet.GetFirstHediffOfDef(NIHediffDefOf.VigorRavage, false) : null;
			return (hediff != null) ? hediff.Severity : 0f;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002D14 File Offset: 0x00000F14
		public static float GetDeficiencyLv(Pawn pawn)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			Hediff hediff = (hediffSet != null) ? hediffSet.GetFirstHediffOfDef(NIHediffDefOf.ManaDeficiency, false) : null;
			return (hediff != null) ? hediff.Severity : 0f;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002D54 File Offset: 0x00000F54
		public static float GetNetMana(Pawn pawn)
		{
			return commonUtl.GetMana(pawn) - ((pawn.GetComp<CompSuccubusRace>() == null) ? commonUtl.GetVigorRavageLv(pawn) : commonUtl.GetDeficiencyLv(pawn));
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002D84 File Offset: 0x00000F84
		public static bool HasEnoughtMana(Pawn pawn, WorkPlan workPlan, float forcedValue = 0f)
		{
			float num = (forcedValue > 0f) ? forcedValue : workPlan.SqueezeValue;
			bool allowOverSqueeze = workPlan.AllowOverSqueeze;
			bool result;
			if (allowOverSqueeze)
			{
				bool allowDown = workPlan.AllowDown;
				if (allowDown)
				{
					bool allowKill = workPlan.AllowKill;
					result = (allowKill || commonUtl.GetNetMana(pawn) - num >= -0.95f);
				}
				else
				{
					result = (commonUtl.GetNetMana(pawn) - num >= -0.79f && !pawn.Downed && !pawn.health.HasHediffsNeedingTendByPlayer(false));
				}
			}
			else
			{
				bool allowDown2 = workPlan.AllowDown;
				if (allowDown2)
				{
					result = (commonUtl.GetMana(pawn) >= num);
				}
				else
				{
					result = (commonUtl.GetMana(pawn) >= num && !pawn.Downed && !pawn.health.HasHediffsNeedingTendByPlayer(false));
				}
			}
			return result;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002E58 File Offset: 0x00001058
		public static bool IsSuccubus(Pawn pawn)
		{
			return pawn.GetComp<CompSuccubusRace>() != null;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002E74 File Offset: 0x00001074
		public static float GetPreferWeight(Pawn target, Pawn pawn)
		{
			CompSuccubusRace comp = pawn.GetComp<CompSuccubusRace>();
			bool flag = comp == null || target.story == null || pawn.story == null;
			float result;
			if (flag)
			{
				result = 0f;
			}
			else
			{
				bool flag2 = PawnRelationUtility.GetRelations(target, pawn) == PawnRelationDefOf.Lover || PawnRelationUtility.GetRelations(target, pawn) == PawnRelationDefOf.Fiance || PawnRelationUtility.GetRelations(target, pawn) == PawnRelationDefOf.Spouse;
				if (flag2)
				{
					result = 3f;
				}
				else
				{
					bool flag3 = pawn.story.traits.HasTrait(TraitDefOf.DislikesMen) && target.gender == Gender.Male;
					if (flag3)
					{
						result = 0.01f;
					}
					else
					{
						bool flag4 = pawn.story.traits.HasTrait(TraitDefOf.DislikesWomen) && target.gender == Gender.Female;
						if (flag4)
						{
							result = 0.01f;
						}
						else
						{
							bool flag5 = pawn.story.traits.HasTrait(TraitDefOf.Gay);
							if (flag5)
							{
								result = ((target.gender == pawn.gender) ? 2f : 0.1f);
							}
							else
							{
								result = 1f;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002F94 File Offset: 0x00001194
		public static Thing BestManaSourceInInventory(Pawn holder, Pawn eater)
		{
			bool flag = holder.inventory == null;
			Thing result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = eater == null;
				if (flag2)
				{
					eater = holder;
				}
				ThingOwner<Thing> innerContainer = holder.inventory.innerContainer;
				int i = 0;
				while (i < innerContainer.Count)
				{
					Thing thing = innerContainer[i];
					bool flag3 = CommonData.ManaSourceDefs.Any((ThingDef x) => x == thing.def) && thing.IngestibleNow;
					if (flag3)
					{
						bool flag4 = holder == eater && commonUtl.IsSuccubus(eater);
						if (flag4)
						{
							WorkPlan workPlan = eater.GetComp<CompSuccubusRace>().policy.plans.Find((WorkPlan x) => x.targetType == RelationType.Meal);
							bool flag5 = workPlan.MainDishThing != null;
							if (flag5)
							{
								bool flag6 = thing.def == workPlan.MainDishThing;
								if (!flag6)
								{
									goto IL_15C;
								}
								float num = (float)thing.stackCount;
								bool flag7 = num >= 0f;
								if (flag7)
								{
									return thing;
								}
							}
						}
						float num2 = (float)thing.stackCount;
						bool flag8 = num2 >= 0f;
						if (flag8)
						{
							return thing;
						}
					}
					IL_15C:
					i++;
					continue;
					goto IL_15C;
				}
				result = null;
			}
			return result;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000311C File Offset: 0x0000131C
		public static Thing BestManaSourceOnMap(Pawn getter, Pawn eater, bool allowForbidden = false, bool forceScanWholeMap = false)
		{
			bool flag = !getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && getter != eater;
			Thing result;
			if (flag)
			{
				result = null;
			}
			else
			{
				IEnumerable<Thing> enumerable = from x in getter.Map.listerThings.AllThings
				where CommonData.ManaSourceDefs.Any((ThingDef y) => x.def == y)
				select x;
				bool flag2 = !enumerable.EnumerableNullOrEmpty();
				if (flag2)
				{
					result = null;
				}
				else
				{
					ThingDef restrict = null;
					bool flag3 = getter == eater && commonUtl.IsSuccubus(eater);
					if (flag3)
					{
						WorkPlan workPlan = eater.GetComp<CompSuccubusRace>().policy.plans.Find((WorkPlan x) => x.targetType == RelationType.Meal);
						bool suspended = workPlan.Suspended;
						if (suspended)
						{
							return null;
						}
						bool flag4 = workPlan.MainDishThing != null;
						if (flag4)
						{
							restrict = workPlan.MainDishThing;
						}
					}
					ThingRequest thingRequest = ThingRequest.ForUndefined();
					Predicate<Thing> predicate = (Thing t) => t.Spawned && CommonData.ManaSourceDefs.Any((ThingDef x) => t.def == x) && (allowForbidden || !ForbidUtility.IsForbidden(t, Faction.OfPlayer)) && (restrict == null || t.def == restrict) && ReservationUtility.CanReserve(getter, t, 1, -1, null, false);
					Thing thing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.Touch, TraverseParms.For(getter, Danger.Deadly, 0, false), 9999f, predicate, enumerable, 0, -1, false, RegionType.Set_Passable, false);
					result = thing;
				}
			}
			return result;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x000032A8 File Offset: 0x000014A8
		public static bool TryFindBestManaSourceFor(Pawn getter, Pawn eater, out Thing manaSource, out ThingDef manaSourceDef, bool canUseInventory = true, bool allowForbidden = false, bool forceScanWholeMap = false)
		{
			bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			bool flag2 = !PawnUtility.IsTeetotaler(eater);
			Thing thing = null;
			if (canUseInventory)
			{
				bool flag3 = flag;
				if (flag3)
				{
					thing = commonUtl.BestManaSourceInInventory(getter, eater);
				}
				bool flag4 = thing != null;
				if (flag4)
				{
					bool flag5 = getter.Faction != Faction.OfPlayer;
					if (flag5)
					{
						manaSource = thing;
						manaSourceDef = manaSource.def;
						return true;
					}
					CompRottable compRottable = ThingCompUtility.TryGetComp<CompRottable>(thing);
					bool flag6 = compRottable != null && compRottable.Stage == null && compRottable.TicksUntilRotAtCurrentTemp < 30000;
					if (flag6)
					{
						manaSource = thing;
						manaSourceDef = manaSource.def;
						return true;
					}
				}
			}
			Thing thing2 = commonUtl.BestManaSourceOnMap(getter, eater, allowForbidden, forceScanWholeMap);
			bool flag7 = thing == null && thing2 == null;
			bool result;
			if (flag7)
			{
				manaSource = null;
				manaSourceDef = null;
				result = false;
			}
			else
			{
				bool flag8 = thing == null && thing2 != null;
				if (flag8)
				{
					manaSource = thing2;
					manaSourceDef = manaSource.def;
					result = true;
				}
				else
				{
					ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(thing, false);
					bool flag9 = thing2 == null;
					if (flag9)
					{
						manaSource = thing;
						manaSourceDef = finalIngestibleDef;
						result = true;
					}
					else
					{
						float num = FoodUtility.FoodOptimality(eater, thing2, thing2.def, (float)(getter.Position - thing2.Position).LengthManhattan, false);
						float num2 = FoodUtility.FoodOptimality(eater, thing, finalIngestibleDef, 0f, false);
						num2 -= 32f;
						bool flag10 = num > num2;
						if (flag10)
						{
							manaSource = thing2;
							manaSourceDef = manaSource.def;
							result = true;
						}
						else
						{
							manaSource = thing;
							manaSourceDef = FoodUtility.GetFinalIngestibleDef(manaSource, false);
							result = true;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003468 File Offset: 0x00001668
		public static int EstimateIngestCountOf(Pawn pawn, Thing manaSource)
		{
			bool flag = !commonUtl.IsSuccubus(pawn);
			int result;
			if (flag)
			{
				result = 1;
			}
			else
			{
				IngestionOutcomeDoer_OffsetNeed ingestionOutcomeDoer_OffsetNeed = manaSource.def.ingestible.outcomeDoers.FirstOrDefault(delegate(IngestionOutcomeDoer x)
				{
					IngestionOutcomeDoer_OffsetNeed ingestionOutcomeDoer_OffsetNeed2;
					return (ingestionOutcomeDoer_OffsetNeed2 = (x as IngestionOutcomeDoer_OffsetNeed)) != null && ingestionOutcomeDoer_OffsetNeed2.need == NINeedDefOf.NI_Need_Mana;
				}) as IngestionOutcomeDoer_OffsetNeed;
				Need_Mana need_Mana = pawn.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
				float num = need_Mana.MaxLevel - need_Mana.CurLevel;
				float num2 = 0f;
				int num3 = 0;
				for (int i = 0; i < manaSource.def.ingestible.maxNumToIngestAtOnce; i++)
				{
					num2 += ingestionOutcomeDoer_OffsetNeed.offset;
					bool flag2 = num2 >= num;
					if (flag2)
					{
						break;
					}
					num3++;
				}
				result = Math.Min(Math.Max(1, num3), manaSource.stackCount);
			}
			return result;
		}
	}
}
