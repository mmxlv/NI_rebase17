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

        protected Pawn Victim
        {
            get
            {
                return (Pawn)this.job.targetA.Thing;
            }
        }

        protected CompSuccubusRace CompRace
        {
            get
            {
                return this.pawn.GetComp<CompSuccubusRace>();
            }
        }

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
            Pawn pawn = this.pawn;
            LocalTargetInfo localTargetInfo = this.Victim;
            Job job = this.job;
            bool canReserveToil = ReservationUtility.Reserve(pawn, localTargetInfo, job, 1, -1, null, errorOnFailed);
            if (!canReserveToil)
                return false;
            
            CompSuccubusRace compRace;
            bool isSuccubus = (compRace = pawn.GetComp<CompSuccubusRace>()) != null;
            if (isSuccubus)
            {
                this.JoyMode = compRace.tmpJoyMode;
                this.workPlan = compRace.policy.plans.Find((WorkPlan x) => x.targetType == compRace.tmpTargetType);
                Pawn_StoryTracker story = pawn.story;
                int? num = (story != null) ? new int?(story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute)) : null;
                bool flag3 = (pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) || pawn.story.traits.HasTrait(TraitDefOf.Psychopath) || (num != null && (num == 5 || num == 6))) && commonUtl.HasEnoughtMana(this.Victim, this.workPlan, 0f);
                this.SqueezeAmount = ((this.JoyMode && !flag3) ? 0.2f : this.workPlan.SqueezeValue);
                this.manaGainPerTick = (this.JoyMode ? 0.8f : 1f) * this.SqueezeAmount / (float)this.job.def.joyDuration;
                this.joyGainPerTick = 0.25f * compRace.JoyTolerancesFactor / (float)this.job.def.joyDuration;
                this.joyGainPerTickOffset = 0.25f / (compRace.JoyTolerancesFactor * (float)this.job.def.joyDuration);
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Need_Mana succubusManaNeed = this.pawn.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
            Need_Mana victimManaNeed = this.Victim.needs.AllNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana) as Need_Mana;
            ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_NIManaSqueeze>(this,TargetIndex.A);
            ToilFailConditions.FailOn<JobDriver_NIManaSqueeze>(this, () => !commonUtl.IsPassive(this.Victim) || this.workPlan.Suspended || (this.JoyMode && !this.workPlan.AllowJoy) || (!this.workPlan.AllowOverSqueeze && commonUtl.GetMana(this.Victim) <= 0f) || (!this.workPlan.AllowDown && (this.Victim.Downed || this.Victim.health.HasHediffsNeedingTendByPlayer(false) || commonUtl.GetNetMana(this.Victim) < -0.79f)) || (!this.workPlan.AllowKill && commonUtl.GetNetMana(this.Victim) < -0.95f));
            ToilFailConditions.FailOnAggroMentalState<JobDriver_NIManaSqueeze>(this, TargetIndex.A);
            base.AddFinishAction(delegate()
            {
                bool victimWaiting = this.Victim != this.pawn && this.Victim.CurJobDef == JobDefOf.Wait_MaintainPosture;
                if (victimWaiting)
                {
                    this.Victim.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
                if (this.totalGainMana > 0f)
                {
                    this.ApplyJobThought(this.pawn, this.Victim);
                    this.CompRace.TotalManaGained = Mathf.Min(this.CompRace.TotalManaGained + (float)Mathf.RoundToInt(this.totalGainMana / 0.01f) * 0.01f, 9999f);
                }
            });

            PathEndMode interactionCell =PathEndMode.OnCell;
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, interactionCell);
            yield return gotoToil;
            yield return Toils_General.Do(delegate()
            {
            });
            Toil work = new Toil();
            work.initAction = delegate()
            {
                Pawn actor = work.actor;
                Pawn pawn = (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
                actor.pather.StopDead();
                PawnUtility.ForceWait(pawn, this.job.def.joyDuration, null, true);
            };
            work.tickAction = delegate()
            {
                this.totalGainMana += this.manaGainPerTick;
                succubusManaNeed.CurLevel += this.manaGainPerTick;
                float drainedMana = victimManaNeed.CurLevel - this.manaGainPerTick;
                victimManaNeed.CurLevel = ((drainedMana < 0f) ? 0f : drainedMana);
                if (drainedMana < 0f)
                {
                    HealthUtility.AdjustSeverity(this.Victim, NIHediffDefOf.VigorRavage, -drainedMana);
                }
                int ticksGame = Find.TickManager.TicksGame;
                bool throwHeartParticles = ticksGame > this.moteInterval + this.motethrownTick;
                if (throwHeartParticles)
                {
                    this.motethrownTick = ticksGame;
                    MoteMaker.ThrowMetaIcon(this.Victim.Position, this.pawn.Map, ThingDefOf.Mote_Heart);
                }
                if (this.JoyMode)
                {
                    this.pawn.skills.Learn(SkillDefOf.Social, 0.02f, false);
                    this.pawn.needs.joy.GainJoy(this.joyGainPerTick, NIJoyKindDefOf.JoyKind_NISuccubusWork);
                    this.pawn.needs.joy.CurLevel += this.joyGainPerTickOffset;
                    bool joyneedFullfilled = this.pawn.needs.joy.CurLevelPercentage >= 1f;
                    if (joyneedFullfilled)
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    }
                }
                else
                {
                    this.pawn.skills.Learn(SkillDefOf.Social, 0.01f, false);
                }
                if (this.totalGainMana > this.SqueezeAmount)
                {
                    if (!this.JoyMode)
                    {
                        this.pawn.needs.joy.CurLevel += 0.1f;
                    }
                    this.pawn.needs.food.CurLevel += 0.05f;
                    this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                }
            };
            work.defaultCompleteMode =ToilCompleteMode.Never;
            ToilEffects.PlaySustainerOrSound(ToilEffects.WithProgressBar(ToilFailConditions.FailOnCannotTouch<Toil>(work, TargetIndex.A, interactionCell), TargetIndex.A, new Func<float>(this.GetProgress), false, -0.5f), NISoundDefOf.Recipe_Surgery);
            work.activeSkill = (() => SkillDefOf.Social);
            yield return work;
            yield break;
        }

        public override string GetReport()
        {
            if (this.JoyMode)
            {
                return TranslatorFormattedStringExtensions.Translate("NItxt_Teasing", this.Victim);
            }
            return base.GetReport();
        }

        private void ApplyJobThought(Pawn succubus, Pawn target)
        {
            var relations = PawnRelationUtility.GetRelations(succubus, target);
            bool inRomanticRelationship = relations.Intersect(RomanticRelationshipDefs).Any();
            if (!target.Dead) 
            {
                if (inRomanticRelationship)
                {
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, succubus, target, 4);
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterOpLover, succubus, target, 0);
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodLover, target, succubus, 0);
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpLover, target, succubus, 0);
                }
                else
                {    
                    ApplyThoughtsForLiving(succubus, target);

                    //if the target has a lover that is not the succ, add thoughts about the succ to the lover
                    Pawn romanticPartnerOfTarget = this.GetLover(target); 
                    if (romanticPartnerOfTarget != null && romanticPartnerOfTarget != succubus)
                    {
                        if (romanticPartnerOfTarget.story != null && romanticPartnerOfTarget.story.traits.HasTrait(NITraitDefOf.Jealous))
                        {
                            JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodJealousy, romanticPartnerOfTarget, succubus, 1);
                        }
                        else
                        {
                            JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodJealousy, romanticPartnerOfTarget, succubus, 0);
                        }
                        JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpJealousy, romanticPartnerOfTarget, succubus, 0);
                    }
                }
            }
            else
            {
                if (inRomanticRelationship)
                {
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillLover, succubus, target, 0);
                }
                else
                {
                    Pawn_StoryTracker succubusStory = succubus.story;
                    if (succubusStory.traits.HasTrait(TraitDefOf.Kind) || succubusStory.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 1)
                        JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillKind, succubus, target, 0);
                    if (succubusStory.traits.HasTrait(TraitDefOf.Bloodlust) || succubusStory.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 5)
                    {
                        JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKillEvil, succubus, target, 0);
                    }
                    else if(!succubusStory.traits.HasTrait(TraitDefOf.Psychopath))
                        JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMoodKill, succubus, target, 0);
                }
                if (target.IsColonist)
                {
                    List<Pawn> colonists = (from x in Find.WorldPawns.AllPawnsAlive where x.IsColonist && x != succubus select x).ToList<Pawn>();
                    Func<Pawn, bool> predicate = (Pawn x) => !x.Dead && x.IsColonist && x != succubus;
                    foreach (Map map in Find.Maps)
                    {
                        IEnumerable<Pawn> allPawns = map.mapPawns.AllPawns;
                        colonists.AddRange(allPawns.Where(predicate));
                    }
                    foreach (Pawn pawn in colonists)
                    {
                        JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledColonist, pawn, succubus, 0);
                    }
                }
                else
                {
                    if (target.IsPrisoner)
                    {
                        List<Pawn> allPawnsExceptSuc = (from x in Find.WorldPawns.AllPawnsAlive
                                            where (x.IsColonist || x.IsPrisoner) && x != succubus
                                            select x).ToList<Pawn>();
                        Func<Pawn, bool> predicate2 = (Pawn x) => !x.Dead && (x.IsColonist || x.IsPrisoner) && x != succubus;
                        foreach (Map map2 in Find.Maps)
                        {
                            IEnumerable<Pawn> allPawns2 = map2.mapPawns.AllPawns;
                            allPawnsExceptSuc.AddRange(allPawns2.Where(predicate2));
                        }
                        foreach (Pawn pawn in allPawnsExceptSuc)
                        {
                            JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledPrisoner, pawn, succubus, 0);
                        }
                    }
                }
                Pawn romanticPartnerOfTarget = this.GetLover(target);
                if (romanticPartnerOfTarget != null && romanticPartnerOfTarget != succubus)
                {
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpKilledLover, romanticPartnerOfTarget, succubus, 0);
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
                        JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 3);
                    if (targetStory.traits.HasTrait(TraitDefOf.Gay))
                    {
                        JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodLes, target, pawn, 0);
                        JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpDesire, target, pawn, 0);
                    }

                }
                //Succubus thoughts
                if (succubusStory.traits.HasTrait(TraitDefOf.Kind) || succubusStory.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 1 || targetStory.traits.HasTrait(TraitDefOf.Ascetic))
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 5);
                if (succubusStory.traits.HasTrait(TraitDefOf.DislikesMen) && target.gender == Gender.Male)
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 2);
                if (succubusStory.traits.HasTrait(TraitDefOf.DislikesWomen) && target.gender == Gender.Female)
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 1);
                if (succubusStory.traits.HasTrait(TraitDefOf.Bloodlust) || succubusStory.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute) == 5)
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracterMood, pawn, target, 0);

                //Target thoughts
                if (targetStory.traits.HasTrait(TraitDefOf.Ascetic))
                {
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodAscetic, target, pawn, 0);
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
                }
                if (targetStory.traits.HasTrait(TraitDefOf.DislikesMen) && pawn.gender == Gender.Male)
                {
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodDislikeMen, target, pawn, 0);
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
                }
                if (targetStory.traits.HasTrait(NITraitDefOf.Masochist))
                {
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodMaso, target, pawn, 0);
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOpDesire, target, pawn, 0);
                }
                if (targetStory.traits.HasTrait(TraitDefOf.DislikesWomen) && pawn.gender == Gender.Female)
                {
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMoodDislikeFem, target, pawn, 0);
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
                }
                else
                {
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeMood, target, pawn, 0);
                    JobDriver_NIManaSqueeze.AddSucJobThought(NIThoughtDefOf.NI_SuccubusInteracteeOp, target, pawn, 0);
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
            Thought_MemorySocial thought_MemorySocial = thought_Memory as Thought_MemorySocial;
            if (thought_MemorySocial != null && commonUtl.IsSuccubus(affecter))
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

        private Pawn GetLover(Pawn pawn)
        {
            return LovePartnerRelationUtility.ExistingMostLikedLovePartner(pawn, false);
        }

        private float GetProgress()
        {
            return this.totalGainMana / this.SqueezeAmount;
        }


    }
}
