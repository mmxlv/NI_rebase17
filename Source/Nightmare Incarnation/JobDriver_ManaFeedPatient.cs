using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	// Token: 0x02000003 RID: 3
	public class JobDriver_ManaFeedPatient : JobDriver
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000007 RID: 7 RVA: 0x000022AC File Offset: 0x000004AC
		protected Thing Food
		{
			get
			{
				return this.job.targetA.Thing;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000008 RID: 8 RVA: 0x000022D0 File Offset: 0x000004D0
		protected Pawn Deliveree
		{
			get
			{
				return (Pawn)this.job.targetB.Thing;
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000022F8 File Offset: 0x000004F8
		public override string GetReport()
		{
			bool flag = this.job.GetTarget(TargetIndex.A).Thing is Building_NutrientPasteDispenser && this.Deliveree != null;
			string result;
			if (flag)
			{
				result = this.job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label).Replace("TargetB", this.Deliveree.LabelShort);
			}
			else
			{
				result = base.GetReport();
			}
			return result;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002378 File Offset: 0x00000578
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo localTargetInfo = this.Deliveree;
			Job job = this.job;
			bool flag = !ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = ThingCompUtility.TryGetComp<CompSuccubusRace>(base.TargetThingB) == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !(base.TargetThingA is Building_NutrientPasteDispenser) && (this.pawn.inventory == null || !this.pawn.inventory.Contains(base.TargetThingA));
					if (flag3)
					{
						pawn = this.pawn;
						localTargetInfo = this.Food;
						job = this.job;
						bool flag4 = !ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
						if (flag4)
						{
							return false;
						}
					}
					result = true;
				}
			}
			return result;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002453 File Offset: 0x00000653
		protected override IEnumerable<Toil> MakeNewToils()
		{
			ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_ManaFeedPatient>(this, TargetIndex.B);
			ToilFailConditions.FailOn<JobDriver_ManaFeedPatient>(this, delegate()
			{
				if (!FoodUtility.ShouldBeFedBySomeone(this.Deliveree))
				{
					Need_Mana need_Mana;
					if ((need_Mana = (this.Deliveree.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana)) != null)
					{
						return need_Mana.CurCategory == ManaAmountCategory.Poverty;
					}
				}
				return false;
			});
			bool flag = this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA);
			if (flag)
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else
			{
				yield return ToilFailConditions.FailOnForbidden<Toil>(Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch), TargetIndex.A);
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.Deliveree);
			}
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			yield return ToilFailConditions.FailOnCannotTouch<Toil>(Toils_Ingest.ChewIngestible(this.Deliveree, 1.5f, TargetIndex.A, 0), TargetIndex.B,PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(this.Deliveree, TargetIndex.A);
			yield break;
		}

		// Token: 0x04000001 RID: 1
		private const TargetIndex ManaSourceInd = TargetIndex.A;

		// Token: 0x04000002 RID: 2
		private const TargetIndex DelivereeInd = TargetIndex.B;

		// Token: 0x04000003 RID: 3
		private const float FeedDurationMultiplier = 1.5f;
	}
}
