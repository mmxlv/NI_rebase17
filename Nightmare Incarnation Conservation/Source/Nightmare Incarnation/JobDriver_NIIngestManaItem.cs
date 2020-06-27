using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	// Token: 0x0200000D RID: 13
	public class JobDriver_NIIngestManaItem : JobDriver
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00005930 File Offset: 0x00003B30
		private Thing IngestibleSource
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600004C RID: 76 RVA: 0x00005958 File Offset: 0x00003B58
		private float ChewDurationMultiplier
		{
			get
			{
				Thing ingestibleSource = this.IngestibleSource;
				bool flag = ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat;
				float result;
				if (flag)
				{
					result = 1f;
				}
				else
				{
					result = 1f / StatExtension.GetStatValue(this.pawn, StatDefOf.EatingSpeed, true);
				}
				return result;
			}
		}

		// Token: 0x0600004D RID: 77 RVA: 0x000059B8 File Offset: 0x00003BB8
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.usingNutrientPasteDispenser, "usingNutrientPasteDispenser", false, false);
			Scribe_Values.Look<bool>(ref this.eatingFromInventory, "eatingFromInventory", false, false);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000059E8 File Offset: 0x00003BE8
		public override string GetReport()
		{
			bool flag = this.usingNutrientPasteDispenser;
			string result;
			if (flag)
			{
				result = this.job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label);
			}
			else
			{
				Thing thing = this.job.targetA.Thing;
				bool flag2 = thing != null && thing.def.ingestible != null;
				if (flag2)
				{
					bool flag3 = !GenText.NullOrEmpty(thing.def.ingestible.ingestReportStringEat) && (GenText.NullOrEmpty(thing.def.ingestible.ingestReportString) || this.pawn.RaceProps.intelligence < Intelligence.ToolUser);
					if (flag3)
					{
						return string.Format(thing.def.ingestible.ingestReportStringEat, this.job.targetA.Thing.LabelShort);
					}
					bool flag4 = !GenText.NullOrEmpty(thing.def.ingestible.ingestReportString);
					if (flag4)
					{
						return string.Format(thing.def.ingestible.ingestReportString, this.job.targetA.Thing.LabelShort);
					}
				}
				result = base.GetReport();
			}
			return result;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00005B2C File Offset: 0x00003D2C
		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.usingNutrientPasteDispenser = (this.IngestibleSource is Building_NutrientPasteDispenser);
			this.eatingFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(this.IngestibleSource));
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00005B84 File Offset: 0x00003D84
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			bool flag = this.pawn.Faction != null && !(this.IngestibleSource is Building_NutrientPasteDispenser);
			if (flag)
			{
				Thing ingestibleSource = this.IngestibleSource;
				int num = FoodUtility.WillIngestStackCountOf(this.pawn, ingestibleSource.def, StatExtension.GetStatValue(ingestibleSource, StatDefOf.Nutrition, true));
				bool flag2 = num >= ingestibleSource.stackCount && ingestibleSource.Spawned;
				if (flag2)
				{
					Pawn pawn = this.pawn;
					LocalTargetInfo localTargetInfo = ingestibleSource;
					Job job = this.job;
					bool flag3 = !ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
					if (flag3)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00005C33 File Offset: 0x00003E33
		protected override IEnumerable<Toil> MakeNewToils()
		{
			bool flag = !this.usingNutrientPasteDispenser;
			if (flag)
			{
				ToilFailConditions.FailOn<JobDriver_NIIngestManaItem>(this, () => !this.IngestibleSource.Destroyed && !this.IngestibleSource.IngestibleNow);
			}
			Toil chew = ToilFailConditions.FailOnCannotTouch<Toil>(ToilFailConditions.FailOn(Toils_Ingest.ChewIngestible(this.pawn, this.ChewDurationMultiplier, TargetIndex.A, TargetIndex.B), (Toil x) => !this.IngestibleSource.Spawned && (this.pawn.carryTracker == null || this.pawn.carryTracker.CarriedThing != this.IngestibleSource)), TargetIndex.A,PathEndMode.Touch);
			foreach (Toil toil in this.PrepareToIngestToils(chew))
			{
				yield return toil;
				//toil = null;
			}
			IEnumerator<Toil> enumerator = null;
			yield return chew;
			yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(chew, () => this.job.GetTarget(TargetIndex.A).Thing is Corpse && this.pawn.needs.food.CurLevelPercentage < 0.9f);
			yield break;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00005C44 File Offset: 0x00003E44
		private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
		{
			bool flag = this.usingNutrientPasteDispenser;
			IEnumerable<Toil> result;
			if (flag)
			{
				result = this.PrepareToIngestToils_Dispenser();
			}
			else
			{
				bool toolUser = this.pawn.RaceProps.ToolUser;
				if (toolUser)
				{
					result = this.PrepareToIngestToils_ToolUser(chewToil);
				}
				else
				{
					result = this.PrepareToIngestToils_NonToolUser();
				}
			}
			return result;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00005C8F File Offset: 0x00003E8F
		private IEnumerable<Toil> PrepareToIngestToils_Dispenser()
		{
			yield return ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing(TargetIndex.A ,PathEndMode.InteractionCell), TargetIndex.A);
			yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, this.pawn);
			yield return ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A), TargetIndex.A);
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
			yield break;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00005C9F File Offset: 0x00003E9F
		private IEnumerable<Toil> PrepareToIngestToils_ToolUser(Toil chewToil)
		{
			bool flag = this.eatingFromInventory;
			if (flag)
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else
			{
				//JobDriver_NIIngestManaItem.<>c__DisplayClass17_0 CS$<>8__locals1 = new JobDriver_NIIngestManaItem.<>c__DisplayClass17_0();
				//CS$<>8__locals1.<>4__this = this;
				yield return this.ReserveFoodIfWillIngestWholeStack();
				Toil gotoToPickup = ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch), TargetIndex.A);
				yield return Toils_Jump.JumpIf(gotoToPickup, () => this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
				yield return ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch), TargetIndex.A);
				yield return Toils_Jump.Jump(chewToil);
				yield return gotoToPickup;
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.pawn);
				//CS$<>8__locals1.reserveExtraFoodToCollect = Toils_Reserve.Reserve(3, 1, -1, null);
				Toil reserveExtraFoodToCollect = Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
				Toil findExtraFoodToCollect = new Toil();
				findExtraFoodToCollect.initAction = delegate()
				{
					//bool flag2 = CS$<>8__locals1.<>4__this.pawn.inventory.innerContainer.TotalStackCountOfDef(CS$<>8__locals1.<>4__this.IngestibleSource.def) < CS$<>8__locals1.<>4__this.job.takeExtraIngestibles;
					bool flag2 =this.pawn.inventory.innerContainer.TotalStackCountOfDef(this.IngestibleSource.def) > this.job.takeExtraIngestibles;
					if (flag2)
					{
						//Thing thing = GenClosest.ClosestThingReachable(CS$<> 8__locals1.<> 4__this.pawn.Position, CS$<> 8__locals1.<> 4__this.pawn.Map, ThingRequest.ForDef(CS$<> 8__locals1.<> 4__this.IngestibleSource.def), 2, TraverseParms.For(CS$<> 8__locals1.<> 4__this.pawn, 3, 0, false), 12f, (Thing x) => ReservationUtility.CanReserve(CS$<> 8__locals1.<> 4__this.pawn, x, 1, -1, null, false) && !ForbidUtility.IsForbidden(x, CS$<> 8__locals1.<> 4__this.pawn) && SocialProperness.IsSociallyProper(x, CS$<> 8__locals1.<> 4__this.pawn), null, 0, -1, false, 6, false);
						Thing thing = GenClosest.ClosestThingReachable(this.pawn.Position, this.pawn.Map, ThingRequest.ForDef(this.IngestibleSource.def),PathEndMode.Touch, TraverseParms.For(this.pawn,Danger.Deadly, 0, false), 12f, (Thing x) => ReservationUtility.CanReserve(this.pawn, x, 1, -1, null, false) && !ForbidUtility.IsForbidden(x, this.pawn) && SocialProperness.IsSociallyProper(x, this.pawn), null, 0, -1, false,RegionType.Set_Passable, false);
						bool flag3 = thing != null;
						if (flag3)
						{
							this.job.SetTarget(TargetIndex.C, thing);
							this.JumpToToil(reserveExtraFoodToCollect);
						}
					}
				};
				findExtraFoodToCollect.defaultCompleteMode = ToilCompleteMode.Instant;
				yield return Toils_Jump.Jump(findExtraFoodToCollect);
				yield return reserveExtraFoodToCollect;
				yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
				yield return Toils_Haul.TakeToInventory(TargetIndex.C, () => this.job.takeExtraIngestibles - this.pawn.inventory.innerContainer.TotalStackCountOfDef(this.IngestibleSource.def));
				yield return findExtraFoodToCollect;
				reserveExtraFoodToCollect = null;
				gotoToPickup = null;
				findExtraFoodToCollect = null;
			}
			yield return ToilFailConditions.FailOnDestroyedOrNull<Toil>(Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A), TargetIndex.A);
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
			yield break;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00005CB6 File Offset: 0x00003EB6
		private IEnumerable<Toil> PrepareToIngestToils_NonToolUser()
		{
			yield return this.ReserveFoodIfWillIngestWholeStack();
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield break;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00005CC8 File Offset: 0x00003EC8
		private Toil ReserveFoodIfWillIngestWholeStack()
		{
			return new Toil
			{
				initAction = delegate()
				{
					bool flag = this.pawn.Faction == null;
					if (!flag)
					{
						Thing thing = this.job.GetTarget(TargetIndex.A).Thing;
						bool flag2 = this.pawn.carryTracker.CarriedThing == thing;
						if (!flag2)
						{
							int num = FoodUtility.WillIngestStackCountOf(this.pawn, thing.def, StatExtension.GetStatValue(thing, StatDefOf.Nutrition, true));
							bool flag3 = num >= thing.stackCount;
							if (flag3)
							{
								bool flag4 = !thing.Spawned;
								if (flag4)
								{
									this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
								}
								else
								{
									ReservationUtility.Reserve(this.pawn, thing, this.job, 1, -1, null, true);
								}
							}
						}
					}
				},
				defaultCompleteMode =ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00005D00 File Offset: 0x00003F00
		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 cell = this.job.GetTarget(TargetIndex.B).Cell;
			return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, this.pawn);
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00005D38 File Offset: 0x00003F38
		public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
		{
			bool moving = pawn.pather.Moving;
			bool result;
			if (moving)
			{
				result = false;
			}
			else
			{
				Thing carriedThing = pawn.carryTracker.CarriedThing;
				bool flag = carriedThing == null || !carriedThing.IngestibleNow;
				if (flag)
				{
					result = false;
				}
				else
				{
					bool flag2 = placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && GenGrid.HasEatSurface(placeCell, pawn.Map) && carriedThing.def.ingestible.ingestHoldUsesTable;
					if (flag2)
					{
						drawPos = new Vector3((float)placeCell.x + 0.5f, drawPos.y, (float)placeCell.z + 0.5f);
						result = true;
					}
					else
					{
						bool flag3 = carriedThing.def.ingestible.ingestHoldOffsetStanding != null;
						if (flag3)
						{
							HoldOffset holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
							bool flag4 = holdOffset != null;
							if (flag4)
							{
								drawPos += holdOffset.offset;
								behind = holdOffset.behind;
								flip = holdOffset.flip;
								return true;
							}
						}
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x04000066 RID: 102
		private bool usingNutrientPasteDispenser;

		// Token: 0x04000067 RID: 103
		private bool eatingFromInventory;

		// Token: 0x04000068 RID: 104
		public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

		// Token: 0x04000069 RID: 105
		public const TargetIndex IngestibleSourceInd =TargetIndex.A;

		// Token: 0x0400006A RID: 106
		private const TargetIndex TableCellInd = TargetIndex.B;

		// Token: 0x0400006B RID: 107
		private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;
	}

}
