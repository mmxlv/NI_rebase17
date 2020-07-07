using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			Harmony harmony = new Harmony("NightmareIncarnation.HarmonyPatch");
			harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), "AddHediff", new Type[]
			{
				typeof(Hediff),
				typeof(BodyPartRecord),
				typeof(DamageInfo),
				typeof(DamageWorker.DamageResult)
			}, null), new HarmonyMethod(typeof(HarmonyPatches), "Pawn_HealthTrackerAddHediffPrefix", null), null, null);
			harmony.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos", null, null), null, new HarmonyMethod(typeof(HarmonyPatches), "Pawn_GetGizmos_Postfix", null), null);
			harmony.Patch(AccessTools.Method(typeof(Pawn), "PreApplyDamage", null, null), new HarmonyMethod(typeof(HarmonyPatches), "Pawn_PreApplyDamage_Prefix", null), null, null);
			harmony.Patch(AccessTools.Method(typeof(Thing), "Ingested", null, null), new HarmonyMethod(typeof(HarmonyPatches), "Thing_Ingested_Prefix", null), null, null);
		}

		/// <summary>
		/// Checks if the given pawn is resistant to the given hediff
		/// </summary>
		/// <param name="___pawn"></param>
		/// <param name="hediff"></param>
		/// <param name="part"></param>
		/// <param name="dinfo"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		private static bool Pawn_HealthTrackerAddHediffPrefix(Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			if (HarmonyPatches.HasResistFor(___pawn, hediff, part, dinfo, result))
			{
				Log.Message("The pawn "+___pawn.Label + "is resistant to" + hediff.Label, false);
				return false;
			}
			return true;
		}

		
		private static bool HasResistFor(Pawn pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			if (commonUtl.IsSuccubus(pawn))
			{
				return HarmonyPatches.FleshHediffInvalidFor(pawn, hediff);
			}
			return false;
		}

		private static bool FleshHediffInvalidFor(Pawn pawn, Hediff hediff)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				//I think this should be true cause when it steps in here the pawn is not fleshy, right?
				return false;
			}
			else
			{
				if (hediff.def == HediffDefOf.Anesthetic)
				{
					return pawn.Faction == null || !pawn.Faction.IsPlayer || pawn.CurJob == null || pawn.CurJobDef != JobDefOf.LayDown || (pawn.jobs.posture != PawnPosture.LayingInBed && pawn.jobs.posture != PawnPosture.LayingOnGroundNormal);
				}
				if (hediff.def == HediffDefOf.BloodLoss)
				{
					HediffSet hediffSet = pawn.health.hediffSet;
					return hediffSet != null && hediffSet.BleedRateTotal < 0.1f;
				}
				if (hediff.def == HediffDefOf.Malnutrition)
				{
					Pawn_NeedsTracker needs = pawn.needs;
					if (needs == null)
					{
						return false;
					}
					Need_Food food = needs.food;
					return ((food != null) ? new float?(food.CurLevel) : null) > 0f;
				}
			}
			return false;
		}

		private static void Pawn_GetGizmos_Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
		{
			if (__result != null)
			{
				List<Gizmo> listedGizmos = __result.ToList<Gizmo>();
				CompSuccubusRace compSuccubusRace = ThingCompUtility.TryGetComp<CompSuccubusRace>(__instance);
				bool flag2 = compSuccubusRace != null;
				if (commonUtl.IsSuccubus(__instance))
				{
					foreach (Gizmo item in compSuccubusRace.CompGetGizmosExtra())
					{
						listedGizmos.Add(item);
					}
				}
				__result = listedGizmos;
			}
		}

		private static bool Pawn_PreApplyDamage_Prefix(Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
		{
			if (commonUtl.IsSuccubus(__instance))
			{
				bool wearingSuccubusOutfit = GenCollection.Any<Apparel>(__instance.apparel.WornApparel, (Apparel outfit) => outfit.def == NIThingDefOf.Apparel_NIHalfPersona);
				if (wearingSuccubusOutfit)
				{
					if (Rand.Chance(0.1f))
					{
						MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "Dodge", Color.white, 3.65f);
						absorbed = true;
						return false;
					}
				}
				bool wearingSuccubusArmor = GenCollection.Any<Apparel>(__instance.apparel.WornApparel, (Apparel armor) => armor.def == NIThingDefOf.Apparel_NIBattleGarb);
				if (wearingSuccubusArmor)
				{
					dinfo.SetAmount(dinfo.Amount * 0.7f);
				}
			}
			return true;
		}

		private static bool Thing_Ingested_Prefix(Pawn ingester, float nutritionWanted, Thing __instance)
		{
			if (__instance ==  null)
			{
				return true;
			}
			if (__instance.Destroyed)
			{
				return true;
			}
			if (!__instance.IngestibleNow)
			{
				return true;
			}
			
			CompSuccubusRace comp = ingester.GetComp<CompSuccubusRace>();
			if (comp != null)
			{
				if (CommonData.ManaSourceDefs.Any((ThingDef manaSource) => __instance.def == manaSource))
				{
					IngestionOutcomeDoer_OffsetNeed ingestionOutcomeOffset = __instance.def.ingestible.outcomeDoers.FirstOrDefault(delegate(IngestionOutcomeDoer ingestionOutcomer)
					{
						IngestionOutcomeDoer_OffsetNeed ingestionOutcomeOffset2;
						return (ingestionOutcomeOffset2 = (ingestionOutcomer as IngestionOutcomeDoer_OffsetNeed)) != null && ingestionOutcomeOffset2.need == NINeedDefOf.NI_Need_Mana;
					}) as IngestionOutcomeDoer_OffsetNeed;
					comp.TotalManaGained = Mathf.Min(comp.TotalManaGained + ingestionOutcomeOffset.offset, 9999f);
				}
			}
			return true;
		}
	}
}
