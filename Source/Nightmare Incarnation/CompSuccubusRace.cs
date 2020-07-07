using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
	/// <summary>
	/// Comps for Succubi
	/// </summary>
	public class CompSuccubusRace : ThingComp
	{
		public float ManaFallPerTick = 1E-05f;

		public float ManaFallFactor = 1f;

		public float JoyThreshold = 0.6f;

		public float JoyTolerancesFactor = 1f;

		public WorkPlanStack policy;

		public float TotalManaGained;

		public RelationType tmpTargetType;

		public bool tmpJoyMode;

		public const float JoyModeSqueezeAmount = 0.2f;

		public CompProperties_SuccubusRace Props
		{
			get
			{
				return (CompProperties_SuccubusRace)this.props;
			}
		}

		public Pawn Pawn
		{
			get
			{
				return this.parent as Pawn;
			}
		}

		/// <summary>
		/// Que?
		/// </summary>
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<float>(ref this.TotalManaGained, "TotalManaGained", 0f, false);
			Scribe_Deep.Look<WorkPlanStack>(ref this.policy, "ManaSqueezePolicy", new object[]
			{
				this
			});
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.policy = new WorkPlanStack(this);
		}

		/// <summary>
		/// adds settings to the succubus, I think
		/// </summary>
		/// <param name="respawningAfterLoad"></param>
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			switch (this.Pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute))
			{
				case 1:
					this.ManaFallFactor = 0.5f;
					this.JoyThreshold = 0.4f;
					break;
				case 2:
					this.ManaFallFactor = 1f;
					break;
				case 3:
					this.ManaFallFactor = 1f;
					this.JoyThreshold = 0.8f;
					this.JoyTolerancesFactor = 1.25f;
					break;
				case 4:
					this.ManaFallFactor = 2f;
					this.JoyThreshold = 0.8f;
					break;
				case 5:
					this.ManaFallFactor = 2f;
					this.JoyThreshold = 0.8f;
					this.JoyTolerancesFactor = 0.5f;
					break;
				case 6:
					this.ManaFallFactor = 5f;
					break;
			}
			if (this.Pawn.story.traits.HasTrait(TraitDefOf.Ascetic))
			{
				this.JoyThreshold = 0f;
			}
			this.InitManaSqueezePolicy(false);
		}

		/// <summary>
		/// sets how the succubus is gonna handling being "thirsty"
		/// </summary>
		/// <param name="forcedInit"></param>
		public void InitManaSqueezePolicy(bool forcedInit = false)
		{
			if (forcedInit || !GenCollection.Any<WorkPlan>(policy.plans))
			{
				this.policy.plans.Clear();
				int degreeOfManaAttribute = this.Pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute);
				WorkPlan useManaItem = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Meal
				};
				WorkPlan useColonist = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Colonist,
					AllowJoy = true
				};
				WorkPlan usePrisoner = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Prisoner,
					AllowJoy = true,
					AllowOverSqueeze = true
				};
				WorkPlan useEnemy = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Hostile,
					AllowJoy = true,
					AllowOverSqueeze = true,
					AllowDown = true,
					AllowKill = true,
					SqueezeValue = 2.1f
				};
				WorkPlan useOther = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Other,
					AllowJoy = true
				};
				switch (degreeOfManaAttribute)
				{
					case 1:
						useColonist.AllowJoy = false;
						useColonist.SqueezeValue = 0.7f;
						usePrisoner.AllowJoy = false;
						usePrisoner.SqueezeValue = 0.7f;
						useEnemy.AllowJoy = false;
						useEnemy.AllowKill = false;
						useEnemy.SqueezeValue = 1f;
						useOther.AllowJoy = false;
						useOther.SqueezeValue = 0.7f;
						break;
					case 2:
						break;
					case 3:
						break;
					case 4:
						usePrisoner.SqueezeValue = 1.2f;
						break;
					case 5:
						useColonist.AllowOverSqueeze = true;
						usePrisoner.AllowDown = true;
						usePrisoner.AllowKill = true;
						usePrisoner.SqueezeValue = 2.1f;
						useOther.AllowOverSqueeze = true;
						useOther.SqueezeValue = 1.5f;
						break;
					case 6:
						useColonist.AllowOverSqueeze = true;
						usePrisoner.AllowDown = true;
						usePrisoner.AllowKill = true;
						usePrisoner.SqueezeValue = 2.1f;
						useOther.AllowOverSqueeze = true;
						useOther.SqueezeValue = 1.5f;
						break;
					default:
						break;
				}
				this.policy.plans = new List<WorkPlan>
					{
						useManaItem,
						useEnemy,
						usePrisoner,
						useColonist,
						useOther
					};
			}
		}
	}
}
