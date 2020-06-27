using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	// Token: 0x0200000F RID: 15
	public class JobDriver_NIFeedManaItem : JobDriver
	{
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000067 RID: 103 RVA: 0x00006210 File Offset: 0x00004410
		protected Thing Food
		{
			get
			{
				return this.job.targetA.Thing;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000068 RID: 104 RVA: 0x00006234 File Offset: 0x00004434
		protected Pawn Deliveree
		{
			get
			{
				return (Pawn)this.job.targetB.Thing;
			}
		}

		// Token: 0x06000069 RID: 105 RVA: 0x0000625C File Offset: 0x0000445C
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

		// Token: 0x0600006A RID: 106 RVA: 0x000062DC File Offset: 0x000044DC
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
				bool flag2 = !(base.TargetThingA is Building_NutrientPasteDispenser) && (this.pawn.inventory == null || !this.pawn.inventory.Contains(base.TargetThingA));
				if (flag2)
				{
					pawn = this.pawn;
					localTargetInfo = this.Food;
					job = this.job;
					bool flag3 = !ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
					if (flag3)
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x0000639A File Offset: 0x0000459A
		protected override IEnumerable<Toil> MakeNewToils()
		{
			ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_NIFeedManaItem>(this, TargetIndex.B);
			ToilFailConditions.FailOn<JobDriver_NIFeedManaItem>(this, () => !FoodUtility.ShouldBeFedBySomeone(this.Deliveree));
			bool flag = this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA);
			if (flag)
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else
			{
				bool flag2 = base.TargetThingA is Building_NutrientPasteDispenser;
				if (flag2)
				{
					yield return ToilFailConditions.FailOnForbidden<Toil>(Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell), TargetIndex.A);
					yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, this.pawn);
				}
				else
				{
					yield return ToilFailConditions.FailOnForbidden<Toil>(Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch), TargetIndex.A);
					yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.Deliveree);
				}
			}
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			yield return ToilFailConditions.FailOnCannotTouch<Toil>(Toils_Ingest.ChewIngestible(this.Deliveree, 1.5f, TargetIndex.A, 0), TargetIndex.B,PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(this.Deliveree, TargetIndex.A);
			yield break;
		}

		// Token: 0x0400006C RID: 108
		private const TargetIndex FoodSourceInd = TargetIndex.A;

		// Token: 0x0400006D RID: 109
		private const TargetIndex DelivereeInd = TargetIndex.B;

		// Token: 0x0400006E RID: 110
		private const float FeedDurationMultiplier = 1.5f;
	}
}
