using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	public class JobDriver_NIFeedManaItem : JobDriver
	{
		private const TargetIndex FoodSourceInd = TargetIndex.A;

		private const TargetIndex DelivereeInd = TargetIndex.B;

		private const float FeedDurationMultiplier = 1.5f;
		protected Thing Food
		{
			get
			{
				return this.job.targetA.Thing;
			}
		}

		protected Pawn Deliveree
		{
			get
			{
				return (Pawn)this.job.targetB.Thing;
			}
		}

		public override string GetReport()
		{
			bool foodSourceIsNutrientDispenser = this.job.GetTarget(FoodSourceInd).Thing is Building_NutrientPasteDispenser && this.Deliveree != null;
			if (foodSourceIsNutrientDispenser)
			{
				return this.job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label).Replace("TargetB", this.Deliveree.LabelShort);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo localTargetInfo = this.Deliveree;
			Job job = this.job;
			bool canNotReserveJob = !ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
			if (canNotReserveJob)
			{
				return false;
			}
			bool isNotNutrientDispenserOrEmpty = !(base.TargetThingA is Building_NutrientPasteDispenser) && (this.pawn.inventory == null || !this.pawn.inventory.Contains(base.TargetThingA));
			if (isNotNutrientDispenserOrEmpty)
			{
				pawn = this.pawn;
				localTargetInfo = this.Food;
				job = this.job;
				bool canNotReserve = !ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
				if (canNotReserve)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_NIFeedManaItem>(this, DelivereeInd);
			ToilFailConditions.FailOn<JobDriver_NIFeedManaItem>(this, () => !FoodUtility.ShouldBeFedBySomeone(this.Deliveree));
			bool inventoryContainsItem = this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA);
			if (inventoryContainsItem)
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, FoodSourceInd);
			}
			else
			{
				bool isNutrientDispenser = base.TargetThingA is Building_NutrientPasteDispenser;
				if (isNutrientDispenser)
				{
					yield return ToilFailConditions.FailOnForbidden<Toil>(Toils_Goto.GotoThing(FoodSourceInd, PathEndMode.InteractionCell), FoodSourceInd);
					yield return Toils_Ingest.TakeMealFromDispenser(FoodSourceInd, this.pawn);
				}
				else
				{
					yield return ToilFailConditions.FailOnForbidden<Toil>(Toils_Goto.GotoThing(FoodSourceInd, PathEndMode.ClosestTouch), FoodSourceInd);
					yield return Toils_Ingest.PickupIngestible(FoodSourceInd, this.Deliveree);
				}
			}
			yield return Toils_Goto.GotoThing(DelivereeInd, PathEndMode.Touch);
			yield return ToilFailConditions.FailOnCannotTouch<Toil>(Toils_Ingest.ChewIngestible(this.Deliveree, 1.5f, FoodSourceInd, 0), DelivereeInd,PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(this.Deliveree, FoodSourceInd);
			yield break;
		}
	}
}
