using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	// Token: 0x02000002 RID: 2
	public class WorkGiver_ManaFeedPatient : WorkGiver_Scanner
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(11);
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000002 RID: 2 RVA: 0x0000206C File Offset: 0x0000026C
		public override PathEndMode PathEndMode
		{
			get
			{
				return 3;
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002080 File Offset: 0x00000280
		public override Danger MaxPathDanger(Pawn pawn)
		{
			return 3;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002094 File Offset: 0x00000294
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			bool flag = pawn2 == null || pawn2 == pawn;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = pawn2.GetComp<CompSuccubusRace>() == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = this.def.feedHumanlikesOnly && !pawn2.RaceProps.Humanlike;
					if (flag3)
					{
						result = false;
					}
					else
					{
						bool flag4 = this.def.feedAnimalsOnly && !pawn2.RaceProps.Animal;
						if (flag4)
						{
							result = false;
						}
						else
						{
							Need_Mana need_Mana = pawn2.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
							bool flag5 = need_Mana == null || need_Mana.CurLevel > 0f;
							if (flag5)
							{
								result = false;
							}
							else
							{
								bool flag6 = !FeedPatientUtility.ShouldBeFed(pawn2) || need_Mana.CurLevel > 0f;
								if (flag6)
								{
									result = false;
								}
								else
								{
									LocalTargetInfo localTargetInfo = t;
									bool flag7 = !ReservationUtility.CanReserve(pawn, localTargetInfo, 1, -1, null, forced);
									if (flag7)
									{
										result = false;
									}
									else
									{
										Thing thing;
										ThingDef thingDef;
										bool flag8 = !FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == 3, ref thing, ref thingDef, false, true, false, true, false, false, false);
										if (flag8)
										{
											JobFailReason.Is(Translator.Translate("NoFood"), null);
											result = false;
										}
										else
										{
											result = true;
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000221C File Offset: 0x0000041C
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn)t;
			Thing thing;
			ThingDef thingDef;
			bool flag = FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == 3, ref thing, ref thingDef, false, true, false, true, false, false, false);
			Job result;
			if (flag)
			{
				float nutrition = FoodUtility.GetNutrition(thing, thingDef);
				result = new Job(JobDefOf.FeedPatient)
				{
					targetA = thing,
					targetB = pawn2,
					count = FoodUtility.WillIngestStackCountOf(pawn2, thingDef, nutrition)
				};
			}
			else
			{
				result = null;
			}
			return result;
		}
	}
}
