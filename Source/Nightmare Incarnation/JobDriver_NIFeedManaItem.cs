using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    /// <summary>
    /// Feed succubi who can't do it on their own.
    /// </summary>
    public class JobDriver_NIFeedManaItem : JobDriver
    {
        private const TargetIndex ManaSourceInd = TargetIndex.A;

        private const TargetIndex SuccubusInd = TargetIndex.B;

        protected Pawn Actor => GetActor();

        protected Thing ManaSource => job.GetTarget(ManaSourceInd).Thing;

        protected Pawn Succubus => job.GetTarget(SuccubusInd).Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return Actor.Reserve(Succubus, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(SuccubusInd);
            this.FailOn(() =>
            {
                return !FoodUtility.ShouldBeFedBySomeone(Succubus);
            });

            if (Actor.inventory != null && Actor.inventory.Contains(ManaSource))
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier(Actor, ManaSourceInd);
            }
            else
            {
                yield return Toils_Goto.GotoThing(ManaSourceInd, PathEndMode.ClosestTouch);
                yield return Toils_Ingest.PickupIngestible(ManaSourceInd, Succubus);
            }
            yield return Toils_Goto.GotoThing(SuccubusInd, PathEndMode.Touch);
            yield return Toils_Ingest.ChewIngestible(Succubus, 1.5f, ManaSourceInd, 0);
            yield return Toils_Ingest.FinalizeIngest(Succubus, ManaSourceInd);
        }
    }
}
