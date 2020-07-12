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
        [Obsolete]
        public static List<Pawn> GetPawnsByManaPolicy(Pawn pawn, WorkPlan workPlan, float forcedValue = 0f)
        {
            RelationType targetType = workPlan.targetType;
            List<Pawn> result;
            switch (targetType)
            {
                case RelationType.Colonist:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll(colonist => colonist.IsColonist
                                                                                   && colonist != pawn
                                                                                   && !colonist.IsPrisonerOfColony
                                                                                   && !IsSuccubus(colonist)
                                                                                   && IsManaConformant(colonist)
                                                                                   && IsPassive(colonist)
                                                                                   && HasEnoughtMana(colonist, workPlan, forcedValue)
                                                                                   && ReservationUtility.CanReserveAndReach(pawn, colonist, PathEndMode.OnCell, Danger.Deadly));
                    break;
                case RelationType.Prisoner:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll(prisoner => prisoner.IsPrisonerOfColony
                                                                                   && prisoner != pawn
                                                                                   && !IsSuccubus(prisoner)
                                                                                   && IsManaConformant(prisoner)
                                                                                   && IsPassive(prisoner)
                                                                                   && HasEnoughtMana(prisoner, workPlan, forcedValue)
                                                                                   && ReservationUtility.CanReserveAndReach(pawn, prisoner, PathEndMode.OnCell, Danger.Deadly));
                    break;
                case RelationType.Hostile:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll(hostile => hostile.HostileTo(Faction.OfPlayer)
                                                                                  && !hostile.IsPrisonerOfColony
                                                                                  && hostile != pawn
                                                                                  && !IsSuccubus(hostile)
                                                                                  && IsManaConformant(hostile)
                                                                                  && IsPassive(hostile)
                                                                                  && HasEnoughtMana(pawn, workPlan, forcedValue)
                                                                                  && ReservationUtility.CanReserveAndReach(pawn, hostile, PathEndMode.OnCell, Danger.Deadly));
                    break;
                default:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.FindAll(other => other.Faction != Faction.OfPlayer
                                                                                && (other.Faction == null
                                                                                    || (other.Faction != null && FactionUtility.HostileTo(other.Faction, Faction.OfPlayer)))
                                                                                && !other.IsPrisonerOfColony
                                                                                && other != pawn
                                                                                && !IsSuccubus(other)
                                                                                && IsManaConformant(other)
                                                                                && IsPassive(other)
                                                                                && HasEnoughtMana(pawn, workPlan, forcedValue)
                                                                                && ReservationUtility.CanReserveAndReach(pawn, other, PathEndMode.OnCell, Danger.Deadly));
                    break;
            }
            return result;
        }

        /// <summary>
        /// Checks if a given pawn can have a need for mana
        /// </summary>
        /// <param name="pawn">pawn to check</param>
        /// <returns>true if the pawn can have a need for mana, false otherwise</returns>
        [Obsolete]
        public static bool IsManaConformant(Pawn pawn)
        {
            if (pawn != null && pawn.Spawned)
            {
                return pawn.needs.AllNeeds.Any(needForMana => needForMana.def == NINeedDefOf.NI_Need_Mana);
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
            return !pawn.InAggroMentalState
                   && ((pawn.Faction == Faction.OfPlayer && !pawn.Drafted)
                       || pawn.IsPrisonerOfColony
                       || (pawn.Faction != Faction.OfPlayer && pawn.Downed));
        }

        /// <summary>
        /// gets the current mana need level of a Pawn
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        [Obsolete]
        public static float GetMana(Pawn pawn)
        {
            Need_Mana needMana = (pawn.needs.AllNeeds.Find((Need needForMana) => needForMana.def == NINeedDefOf.NI_Need_Mana) as Need_Mana);
            if (needMana != null)
            {
                return needMana.CurLevel;
            }
            return -1f;
        }

        /// <summary>
        /// Gets the current level of the Vifor Ravage hediff for non-succubi
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static float GetVigorRavageLv(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(NIHediffDefOf.VigorRavage) is Hediff hediff)
                return hediff.Severity;
            else
                return 0f;
        }

        /// <summary>
        /// Gets the current level of mana deficiency for succubi
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static float GetDeficiencyLv(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(NIHediffDefOf.ManaDeficiency) is Hediff hediff)
                return hediff.Severity;
            else
                return 0f;
        }

        /// <summary>
        /// gets the mana level after applying vigor ravage or mana deficiency hediffs
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        [Obsolete]
        public static float GetNetMana(Pawn pawn)
        {
            return GetMana(pawn) - ((pawn.GetComp<CompSuccubusRace>() == null) ? GetVigorRavageLv(pawn) : GetDeficiencyLv(pawn));
        }

        /// <summary>
        /// checks if a pawn can be squeezed for mana
        /// TODO change the method name, it should be isAllowedToBeSqueezed, since this method doesn't only check if the pawn has enough mana but also if it can be killed if it doesn't
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="workPlan"></param>
        /// <param name="forcedValue"></param>
        /// <returns></returns>
        [Obsolete]
        public static bool HasEnoughtMana(Pawn pawn, WorkPlan workPlan, float forcedValue = 0f)
        {
            float squuezeAmount = (forcedValue > 0f) ? forcedValue : workPlan.SqueezeValue;
            if (workPlan.AllowOverSqueeze)
            {
                if (workPlan.AllowDown)
                {
                    return workPlan.AllowKill || GetNetMana(pawn) - squuezeAmount >= -0.95f;
                }
                else
                    return GetNetMana(pawn) - squuezeAmount >= -0.79f && !pawn.Downed && !pawn.health.HasHediffsNeedingTendByPlayer();
            }
            else
            {
                if (workPlan.AllowDown)
                {
                    return GetMana(pawn) >= squuezeAmount;
                }
                else
                    return GetMana(pawn) >= squuezeAmount && !pawn.Downed && !pawn.health.HasHediffsNeedingTendByPlayer();
            }
        }

        [Obsolete]
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
            if (!IsSuccubus(succubus)
                || target.story == null
                || succubus.story == null)
            {
                return 0f;
            }
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
            }
            else
            {
                return 1f;
            }
        }

        private static bool isInRomanticRelationship(Pawn fistPawn, Pawn otherPawn)
        {
            return PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Lover || PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Fiance || PawnRelationUtility.GetRelations(fistPawn, otherPawn) == PawnRelationDefOf.Spouse;
        }

        /// <summary>
        /// gets the best mana source in the holder'S inventory, if there is one
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
            for (int itemCount = 0; itemCount < innerContainer.Count; itemCount++)
            {
                Thing inventoryItem = innerContainer[itemCount];
                if (CommonData.ManaSourceDefs.Any((ThingDef manaSource) => manaSource == inventoryItem.def) && inventoryItem.IngestibleNow)
                {
                    if (holder == eater && IsSuccubus(eater))
                    {
                        WorkPlan workPlan = eater.GetComp<CompSuccubusRace>().policy.plans.Find((WorkPlan mealWorkplan) => mealWorkplan.targetType == RelationType.Meal);
                        if (workPlan.MainDishThing != null)
                        {
                            if (inventoryItem.def == workPlan.MainDishThing)
                            {
                                if (inventoryItem.stackCount >= 0f)
                                {
                                    return inventoryItem;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (inventoryItem.stackCount >= 0f)
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
            if (!getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && getter != eater)
            {
                return null;
            }

            IEnumerable<Thing> manaSourceItems = getter.Map.listerThings.AllThings.Where(x => CommonData.ManaSourceDefs.Any((ThingDef manaSourceDef) => x.def == manaSourceDef));
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
            bool allowedManaSourceFinder(Thing item) => item.Spawned
                                                        && CommonData.ManaSourceDefs.Any((ThingDef manaSourceDef) => item.def == manaSourceDef)
                                                        && (allowForbidden || !ForbidUtility.IsForbidden(item, Faction.OfPlayer))
                                                        && (restric == null || item.def == restric)
                                                        && ReservationUtility.CanReserve(getter, item, 1, -1, null, false);

            Thing thing = GenClosest.ClosestThingReachable(root: getter.Position,
                                                           map: getter.Map,
                                                           thingReq: thingRequest,
                                                           peMode: PathEndMode.Touch,
                                                           traverseParams: TraverseParms.For(getter, Danger.Deadly, 0, false),
                                                           maxDistance: 9999f,
                                                           validator: allowedManaSourceFinder,
                                                           customGlobalSearchSet: manaSourceItems,
                                                           searchRegionsMin: 0,
                                                           searchRegionsMax: -1,
                                                           forceAllowGlobalSearch: false,
                                                           traversableRegionTypes: RegionType.Set_Passable,
                                                           ignoreEntirelyForbiddenRegions: false);
            return thing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getter">Pawn who will deliver mana.</param>
        /// <param name="eater">Pawn who will eat.</param>
        /// <param name="manaSource"></param>
        /// <param name="manaSourceDef"></param>
        /// <param name="canUseInventory"></param>
        /// <param name="allowForbidden"></param>
        /// <param name="forceScanWholeMap"></param>
        /// <returns></returns>
        public static bool TryFindBestManaSourceFor(Pawn getter, Pawn eater, out Thing manaSource, out ThingDef manaSourceDef, bool canUseInventory = true, bool allowForbidden = false, bool forceScanWholeMap = false)
        {
            Thing inventoryManaSource = null;
            if (canUseInventory)
            {
                if (getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    inventoryManaSource = BestManaSourceInInventory(getter, eater);
                }
                if (inventoryManaSource != null)
                {
                    manaSource = inventoryManaSource;
                    manaSourceDef = manaSource.def;
                    return true;
                }
            }
            Thing mapManaSource = BestManaSourceOnMap(getter, eater, allowForbidden, forceScanWholeMap);
            if (mapManaSource != null)
            {
                manaSource = mapManaSource;
                manaSourceDef = manaSource.def;
                return true;
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
        [Obsolete]
        public static int EstimateIngestCountOf(Pawn pawn, Thing manaSource)
        {
            if (!IsSuccubus(pawn))
            {
                return 1;
            }
            IngestionOutcomeDoer_OffsetNeed ingestionOutcomeDoer_OffsetNeed = manaSource.def.ingestible.outcomeDoers.FirstOrDefault(delegate (IngestionOutcomeDoer manaNeedIngestionOffset)
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
