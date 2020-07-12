using System;
using RimWorld;
using UnityEngine.Assertions.Must;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    /// <summary>
    /// Create works for pawns to feed succubi who can't eat on their own. Someone have to feed them.
    /// </summary>
    public class WorkGiver_NIFeedManaItem : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn succubus)
                || succubus == pawn
                || !commonUtl.IsSuccubus(succubus)
                || !FoodUtility.ShouldBeFedBySomeone(succubus))
            {
                return false;
            }

            Need_Mana needMana = succubus.needs.TryGetNeed<Need_Mana>();
            if (needMana == null || needMana.CurLevelPercentage > 0.35f)
                return false;

            if (!pawn.CanReserve(succubus, 1, -1, null, forced))
                return false;

            if (!commonUtl.TryFindBestManaSourceFor(pawn, succubus, out _, out _))
            {
                JobFailReason.Is(Translator.Translate("NItxt_NoManaSource"), null);
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn succubus = (Pawn)t;
            if (commonUtl.TryFindBestManaSourceFor(pawn, succubus, out Thing manaSource, out _))
            {
                return new Job(NIJobDefOf.Job_NIFeedManaItem)
                {
                    targetA = manaSource,
                    targetB = succubus,
                    count = commonUtl.EstimateIngestCountOf(succubus, manaSource)
                };
            }
            return null;
        }
    }
}
