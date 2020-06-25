using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	// Token: 0x0200000E RID: 14
	public class WorkGiver_NIFeedManaItem : WorkGiver_Scanner
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000061 RID: 97 RVA: 0x00006060 File Offset: 0x00004260
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(11);
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000062 RID: 98 RVA: 0x0000607C File Offset: 0x0000427C
		public override PathEndMode PathEndMode
		{
			get
			{
				return 3;
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00006090 File Offset: 0x00004290
		public override Danger MaxPathDanger(Pawn pawn)
		{
			return 3;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000060A4 File Offset: 0x000042A4
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
				bool flag2 = !commonUtl.IsSuccubus(pawn2);
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !FoodUtility.ShouldBeFedBySomeone(pawn2);
					if (flag3)
					{
						result = false;
					}
					else
					{
						Need_Mana need_Mana = pawn2.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
						bool flag4 = need_Mana == null || need_Mana.CurLevelPercentage > 0.35f;
						if (flag4)
						{
							result = false;
						}
						else
						{
							LocalTargetInfo localTargetInfo = t;
							bool flag5 = !ReservationUtility.CanReserve(pawn, localTargetInfo, 1, -1, null, forced);
							if (flag5)
							{
								result = false;
							}
							else
							{
								Thing thing;
								ThingDef thingDef;
								bool flag6 = !commonUtl.TryFindBestManaSourceFor(pawn, pawn2, out thing, out thingDef, true, false, false);
								if (flag6)
								{
									JobFailReason.Is(Translator.Translate("NItxt_NoManaSource"), null);
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
			return result;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x000061AC File Offset: 0x000043AC
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn)t;
			Thing thing;
			ThingDef thingDef;
			bool flag = commonUtl.TryFindBestManaSourceFor(pawn, pawn2, out thing, out thingDef, true, false, false);
			Job result;
			if (flag)
			{
				result = new Job(NIJobDefOf.Job_NIFeedManaItem)
				{
					targetA = thing,
					targetB = pawn2,
					count = commonUtl.EstimateIngestCountOf(pawn2, thing)
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
