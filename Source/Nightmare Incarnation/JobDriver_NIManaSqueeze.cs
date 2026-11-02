using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NightmareIncarnation
{
    /// <summary>
    /// ManaSqueeze job need to really understand this so I can turn it into a RJW action
    /// </summary>
    //TODO Create an enum for the forcedThoughstages
    public class JobDriver_NIManaSqueeze : JobDriver
    {
        private const TargetIndex VictimInd = TargetIndex.A;

        private bool JoyMode;

        private WorkPlan workPlan;

        private float SqueezeAmount;

        private float SqueezeProgress;

        private int moteInterval = 90;

        private int motethrownTick = -1;

        private float totalGainMana;

        private float manaGainPerTick;

        private float joyGainPerTick;

        private float joyGainPerTickOffset;

        private PawnRelationDef[] RomanticRelationshipDefs = new[] { PawnRelationDefOf.Lover, PawnRelationDefOf.Fiance, PawnRelationDefOf.Spouse };

        protected Pawn Actor => GetActor();

        protected Pawn Victim => job.GetTarget(VictimInd).Pawn;

        protected CompSuccubusRace CompRace => Actor.GetComp<CompSuccubusRace>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.JoyMode, "JoyMode", false, false);
            Scribe_References.Look<WorkPlan>(ref this.workPlan, "workPlan", false);
            Scribe_Values.Look<float>(ref this.SqueezeProgress, "SqueezeProgress", 0f, false);
            Scribe_Values.Look<float>(ref this.totalGainMana, "totalGainMana", 0f, false);
            Scribe_Values.Look<float>(ref this.manaGainPerTick, "manaGainPerTick", 0.001f, false);
            Scribe_Values.Look<float>(ref this.joyGainPerTick, "joyGainPerTick", 0.001f, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!Actor.Reserve(Victim, job, 1, -1, null, errorOnFailed))
                return false;

            if (CompRace != null)
            {
                this.JoyMode = CompRace.tmpJoyMode;
                this.workPlan = CompRace.policy.plans.First(plan => plan.targetType == CompRace.tmpTargetType);
                int traitDegree = Actor.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute);
                bool forced = ManaUtl.CanBeSqueezed(Victim, workPlan, 0f)
                              && (Actor.story.traits.HasTrait(TraitDefOf.Bloodlust)
                                  || Actor.story.traits.HasTrait(TraitDefOf.Psychopath)
                                  || traitDegree == 5
                                  || traitDegree == 6);
                this.SqueezeAmount = (JoyMode && !forced) ? 0.2f : workPlan.SqueezeValue;
                this.manaGainPerTick = (JoyMode ? 0.8f : 1f) * SqueezeAmount / job.def.joyDuration;
                this.joyGainPerTick = 0.25f * CompRace.JoyTolerancesFactor / job.def.joyDuration;
                this.joyGainPerTickOffset = 0.25f / (CompRace.JoyTolerancesFactor * job.def.joyDuration);
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Need_Mana succubusManaNeed = Actor.needs.TryGetNeed<Need_Mana>();
            //Need_Mana victimManaNeed = Victim.needs.TryGetNeed<Need_Mana>();
            this.FailOnDespawnedNullOrForbidden(VictimInd);
            this.FailOnAggroMentalState(VictimInd);
            this.FailOn(() =>
            {
                return !CommonUtl.IsPassive(Victim)
                       || workPlan.Suspended
                       || (JoyMode && !workPlan.AllowJoy)
                       || (!workPlan.AllowOverSqueeze
                           && ManaUtl.GetSeverity(Victim) >= ManaUtl.GetThresholdFromStage(ManaStages.Tired))
                       || (!workPlan.AllowDown
                           && (Victim.Downed
                               || Victim.health.HasHediffsNeedingTendByPlayer(false)
                               || ManaUtl.GetSeverity(Victim) >= ManaUtl.GetThresholdFromStage(ManaStages.Extreme)))
                       || (!workPlan.AllowKill
                           && ManaUtl.GetSeverity(Victim) >= ManaUtl.GetThresholdFromStage(ManaStages.Deadly));
            });
            AddFinishAction(condition =>
            {
                if (Victim.CurJobDef == JobDefOf.Wait_MaintainPosture)
                {
                    Victim.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
                if (totalGainMana > 0f)
                {
                    ApplyJobThought(pawn, Victim);
                    CompRace.RecordSqueezedVictim();
                    CompRace.TotalManaGained = Mathf.Min(CompRace.TotalManaGained + Mathf.RoundToInt(totalGainMana / 0.01f) * 0.01f, 9999f);
                }
            });

            yield return Toils_Goto.GotoThing(VictimInd, PathEndMode.OnCell);

            Toil work = new Toil
            {
                initAction = delegate
                {
                    Actor.pather.StopDead();
                    PawnUtility.ForceWait(Victim, job.def.joyDuration, null, true);
                },
                tickAction = delegate
                {
                    totalGainMana += manaGainPerTick;
                    succubusManaNeed.CurLevel += manaGainPerTick;

                    HealthUtility.AdjustSeverity(Victim, NIHediffDefOf.VigorRavage, manaGainPerTick);

                    int ticksGame = Find.TickManager.TicksGame;
                    if (ticksGame > moteInterval + motethrownTick)
                    {
                        motethrownTick = ticksGame;
                        FleckMaker.ThrowMetaIcon(Victim.Position, pawn.Map, FleckDefOf.Heart);
                    }

                    if (this.JoyMode)
                    {
                        Actor.skills.Learn(SkillDefOf.Social, 0.02f, false);
                        Actor.needs.joy.GainJoy(joyGainPerTick, NIJoyKindDefOf.JoyKind_NISuccubusWork);
                        Actor.needs.joy.CurLevel += joyGainPerTickOffset;
                        if (Actor.needs.joy.CurLevelPercentage >= 1f)
                        {
                            Actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                        }
                    }
                    else
                    {
                        Actor.skills.Learn(SkillDefOf.Social, 0.01f, false);
                    }

                    if (totalGainMana > SqueezeAmount)
                    {
                        if (!JoyMode)
                        {
                            Actor.needs.joy.CurLevel += 0.1f;
                        }
                        Actor.needs.food.CurLevel += 0.05f;
                        Actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never,
            };
            work.PlaySustainerOrSound(NISoundDefOf.Recipe_Surgery);
            work.WithProgressBar(VictimInd, new Func<float>(this.GetProgress));
            work.FailOnCannotTouch(VictimInd, PathEndMode.OnCell);
            work.activeSkill = () => SkillDefOf.Social;
            yield return work;
        }

        public override string GetReport()
        {
            if (JoyMode)
            {
                return TranslatorFormattedStringExtensions.Translate("NItxt_Teasing", this.Victim);
            }
            return base.GetReport();
        }

        private void ApplyJobThought(Pawn succubus, Pawn victim)
        {
            IEnumerable<PawnRelationDef> relations = PawnRelationUtility.GetRelations(succubus, victim);
            bool inRomanticRelationship = relations.Intersect(RomanticRelationshipDefs).Any();
            if (!victim.Dead)
            {
                if (inRomanticRelationship)
                {
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, succubus, victim, 4);
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterOpLover, succubus, victim, 0);
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodLover, victim, succubus, 0);
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpLover, victim, succubus, 0);
                }
                else
                {
                    ApplyThoughtsForLiving(succubus, victim);

                    //if the target has a lover that is not the succ, add thoughts about the succ to the lover
                    Pawn romanticPartnerOfTarget = GetLover(victim);
                    if (romanticPartnerOfTarget != null && romanticPartnerOfTarget != succubus)
                    {
                        if (romanticPartnerOfTarget.story != null && romanticPartnerOfTarget.story.traits.HasTrait(NITraitDefOf.Jealous))
                        {
                            AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodJealousy, romanticPartnerOfTarget, succubus, 1);
                        }
                        else
                        {
                            AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodJealousy, romanticPartnerOfTarget, succubus, 0);
                        }
                        AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpJealousy, romanticPartnerOfTarget, succubus, 0);
                    }
                }
            }
            else
            {
                if (inRomanticRelationship)
                {
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillLover, succubus, victim, 0);
                }
                else
                {
                    Pawn_StoryTracker succubusStory = succubus.story;
                    if (succubusStory.traits.HasTrait(TraitDefOf.Kind) || succubusStory.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 1)
                        AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillKind, succubus, victim, 0);
                    if (succubusStory.traits.HasTrait(TraitDefOf.Bloodlust) || succubusStory.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 5)
                    {
                        AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillEvil, succubus, victim, 0);
                    }
                    else if (!succubusStory.traits.HasTrait(TraitDefOf.Psychopath))
                        AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKill, succubus, victim, 0);
                }
                if (victim.IsColonist)
                {
                    List<Pawn> colonists = (from x in Find.WorldPawns.AllPawnsAlive where x.IsColonist && x != succubus select x).ToList();
                    foreach (Map map in Find.Maps)
                    {
                        IEnumerable<Pawn> allPawns = map.mapPawns.AllPawns;
                        colonists.AddRange(allPawns.Where(x => !x.Dead && x.IsColonist && x != succubus));
                    }
                    foreach (Pawn pawn in colonists)
                    {
                        AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledColonist, pawn, succubus, 0);
                    }
                }
                else
                {
                    if (victim.IsPrisoner)
                    {
                        List<Pawn> allPawnsExceptSuc = (from x in Find.WorldPawns.AllPawnsAlive
                                                        where (x.IsColonist || x.IsPrisoner) && x != succubus
                                                        select x).ToList<Pawn>();
                        foreach (Map map2 in Find.Maps)
                        {
                            IEnumerable<Pawn> allPawns2 = map2.mapPawns.AllPawns;
                            allPawnsExceptSuc.AddRange(allPawns2.Where(x => !x.Dead && (x.IsColonist || x.IsPrisoner) && x != succubus));
                        }
                        foreach (Pawn pawn in allPawnsExceptSuc)
                        {
                            AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledPrisoner, pawn, succubus, 0);
                        }
                    }
                }
                Pawn romanticPartnerOfTarget = GetLover(victim);
                if (romanticPartnerOfTarget != null && romanticPartnerOfTarget != succubus)
                {
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledLover, romanticPartnerOfTarget, succubus, 0);
                }
            }
        }

        private void ApplyThoughtsForLiving(Pawn pawn, Pawn target)
        {
            Pawn_StoryTracker succubusStory = pawn.story;
            Pawn_StoryTracker targetStory = target.story;
            if (succubusStory != null && targetStory != null)
            {
                if (target.gender == pawn.gender)
                {
                    if (succubusStory.traits.HasTrait(TraitDefOf.Gay))
                        AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 3);
                    if (targetStory.traits.HasTrait(TraitDefOf.Gay))
                    {
                        AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodLes, target, pawn, 0);
                        AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpDesire, target, pawn, 0);
                    }

                }
                //Succubus thoughts
                if (succubusStory.traits.HasTrait(TraitDefOf.Kind) || succubusStory.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 1 || targetStory.traits.HasTrait(TraitDefOf.Ascetic))
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 5);
                if (succubusStory.traits.HasTrait(TraitDefOf.DislikesMen) && target.gender == Gender.Male)
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 2);
                if (succubusStory.traits.HasTrait(TraitDefOf.DislikesWomen) && target.gender == Gender.Female)
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 1);
                if (succubusStory.traits.HasTrait(TraitDefOf.Bloodlust) || succubusStory.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 5)
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 0);

                //Target thoughts
                if (targetStory.traits.HasTrait(TraitDefOf.Ascetic))
                {
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodAscetic, target, pawn, 0);
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
                }
                if (targetStory.traits.HasTrait(TraitDefOf.DislikesMen) && pawn.gender == Gender.Male)
                {
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodDislikeMen, target, pawn, 0);
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
                }
                if (targetStory.traits.HasTrait(NITraitDefOf.Masochist))
                {
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodMaso, target, pawn, 0);
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpDesire, target, pawn, 0);
                }
                if (targetStory.traits.HasTrait(TraitDefOf.DislikesWomen) && pawn.gender == Gender.Female)
                {
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodDislikeFem, target, pawn, 0);
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
                }
                else
                {
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMood, target, pawn, 0);
                    AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
                }
            }
        }


        private static void AddSucJobThought(ThoughtDef thoughtDef, Pawn affected, Pawn affecter, int forcedStage = 0)
        {
            float statValue = StatExtension.GetStatValue(affecter, StatDefOf.SocialImpact, true);
            Thought_Memory thought_Memory = ThoughtMaker.MakeThought(thoughtDef, forcedStage);
            bool basicSuccubus = affecter.story != null && affecter.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 1;
            if (basicSuccubus)
            {
                thought_Memory.moodPowerFactor = ((thought_Memory.MoodOffset() < 0f) ? 0.5f : 1f);
            }
            if (thought_Memory is Thought_MemorySocial thought_MemorySocial && CommonUtl.IsSuccubus(affecter))
            {
                if (thought_MemorySocial.opinionOffset < 0f)
                {
                    thought_MemorySocial.opinionOffset /= Mathf.Clamp(statValue, 1f, 2f);
                    if (basicSuccubus)
                    {
                        thought_MemorySocial.opinionOffset *= 0.5f;
                    }
                }
                else
                {
                    thought_MemorySocial.opinionOffset *= Mathf.Clamp(statValue, 1f, 2f);
                }
            }
            affected.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, affecter);
        }

        private Pawn GetLover(Pawn pawn) => LovePartnerRelationUtility.ExistingMostLikedLovePartner(pawn, false);

        private float GetProgress() => this.totalGainMana / this.SqueezeAmount;
    }
}
