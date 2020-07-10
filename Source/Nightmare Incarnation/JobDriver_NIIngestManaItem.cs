using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    public class JobDriver_NIIngestManaItem : JobDriver
    {

        private bool usingNutrientPasteDispenser;

        private bool eatingFromInventory;

        public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

        public const TargetIndex IngestibleSourceInd = TargetIndex.A;

        private const TargetIndex TableCellInd = TargetIndex.B;

        private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

        private Thing IngestibleSource
        {
            get
            {
                return this.job.GetTarget(IngestibleSourceInd).Thing;
            }
        }

        private float ChewDurationMultiplier
        {
            get
            {
                Thing ingestibleSource = this.IngestibleSource;
                bool flag = ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat;
                if (flag)
                    return 1f;
                return 1f / StatExtension.GetStatValue(this.pawn, StatDefOf.EatingSpeed, true);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.usingNutrientPasteDispenser, "usingNutrientPasteDispenser", false, false);
            Scribe_Values.Look<bool>(ref this.eatingFromInventory, "eatingFromInventory", false, false);
        }

        public override string GetReport()
        {
            if (this.usingNutrientPasteDispenser)
                return this.job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label);

            Thing thing = this.job.targetA.Thing;
            bool isIngestible = thing != null && thing.def.ingestible != null;
            if (isIngestible)
            {
                bool pawnIsDumb = !GenText.NullOrEmpty(thing.def.ingestible.ingestReportStringEat) && (GenText.NullOrEmpty(thing.def.ingestible.ingestReportString) || this.pawn.RaceProps.intelligence < Intelligence.ToolUser);
                if (pawnIsDumb)
                    return string.Format(thing.def.ingestible.ingestReportStringEat, this.job.targetA.Thing.LabelShort);
                return string.Format(thing.def.ingestible.ingestReportString, this.job.targetA.Thing.LabelShort);
            }
            return base.GetReport();
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.usingNutrientPasteDispenser = (this.IngestibleSource is Building_NutrientPasteDispenser);
            this.eatingFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(this.IngestibleSource));
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool notNutrientDispenser = this.pawn.Faction != null && !(this.IngestibleSource is Building_NutrientPasteDispenser);
            if (notNutrientDispenser)
            {
                Thing ingestibleSource = this.IngestibleSource;
                int num = FoodUtility.WillIngestStackCountOf(this.pawn, ingestibleSource.def, StatExtension.GetStatValue(ingestibleSource, StatDefOf.Nutrition, true));
                bool enoughFoodToEat = num >= ingestibleSource.stackCount && ingestibleSource.Spawned;
                if (enoughFoodToEat)
                {
                    Pawn pawn = this.pawn;
                    LocalTargetInfo localTargetInfo = ingestibleSource;
                    Job job = this.job;
                    bool reservedJob = ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
                    if (!reservedJob)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Method seems not fully implememented, it doesn't return an enumerable
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            bool notUsingNutrient = !this.usingNutrientPasteDispenser;
            if (notUsingNutrient)
            {
                ToilFailConditions.FailOn<JobDriver_NIIngestManaItem>(this, () => !this.IngestibleSource.Destroyed && !this.IngestibleSource.IngestibleNow);
            }
            Toil chew = ToilFailConditions.FailOnCannotTouch<Toil>(ToilFailConditions.FailOn(Toils_Ingest.ChewIngestible(this.pawn, this.ChewDurationMultiplier, IngestibleSourceInd, TableCellInd), (Toil x) => !this.IngestibleSource.Spawned && (this.pawn.carryTracker == null || this.pawn.carryTracker.CarriedThing != this.IngestibleSource)), IngestibleSourceInd,PathEndMode.Touch);
            foreach (Toil toil in this.PrepareToIngestToils(chew))
            {
                yield return toil;
            }
            yield return chew;
            yield return Toils_Ingest.FinalizeIngest(this.pawn, IngestibleSourceInd);
            yield return Toils_Jump.JumpIf(chew, () => this.job.GetTarget(IngestibleSourceInd).Thing is Corpse && this.pawn.needs.food.CurLevelPercentage < 0.9f);
            yield break;
        }

        private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
        {
            bool flag = this.usingNutrientPasteDispenser;
            if (this.usingNutrientPasteDispenser)
                return this.PrepareToIngestToils_Dispenser();
            if (this.pawn.RaceProps.ToolUser)
                return this.PrepareToIngestToils_ToolUser(chewToil);
            return this.PrepareToIngestToils_NonToolUser();
        }

        private IEnumerable<Toil> PrepareToIngestToils_Dispenser()
        {
            yield return ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing(IngestibleSourceInd ,PathEndMode.InteractionCell), IngestibleSourceInd);
            yield return Toils_Ingest.TakeMealFromDispenser(IngestibleSourceInd, this.pawn);
            yield return ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, IngestibleSourceInd), IngestibleSourceInd);
            yield return Toils_Ingest.FindAdjacentEatSurface(TableCellInd, IngestibleSourceInd);
            yield break;
        }

        private IEnumerable<Toil> PrepareToIngestToils_ToolUser(Toil chewToil)
        {
            if (this.eatingFromInventory)
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, IngestibleSourceInd);
            }
            else
            {
                yield return this.ReserveFoodIfWillIngestWholeStack();
                Toil gotoToPickup = ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing(IngestibleSourceInd, PathEndMode.ClosestTouch), IngestibleSourceInd);
                yield return Toils_Jump.JumpIf(gotoToPickup, () => this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
                yield return ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing(IngestibleSourceInd, PathEndMode.Touch), IngestibleSourceInd);
                yield return Toils_Jump.Jump(chewToil);
                yield return gotoToPickup;
                yield return Toils_Ingest.PickupIngestible(IngestibleSourceInd, this.pawn);
                Toil reserveExtraFoodToCollect = Toils_Reserve.Reserve(IngestibleSourceInd, 1, -1, null);
                Toil findExtraFoodToCollect = new Toil();
                findExtraFoodToCollect.initAction = delegate()
                {
                    bool enoughFood =this.pawn.inventory.innerContainer.TotalStackCountOfDef(this.IngestibleSource.def) > this.job.takeExtraIngestibles;
                    if (enoughFood)
                    {
                        Thing thing = GenClosest.ClosestThingReachable(this.pawn.Position, this.pawn.Map, ThingRequest.ForDef(this.IngestibleSource.def),PathEndMode.Touch, TraverseParms.For(this.pawn,Danger.Deadly, 0, false), 12f, (Thing x) => ReservationUtility.CanReserve(this.pawn, x, 1, -1, null, false) && !ForbidUtility.IsForbidden(x, this.pawn) && SocialProperness.IsSociallyProper(x, this.pawn), null, 0, -1, false,RegionType.Set_Passable, false);
                        if (thing != null)
                        {
                            this.job.SetTarget(ExtraIngestiblesToCollectInd, thing);
                            this.JumpToToil(reserveExtraFoodToCollect);
                        }
                    }
                };
                findExtraFoodToCollect.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return Toils_Jump.Jump(findExtraFoodToCollect);
                yield return reserveExtraFoodToCollect;
                yield return Toils_Goto.GotoThing(ExtraIngestiblesToCollectInd, PathEndMode.Touch);
                yield return Toils_Haul.TakeToInventory(ExtraIngestiblesToCollectInd, () => this.job.takeExtraIngestibles - this.pawn.inventory.innerContainer.TotalStackCountOfDef(this.IngestibleSource.def));
                yield return findExtraFoodToCollect;
                reserveExtraFoodToCollect = null;
                gotoToPickup = null;
                findExtraFoodToCollect = null;
            }
            yield return ToilFailConditions.FailOnDestroyedOrNull<Toil>(Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, IngestibleSourceInd), IngestibleSourceInd);
            yield return Toils_Ingest.FindAdjacentEatSurface(TableCellInd, IngestibleSourceInd);
            yield break;
        }

        private IEnumerable<Toil> PrepareToIngestToils_NonToolUser()
        {
            yield return this.ReserveFoodIfWillIngestWholeStack();
            yield return Toils_Goto.GotoThing(IngestibleSourceInd, PathEndMode.Touch);
            yield break;
        }

        private Toil ReserveFoodIfWillIngestWholeStack()
        {
            return new Toil
            {
                initAction = delegate()
                {
                    bool pawnIsFactionLess = this.pawn.Faction == null;
                    if (!pawnIsFactionLess)
                    {
                        Thing foodItem = this.job.GetTarget(IngestibleSourceInd).Thing;
                        bool pawnIsCarryingFoodItem = this.pawn.carryTracker.CarriedThing == foodItem;
                        if (!pawnIsCarryingFoodItem)
                        {
                            int numberOfItemsToEat = FoodUtility.WillIngestStackCountOf(this.pawn, foodItem.def, StatExtension.GetStatValue(foodItem, StatDefOf.Nutrition, true));
                            bool moreHungerThanItems = numberOfItemsToEat >= foodItem.stackCount;
                            if (moreHungerThanItems)
                            {
                                if (!foodItem.Spawned)
                                {
                                    this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                                }
                                else
                                {
                                    ReservationUtility.Reserve(this.pawn, foodItem, this.job, 1, -1, null, true);
                                }
                            }
                        }
                    }
                },
                defaultCompleteMode =ToilCompleteMode.Instant,
                atomicWithPrevious = true
            };
        }

        public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
        {
            IntVec3 cell = this.job.GetTarget(TableCellInd).Cell;
            return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, this.pawn);
        }

        public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
        {
            if (pawn.pather.Moving)
                return false;
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing == null || !carriedThing.IngestibleNow)
                return false;
            bool pawnIsAtFoodLocation = placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && GenGrid.HasEatSurface(placeCell, pawn.Map) && carriedThing.def.ingestible.ingestHoldUsesTable;
            if (pawnIsAtFoodLocation)
            {
                drawPos = new Vector3((float)placeCell.x + 0.5f, drawPos.y, (float)placeCell.z + 0.5f);
                return true;
            }
            if (carriedThing.def.ingestible.ingestHoldOffsetStanding != null)
            {
                HoldOffset holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
                if (holdOffset != null)
                {
                    drawPos += holdOffset.offset;
                    behind = holdOffset.behind;
                    flip = holdOffset.flip;
                    return true;
                }
            }
            return false;
        }
    }

}
