using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    public class WorkGiver_NIFeedManaItem : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.ClosestTouch;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn target = t as Pawn;
            if (target == null || target == pawn)
                return false;
            if (!commonUtl.IsSuccubus(target))
                return false;
            if (!FoodUtility.ShouldBeFedBySomeone(target))
                return false;
            Need_Mana need_Mana = target.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
            if (need_Mana == null || need_Mana.CurLevelPercentage > 0.35f)
                return false;
            LocalTargetInfo localTargetInfo = t;
            if (!ReservationUtility.CanReserve(pawn, localTargetInfo, 1, -1, null, forced))
                return false;
            Thing thing;
            ThingDef thingDef;
            if (!commonUtl.TryFindBestManaSourceFor(pawn, target, out thing, out thingDef, true, false, false))
            {
                JobFailReason.Is(Translator.Translate("NItxt_NoManaSource"), null);
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn target = (Pawn)t;
            Thing thing;
            ThingDef thingDef;
            if (commonUtl.TryFindBestManaSourceFor(pawn, target, out thing, out thingDef, true, false, false))
            {
                return new Job(NIJobDefOf.Job_NIFeedManaItem)
                {
                    targetA = thing,
                    targetB = target,
                    count = commonUtl.EstimateIngestCountOf(target, thing)
                };
            }
            return null;
        }
    }
}
