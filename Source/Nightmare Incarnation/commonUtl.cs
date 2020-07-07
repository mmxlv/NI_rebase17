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
	
	public static class commonUtl
	{
		/// <summary>
		/// Gets a list of pawns that can be sucked by the succubus
		/// </summary>
		/// <param name="pawn">should be a succubus</param>
		/// <param name="workPlan"></param>
		/// <param name="forcedValue"></param>
		/// <returns></returns>
		public static List<Pawn> GetPawnsByManaPolicy(Pawn pawn, WorkPlan workPlan, float forcedValue = 0f)
		{
			RelationType targetType = workPlan.targetType;
			List<Pawn> result;
			switch (targetType)
			{
				case RelationType.Colonist:
					result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll((Pawn colonist) => colonist.Faction != null && colonist.Faction == Faction.OfPlayer && colonist.IsColonist && colonist != pawn && !colonist.IsPrisonerOfColony && !IsSuccubus(colonist) && commonUtl.IsManaConformant(colonist) && commonUtl.IsPassive(colonist) && commonUtl.HasEnoughtMana(colonist, workPlan, forcedValue) && ReservationUtility.CanReserveAndReach(pawn, colonist, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false));
					break;
				case RelationType.Prisoner:
					result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll((Pawn prisoner) => prisoner.IsPrisonerOfColony && prisoner != pawn && !IsSuccubus(prisoner) && commonUtl.IsManaConformant(prisoner) && commonUtl.IsPassive(prisoner) && commonUtl.HasEnoughtMana(prisoner, workPlan, forcedValue) && ReservationUtility.CanReserveAndReach(pawn, prisoner, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false));
					break;
				case RelationType.Hostile:
					result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll((Pawn hostile) => hostile.Faction != null && FactionUtility.HostileTo(hostile.Faction, Faction.OfPlayer) && !hostile.IsPrisonerOfColony && hostile != pawn && hostile.GetComp<CompSuccubusRace>() == null && commonUtl.IsManaConformant(hostile) && commonUtl.IsPassive(hostile) && commonUtl.HasEnoughtMana(pawn, workPlan, forcedValue) && ReservationUtility.CanReserveAndReach(pawn, hostile, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false));
					break;
				default:
					result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll((Pawn other) => other.Faction != Faction.OfPlayer && (other.Faction == null || (other.Faction != null && FactionUtility.HostileTo(other.Faction, Faction.OfPlayer))) && !other.IsPrisonerOfColony && other != pawn && other.GetComp<CompSuccubusRace>() == null && commonUtl.IsManaConformant(other) && commonUtl.IsPassive(other) && commonUtl.HasEnoughtMana(pawn, workPlan, forcedValue) && ReservationUtility.CanReserveAndReach(pawn, other, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false));
					break;
			}
			return result;
		}

		/// <summary>
		/// Checks if a given pawn can have a need for mana
		/// </summary>
		/// <param name="pawn">pawn to check</param>
		/// <returns>true if the pawn can have a need for mana, false otherwise</returns>
		public static bool IsManaConformant(Pawn pawn)
		{
			if (pawn != null && pawn.Spawned)
			{
				return (pawn.needs.AllNeeds.Find((Need needForMaba) => needForMaba.def == NINeedDefOf.NI_Need_Mana) != null);
			}
			return false;
		}

		/// <summary>
		/// Checks if a given pawn is passive
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public static bool IsPassive(Pawn pawn)
		{
			return !pawn.InAggroMentalState && ((pawn.Faction == Faction.OfPlayer && !pawn.Drafted) || pawn.IsPrisonerOfColony || (pawn.Faction != Faction.OfPlayer && pawn.Downed));
		}

		/// <summary>
		/// gets the current mana need level of a Pawn
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public static float GetMana(Pawn pawn)
		{
			Need_Mana needMana = (pawn.needs.AllNeeds.Find((Need needForMana) => needForMana.def == NINeedDefOf.NI_Need_Mana) as Need_Mana);
			if (needMana!=null)
			{
				return needMana.CurLevel;
			}
			return -1f;
		}

		/// <summary>
		/// Gets the current level of the Vifor Ravage hediff
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public static float GetVigorRavageLv(Pawn pawn)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			Hediff hediff = (hediffSet != null) ? hediffSet.GetFirstHediffOfDef(NIHediffDefOf.VigorRavage, false) : null;
			return (hediff != null) ? hediff.Severity : 0f;
		}

		/// <summary>
		/// Gets the current level of mana deficiency
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public static float GetDeficiencyLv(Pawn pawn)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			Hediff hediff = (hediffSet != null) ? hediffSet.GetFirstHediffOfDef(NIHediffDefOf.ManaDeficiency, false) : null;
			return (hediff != null) ? hediff.Severity : 0f;
		}

		/// <summary>
		/// gets the mana level after applying vigor ravage or mana deficiency hediffs
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		// Notes: vigor Ravage is a hediff that only applies to non succcubus, so I assume it gets applied after they have been succed by one?
		public static float GetNetMana(Pawn pawn)
		{
			return commonUtl.GetMana(pawn) - ((pawn.GetComp<CompSuccubusRace>() == null) ? commonUtl.GetVigorRavageLv(pawn) : commonUtl.GetDeficiencyLv(pawn));
		}

		/// <summary>
		/// checks if a pawn can be squeezed for mana
		/// TODO change the method name, it should be isAllowedToBeSqueezed, since this method doesn't only check if the pawn has enough mana but also if it can be killed if it doesn't
		/// </summary>
		/// <param name="pawn"></param>
		/// <param name="workPlan"></param>
		/// <param name="forcedValue"></param>
		/// <returns></returns>
		public static bool HasEnoughtMana(Pawn pawn, WorkPlan workPlan, float forcedValue = 0f)
		{
			float squuezeAmount = (forcedValue > 0f) ? forcedValue : workPlan.SqueezeValue;
			if (workPlan.AllowOverSqueeze)
			{
				if (workPlan.AllowDown)
				{
					return workPlan.AllowKill || commonUtl.GetNetMana(pawn) - squuezeAmount >= -0.95f;
				}
				return commonUtl.GetNetMana(pawn) - squuezeAmount >= -0.79f && !pawn.Downed && !pawn.health.HasHediffsNeedingTendByPlayer(false);
			}
			else
			{
				if (workPlan.AllowDown)
				{
					return commonUtl.GetMana(pawn) >= squuezeAmount;
				}
				return commonUtl.GetMana(pawn) >= squuezeAmount && !pawn.Downed && !pawn.health.HasHediffsNeedingTendByPlayer(false);
			}
		}

		public static bool IsSuccubus(Pawn pawn)
		{
			return pawn.GetComp<CompSuccubusRace>() != null;
		}

		/// <summary>
		/// Gets the weight of how much a succubus wants to succ the target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="succubus"></param>
		/// <returns></returns>
		public static float GetPreferWeight(Pawn target, Pawn succubus)
		{
			bool pawnCanSucc = IsSuccubus(succubus)&& target.story != null && succubus.story != null;
			if (pawnCanSucc)
			{
				if (isInRomanticRelationship(target, succubus))
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
				}else  
				{
					return 1f;
				}
			}	
			return 0f;
		}

		private static bool isInRomanticRelationship(Pawn fistPawn, Pawn otherPawn)
		{
			return PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Lover || PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Fiance || PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Spouse;
		}

		/// <summary>
		/// gets the best mana source in the holder'S inventory, if there is one
		/// 
		/// </summary>
		/// <param name="holder"></param>
		/// <param name="eater"></param>
		/// <returns></returns>
		public static Thing BestManaSourceInInventory(Pawn holder, Pawn eater)
		{
			if (holder.inventory == null)
			{
				return null;
			}
			if (eater == null)
			{
				eater = holder;
			}
			ThingOwner<Thing> innerContainer = holder.inventory.innerContainer;
			for(int itemCount = 0; itemCount < innerContainer.Count; itemCount++)
			{
				Thing inventoryItem = innerContainer[itemCount];
				bool isUsableManaSource = CommonData.ManaSourceDefs.Any((ThingDef manaSource) => manaSource == inventoryItem.def) && inventoryItem.IngestibleNow;
				if (isUsableManaSource)
				{
					bool succubusIsCarryingManasource = holder == eater && IsSuccubus(eater);
					if (succubusIsCarryingManasource)
					{
						WorkPlan workPlan = eater.GetComp<CompSuccubusRace>().policy.plans.Find((WorkPlan mealWorkplan) => mealWorkplan.targetType == RelationType.Meal);
						if (workPlan.MainDishThing != null)
						{
							if (inventoryItem.def == workPlan.MainDishThing)
							{
								if ((float)inventoryItem.stackCount>=0f)
								{
									return inventoryItem;
								}
							}
						}
					}
					else
					{
						if ((float)inventoryItem.stackCount >= 0f)
						{
							return inventoryItem;
						}
					}
				}
			}
			return null;
			
		}

		/// <summary>
		/// finds the best edible manasource on the map
		/// </summary>
		/// <param name="getter"></param>
		/// <param name="eater"></param>
		/// <param name="allowForbidden"></param>
		/// <param name="forceScanWholeMap"></param>
		/// <returns></returns>
		public static Thing BestManaSourceOnMap(Pawn getter, Pawn eater, bool allowForbidden = false, bool forceScanWholeMap = false)
		{
			bool getterIsIncapableOfCarrying = !getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && getter != eater;
			if (getterIsIncapableOfCarrying)
			{
				return null;
			}
			IEnumerable<Thing> manaSourceItems = from items in getter.Map.listerThings.AllThings
			where CommonData.ManaSourceDefs.Any((ThingDef manaSourceDef) => items.def == manaSourceDef)
			select items;
			//??? is enumerable returning false when empty? otherwise it doesn't work
			//	bool flag2 = !manaSourceItems.EnumerableNullOrEmpty();
			//	if (flag2)
			//	{
			//		result = null;
			//	}
			if (manaSourceItems.EnumerableNullOrEmpty())
			{
				return null;
			}
			ThingDef restric = null;
			if (getter == eater && IsSuccubus(eater))
			{
				WorkPlan workPlan = eater.GetComp<CompSuccubusRace>().policy.plans.Find((WorkPlan mealWorkPlan) => mealWorkPlan.targetType == RelationType.Meal);
				bool suspended = workPlan.Suspended;
				if (suspended)
				{
					return null;
				}
				if (workPlan.MainDishThing != null)
				{
					restric = workPlan.MainDishThing;
				}
			}
			ThingRequest thingRequest = ThingRequest.ForUndefined();
			Predicate<Thing> allowedManaSourceFinder = (Thing item) => item.Spawned && CommonData.ManaSourceDefs.Any((ThingDef manaSourceDef) => item.def == manaSourceDef) && (allowForbidden || !ForbidUtility.IsForbidden(item, Faction.OfPlayer)) && (restric == null || item.def == restric) && ReservationUtility.CanReserve(getter, item, 1, -1, null, false);
			Thing thing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.Touch, TraverseParms.For(getter, Danger.Deadly, 0, false), 9999f, allowedManaSourceFinder, manaSourceItems, 0, -1, false, RegionType.Set_Passable, false);
			return thing;
		}

		/// <summary>
		/// the original Method extracted by the decompiler
		/// This method stays for debugging purposes until I fully understand it
		/// </summary>
		/// <param name="getter"></param>
		/// <param name="eater"></param>
		/// <param name="manaSource"></param>
		/// <param name="manaSourceDef"></param>
		/// <param name="canUseInventory"></param>
		/// <param name="allowForbidden"></param>
		/// <param name="forceScanWholeMap"></param>
		/// <returns></returns>
		public static bool TryFindBestManaSourceForORIGINALMETHOD(Pawn getter, Pawn eater, out Thing manaSource, out ThingDef manaSourceDef, bool canUseInventory = true, bool allowForbidden = false, bool forceScanWholeMap = false)
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="getter"></param>
		/// <param name="eater"></param>
		/// <param name="manaSource"></param>
		/// <param name="manaSourceDef"></param>
		/// <param name="canUseInventory"></param>
		/// <param name="allowForbidden"></param>
		/// <param name="forceScanWholeMap"></param>
		/// <returns></returns>
		public static bool TryFindBestManaSourceFor(Pawn getter, Pawn eater, out Thing manaSource, out ThingDef manaSourceDef, bool canUseInventory = true, bool allowForbidden = false, bool forceScanWholeMap = false)
		{
			bool getterCanCarry = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			// maybe planned functionality?
			// bool eaterIsNotTeetotaler= !PawnUtility.IsTeetotaler(eater);
			Thing inventoryManaSource = null;
			if (canUseInventory)
			{
				if (getterCanCarry)
				{
					inventoryManaSource = commonUtl.BestManaSourceInInventory(getter, eater);
				}
				if (inventoryManaSource != null)
				{
					if (getter.Faction != Faction.OfPlayer)
					{
						manaSource = inventoryManaSource;
						manaSourceDef = manaSource.def;
						return true;
					}
					CompRottable compRottable = ThingCompUtility.TryGetComp<CompRottable>(inventoryManaSource);
					if (compRottable != null && compRottable.TicksUntilRotAtCurrentTemp < 30000)
					{
						manaSource = inventoryManaSource;
						manaSourceDef = manaSource.def;
						return true;
					}
				}
			}
			Thing mapManaSource = commonUtl.BestManaSourceOnMap(getter, eater, allowForbidden, forceScanWholeMap);
			if (inventoryManaSource!=null || mapManaSource != null)
			{
				if (mapManaSource != null)
				{
					manaSource = mapManaSource;
					manaSourceDef = manaSource.def;
					return true;
				}
				//This code vvv is inreachable, in order to reach it inventoryManaSource cannot be null, but it is always null at this point
				//I need to explore what this method is supposed to do and recreate it accordingly
			//	else
			//	{
			//		ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(inventoryManaSource, false);
			//		bool flag9 = mapManaSource == null;
			//		if (inventoryManaSource!=null)
			//		{
			//			float num = FoodUtility.FoodOptimality(eater, mapManaSource, mapManaSource.def, (float)(getter.Position - mapManaSource.Position).LengthManhattan, false);
			//			float num2 = FoodUtility.FoodOptimality(eater, inventoryManaSource, finalIngestibleDef, 0f, false);
			//			num2 -= 32f;
			//			bool flag10 = num > num2;
			//			if (flag10)
			//			{
			//				manaSource = mapManaSource;
			//				manaSourceDef = manaSource.def;
			//				result = true;
			//			}
			//			else
			//			{
			//				manaSource = inventoryManaSource;
			//				manaSourceDef = FoodUtility.GetFinalIngestibleDef(manaSource, false);
			//				result = true;
			//			}
			//		}
			//		else
			//		{
			//			manaSource = inventoryManaSource;
			//			manaSourceDef = finalIngestibleDef;
			//			result = true;
			//		}
			//	}
			//	return result;
			}
			manaSource = null;
			manaSourceDef = null;
			return false;
		}

		/// <summary>
		/// Returns how many of a stackable manaSource should be ingested to fill the mana need
		/// </summary>
		/// <param name="pawn"></param>
		/// <param name="manaSource"></param>
		/// <returns></returns>
		public static int EstimateIngestCountOf(Pawn pawn, Thing manaSource)
		{
			if (!IsSuccubus(pawn))
			{
				return 1;
			}			
			IngestionOutcomeDoer_OffsetNeed ingestionOutcomeDoer_OffsetNeed = manaSource.def.ingestible.outcomeDoers.FirstOrDefault(delegate(IngestionOutcomeDoer manaNeedIngestionOffset)
			{
				IngestionOutcomeDoer_OffsetNeed ingestionOutcomeDoer_OffsetNeed2;
				return (ingestionOutcomeDoer_OffsetNeed2 = (manaNeedIngestionOffset as IngestionOutcomeDoer_OffsetNeed)) != null && ingestionOutcomeDoer_OffsetNeed2.need == NINeedDefOf.NI_Need_Mana;
			}) as IngestionOutcomeDoer_OffsetNeed;
			Need_Mana need_Mana = pawn.needs.AllNeeds.Find((Need needForMana) => needForMana.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
			float currentNeededMana = need_Mana.MaxLevel - need_Mana.CurLevel;
			float manaGainedSoFar = 0f;
			int ingestedItemCount = 0;
			for (int i = 0; i < manaSource.def.ingestible.maxNumToIngestAtOnce; i++)
			{
				manaGainedSoFar += ingestionOutcomeDoer_OffsetNeed.offset;
				bool flag2 = manaGainedSoFar >= currentNeededMana;
				if (flag2)
				{
					break;
				}
				ingestedItemCount++;
			}
			return Math.Min(Math.Max(1, ingestedItemCount), manaSource.stackCount);
		}
	}
}
