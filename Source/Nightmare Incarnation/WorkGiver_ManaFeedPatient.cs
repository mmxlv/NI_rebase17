using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
	public class WorkGiver_ManaFeedPatient : WorkGiver_Scanner
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
			if (commonUtl.IsSuccubus(target))
				return false;
			if (this.def.feedHumanlikesOnly && !target.RaceProps.Humanlike)
				return false;
			if (this.def.feedAnimalsOnly && !target.RaceProps.Animal)
				return false;
			Need_Mana need_Mana = target.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
			if (need_Mana == null || need_Mana.CurLevel > 0f)
				return false;
			if (!FeedPatientUtility.ShouldBeFed(target) || need_Mana.CurLevel > 0f)
				return false;
			LocalTargetInfo localTargetInfo = t;
			if (!ReservationUtility.CanReserve(pawn, localTargetInfo, 1, -1, null, forced))
				return false;
			Thing thing;
			ThingDef thingDef;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, target, target.needs.food.CurCategory == HungerCategory.Starving, out thing, out thingDef, false, true, false, true, false, false, false))
			{
				JobFailReason.Is(Translator.Translate("NoFood"), null);
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn target = (Pawn)t;
			Thing thing;
			ThingDef thingDef;
			if (FoodUtility.TryFindBestFoodSourceFor(pawn, target, target.needs.food.CurCategory == HungerCategory.Starving, out thing, out thingDef, false, true, false, true, false, false, false))
			{
				float nutrition = FoodUtility.GetNutrition(thing, thingDef);
				return new Job(JobDefOf.FeedPatient)
				{
					targetA = thing,
					targetB = target,
					count = FoodUtility.WillIngestStackCountOf(target, thingDef, nutrition)
				};
			}
			return null;
		}
	}
}
