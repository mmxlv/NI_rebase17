using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    public enum ManaStages
    {
        Safe,
        Tired,
        Fatigue,
        Overworked,
        Extreme,
        Deadly,
    }

    public static class ManaUtl
    {
        public static bool CanBeSqueezed(Pawn pawn, WorkPlan workPlan, float forcedValue = 0f)
        {
            float squuezeAmount = (forcedValue > 0f) ? forcedValue : workPlan.SqueezeValue;
            if (workPlan.AllowOverSqueeze)
            {
                if (workPlan.AllowDown)
                {
                    if (workPlan.AllowKill)
                        return true;
                    else
                        return GetSeverity(pawn) + squuezeAmount < GetThresholdFromStage(ManaStages.Deadly);
                }
                else
                    return GetSeverity(pawn) + squuezeAmount < GetThresholdFromStage(ManaStages.Extreme) && !pawn.Downed && !pawn.health.HasHediffsNeedingTendByPlayer();
            }
            else
            {
                if (workPlan.AllowDown)
                {
                    return GetSeverity(pawn) + squuezeAmount < GetThresholdFromStage(ManaStages.Tired);
                }
                else
                    return GetSeverity(pawn) + squuezeAmount < GetThresholdFromStage(ManaStages.Tired) && !pawn.Downed && !pawn.health.HasHediffsNeedingTendByPlayer();
            }
        }
        public static float GetSeverity(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(NIHediffDefOf.VigorRavage) is Hediff hediff)
            {
                return hediff.Severity;
            }
            return 0f;
        }
        public static float GetThresholdFromStage(ManaStages manaStages)
        {
            return NIHediffDefOf.VigorRavage.stages[(int)manaStages].minSeverity;
        }
        public static List<Pawn> GetPawnsByManaPolicy(Pawn pawn, WorkPlan workPlan, float forcedValue = 0f)
        {
            RelationType targetType = workPlan.targetType;
            List<Pawn> result;
            switch (targetType)
            {
                case RelationType.Colonist:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.Where(colonist => colonist.IsFreeColonist
                                                                                   && colonist != pawn
                                                                                   && !CommonUtl.IsSuccubus(colonist)
                                                                                   && CommonUtl.IsPassive(colonist)
                                                                                   && CanBeSqueezed(colonist, workPlan, forcedValue)
                                                                                   && ReservationUtility.CanReserveAndReach(pawn, colonist, PathEndMode.OnCell, Danger.Deadly)).ToList();
                    break;
                case RelationType.Slave:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.Where(slave => slave.IsSlaveOfColony
                                                                                 && slave != pawn
                                                                                 && !CommonUtl.IsSuccubus(slave)
                                                                                 && CommonUtl.IsPassive(slave)
                                                                                 && CanBeSqueezed(slave, workPlan, forcedValue)
                                                                                 && ReservationUtility.CanReserveAndReach(pawn, slave, PathEndMode.OnCell, Danger.Deadly)).ToList();
                    break;
                case RelationType.Prisoner:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.Where(prisoner => prisoner.IsPrisonerOfColony
                                                                                   && prisoner != pawn
                                                                                   && !CommonUtl.IsSuccubus(prisoner)
                                                                                   && CommonUtl.IsPassive(prisoner)
                                                                                   && CanBeSqueezed(prisoner, workPlan, forcedValue)
                                                                                   && ReservationUtility.CanReserveAndReach(pawn, prisoner, PathEndMode.OnCell, Danger.Deadly)).ToList();
                    break;
                case RelationType.Hostile:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.Where(hostile => hostile.HostileTo(Faction.OfPlayer)
                                                                                  && !hostile.IsPrisonerOfColony
                                                                                  && hostile != pawn
                                                                                  && !CommonUtl.IsSuccubus(hostile)
                                                                                  && CommonUtl.IsPassive(hostile)
                                                                                  && CanBeSqueezed(hostile, workPlan, forcedValue)
                                                                                  && ReservationUtility.CanReserveAndReach(pawn, hostile, PathEndMode.OnCell, Danger.Deadly)).ToList();
                    break;
                default:
                    result = pawn.Map.mapPawns.AllPawnsSpawned.Where(other => other.Faction != Faction.OfPlayer
                                                                                && (other.Faction == null
                                                                                    || (other.Faction != null && FactionUtility.HostileTo(other.Faction, Faction.OfPlayer)))
                                                                                && !other.IsPrisonerOfColony
                                                                                && other != pawn
                                                                                && !CommonUtl.IsSuccubus(other)
                                                                                && CommonUtl.IsPassive(other)
                                                                                && CanBeSqueezed(other, workPlan, forcedValue)
                                                                                && ReservationUtility.CanReserveAndReach(pawn, other, PathEndMode.OnCell, Danger.Deadly)).ToList();
                    break;
            }
            return result;
        }
    }
}
