using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    public class JobDriver_ManaFeedPatient : JobDriver
    {
        private const TargetIndex ManaSourceInd = TargetIndex.A;

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
            bool manaSourceIsNutrientDispenser = this.job.GetTarget(ManaSourceInd).Thing is Building_NutrientPasteDispenser && this.Deliveree != null;
            if (manaSourceIsNutrientDispenser)
            {
                return this.job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label).Replace("TargetB", this.Deliveree.LabelShort);
            }
            return base.GetReport();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo localTargetInfo = this.Deliveree;
            Job feedPatientMana = this.job;
            bool canNotReserveJob = !ReservationUtility.Reserve(pawn, localTargetInfo, feedPatientMana, 1, -1, null, errorOnFailed);
            if (canNotReserveJob)
            {
                return false;
            }
            bool patientIsNotSuccubus = ThingCompUtility.TryGetComp<CompSuccubusRace>(base.TargetThingB) == null;
            if (patientIsNotSuccubus)
            {
                return false;
            }
            
            bool isNotNutrientDispenserOrEmpty = !(base.TargetThingA is Building_NutrientPasteDispenser) && (this.pawn.inventory == null || !this.pawn.inventory.Contains(base.TargetThingA));
            if (isNotNutrientDispenserOrEmpty)
            {
                //why is it set here again?
                pawn = this.pawn;
                localTargetInfo = this.Food;
                feedPatientMana = this.job;
                bool canNotReserveJob2 = !ReservationUtility.Reserve(pawn, localTargetInfo, feedPatientMana, 1, -1, null, errorOnFailed);
                if (canNotReserveJob2)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_ManaFeedPatient>(this, DelivereeInd);
            ToilFailConditions.FailOn<JobDriver_ManaFeedPatient>(this, delegate()
            {
                if (!FoodUtility.ShouldBeFedBySomeone(this.Deliveree))
                {
                    Need_Mana need_Mana;
                    if ((need_Mana = (this.Deliveree.needs.AllNeeds.Find((Need pawnNeed) => pawnNeed.def == NINeedDefOf.NI_Need_Mana) as Need_Mana)) != null)
                    {
                        return need_Mana.CurCategory == ManaAmountCategory.Poverty;
                    }
                }
                return false;
            });
            bool inventoryContainsTargetItem = this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA);
            if (inventoryContainsTargetItem)
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, ManaSourceInd);
            }
            else
            {
                yield return ToilFailConditions.FailOnForbidden<Toil>(Toils_Goto.GotoThing(ManaSourceInd, PathEndMode.ClosestTouch), ManaSourceInd);
                yield return Toils_Ingest.PickupIngestible(ManaSourceInd, this.Deliveree);
            }
            yield return Toils_Goto.GotoThing(DelivereeInd, PathEndMode.Touch);
            yield return ToilFailConditions.FailOnCannotTouch<Toil>(Toils_Ingest.ChewIngestible(this.Deliveree, 1.5f, ManaSourceInd, 0), DelivereeInd,PathEndMode.Touch);
            yield return Toils_Ingest.FinalizeIngest(this.Deliveree, ManaSourceInd);
            yield break;
        }

    }
}
