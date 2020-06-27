using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x02000008 RID: 8
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		// Token: 0x06000025 RID: 37 RVA: 0x00003574 File Offset: 0x00001774
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

		// Token: 0x06000026 RID: 38 RVA: 0x00003698 File Offset: 0x00001898
		private static bool Pawn_HealthTrackerAddHediffPrefix(Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			bool flag = HarmonyPatches.HasResistFor(___pawn, hediff, part, dinfo, result);
			bool result2;
			if (flag)
			{
				Log.Message(___pawn.Label + "に" + hediff.Label + "の状態異常は効かない", false);
				result2 = false;
			}
			else
			{
				result2 = true;
			}
			return result2;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000036E4 File Offset: 0x000018E4
		private static bool HasResistFor(Pawn pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			bool flag = pawn.GetComp<CompSuccubusRace>() != null;
			if (flag)
			{
				bool flag2 = hediff is Hediff_Pregnant || HarmonyPatches.FleshHediffInvalidFor(pawn, hediff);
				bool flag3 = flag2;
				if (flag3)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003728 File Offset: 0x00001928
		private static bool FleshHediffInvalidFor(Pawn pawn, Hediff hediff)
		{
			bool flag = !pawn.RaceProps.IsFlesh;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = false;
				bool flag3 = hediff.def == HediffDefOf.Anesthetic;
				if (flag3)
				{
					flag2 = (pawn.Faction == null || !pawn.Faction.IsPlayer || pawn.CurJob == null || pawn.CurJobDef != JobDefOf.LayDown || (pawn.jobs.posture != PawnPosture.LayingInBed && pawn.jobs.posture != PawnPosture.LayingOnGroundNormal));
				}
				bool flag4 = hediff.def == HediffDefOf.BloodLoss;
				if (flag4)
				{
					HediffSet hediffSet = pawn.health.hediffSet;
					flag2 = (hediffSet != null && hediffSet.BleedRateTotal < 0.1f);
				}
				bool flag5 = hediff.def == HediffDefOf.Malnutrition;
				if (flag5)
				{
					Pawn_NeedsTracker needs = pawn.needs;
					bool flag6;
					if (needs == null)
					{
						flag6 = false;
					}
					else
					{
						Need_Food food = needs.food;
						flag6 = (((food != null) ? new float?(food.CurLevel) : null) > 0f);
					}
					flag2 = flag6;
				}
				result = flag2;
			}
			return result;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003850 File Offset: 0x00001A50
		private static void Pawn_GetGizmos_Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
		{
			bool flag = __result == null;
			if (!flag)
			{
				List<Gizmo> list = __result.ToList<Gizmo>();
				CompSuccubusRace compSuccubusRace = ThingCompUtility.TryGetComp<CompSuccubusRace>(__instance);
				bool flag2 = compSuccubusRace != null;
				if (flag2)
				{
					foreach (Gizmo item in compSuccubusRace.CompGetGizmosExtra())
					{
						list.Add(item);
					}
				}
				__result = list;
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x000038D4 File Offset: 0x00001AD4
		private static bool Pawn_PreApplyDamage_Prefix(Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
		{
			bool flag = __instance.GetComp<CompSuccubusRace>() != null;
			if (flag)
			{
				bool flag2 = GenCollection.Any<Apparel>(__instance.apparel.WornApparel, (Apparel x) => x.def == NIThingDefOf.Apparel_NIHalfPersona);
				if (flag2)
				{
					bool flag3 = Rand.Chance(0.1f);
					if (flag3)
					{
						MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, Translator.Translate("回避"), Color.white, 3.65f);
						absorbed = true;
						return false;
					}
				}
				bool flag4 = GenCollection.Any<Apparel>(__instance.apparel.WornApparel, (Apparel x) => x.def == NIThingDefOf.Apparel_NIBattleGarb);
				if (flag4)
				{
					dinfo.SetAmount(dinfo.Amount * 0.7f);
				}
			}
			return true;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x000039BC File Offset: 0x00001BBC
		private static bool Thing_Ingested_Prefix(Pawn ingester, float nutritionWanted, Thing __instance)
		{
			bool flag = __instance == null;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool destroyed = __instance.Destroyed;
				if (destroyed)
				{
					result = true;
				}
				else
				{
					bool flag2 = !__instance.IngestibleNow;
					if (flag2)
					{
						result = true;
					}
					else
					{
						CompSuccubusRace comp;
						bool flag3 = (comp = ingester.GetComp<CompSuccubusRace>()) != null;
						if (flag3)
						{
							bool flag4 = CommonData.ManaSourceDefs.Any((ThingDef x) => __instance.def == x);
							if (flag4)
							{
								IngestionOutcomeDoer_OffsetNeed ingestionOutcomeDoer_OffsetNeed = __instance.def.ingestible.outcomeDoers.FirstOrDefault(delegate(IngestionOutcomeDoer x)
								{
									IngestionOutcomeDoer_OffsetNeed ingestionOutcomeDoer_OffsetNeed2;
									return (ingestionOutcomeDoer_OffsetNeed2 = (x as IngestionOutcomeDoer_OffsetNeed)) != null && ingestionOutcomeDoer_OffsetNeed2.need == NINeedDefOf.NI_Need_Mana;
								}) as IngestionOutcomeDoer_OffsetNeed;
								comp.TotalManaGained = Mathf.Min(comp.TotalManaGained + ingestionOutcomeDoer_OffsetNeed.offset, 9999f);
							}
						}
						result = true;
					}
				}
			}
			return result;
		}
	}
}
