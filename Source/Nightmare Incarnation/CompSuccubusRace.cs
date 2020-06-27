using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x0200001E RID: 30
	public class CompSuccubusRace : ThingComp
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000078 RID: 120 RVA: 0x000068EC File Offset: 0x00004AEC
		public CompProperties_SuccubusRace Props
		{
			get
			{
				return (CompProperties_SuccubusRace)this.props;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000079 RID: 121 RVA: 0x0000690C File Offset: 0x00004B0C
		public Pawn Pawn
		{
			get
			{
				return this.parent as Pawn;
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00006929 File Offset: 0x00004B29
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<float>(ref this.TotalManaGained, "TotalManaGained", 0f, false);
			Scribe_Deep.Look<WorkPlanStack>(ref this.policy, "ManaSqueezePolicy", new object[]
			{
				this
			});
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00006965 File Offset: 0x00004B65
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.policy = new WorkPlanStack(this);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x0000697C File Offset: 0x00004B7C
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
			bool flag = this.Pawn.story != null && this.Pawn.story.traits.HasTrait(TraitDefOf.Ascetic);
			if (flag)
			{
				this.JoyThreshold = 0f;
			}
			this.InitManaSqueezePolicy(false);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00006AA8 File Offset: 0x00004CA8
		public void InitManaSqueezePolicy(bool forcedInit = false)
		{
			if (forcedInit || !GenCollection.Any<WorkPlan>(policy.plans))
			{
				this.policy.plans.Clear();
				int num = this.Pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute);
				WorkPlan item = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Meal
				};
				WorkPlan workPlan = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Colonist,
					AllowJoy = true
				};
				WorkPlan workPlan2 = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Prisoner,
					AllowJoy = true,
					AllowOverSqueeze = true
				};
				WorkPlan workPlan3 = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Hostile,
					AllowJoy = true,
					AllowOverSqueeze = true,
					AllowDown = true,
					AllowKill = true,
					SqueezeValue = 2.1f
				};
				WorkPlan workPlan4 = new WorkPlan
				{
					workPlanStack = this.policy,
					targetType = RelationType.Other,
					AllowJoy = true
				};
				switch (num)
				{
				case 1:
					workPlan.AllowJoy = false;
					workPlan.SqueezeValue = 0.7f;
					workPlan2.AllowJoy = false;
					workPlan2.SqueezeValue = 0.7f;
					workPlan3.AllowJoy = false;
					workPlan3.AllowKill = false;
					workPlan3.SqueezeValue = 1f;
					workPlan4.AllowJoy = false;
					workPlan4.SqueezeValue = 0.7f;
					this.policy.plans = new List<WorkPlan>
					{
						item,
						workPlan,
						workPlan2,
						workPlan3,
						workPlan4
					};
					break;
				case 2:
				case 3:
					this.policy.plans = new List<WorkPlan>
					{
						item,
						workPlan3,
						workPlan2,
						workPlan,
						workPlan4
					};
					break;
				case 4:
					workPlan2.SqueezeValue = 1.2f;
					this.policy.plans = new List<WorkPlan>
					{
						item,
						workPlan3,
						workPlan2,
						workPlan,
						workPlan4
					};
					break;
				case 5:
					workPlan.AllowOverSqueeze = true;
					workPlan2.AllowDown = true;
					workPlan2.AllowKill = true;
					workPlan2.SqueezeValue = 2.1f;
					workPlan4.AllowOverSqueeze = true;
					workPlan4.SqueezeValue = 1.5f;
					this.policy.plans = new List<WorkPlan>
					{
						item,
						workPlan3,
						workPlan2,
						workPlan,
						workPlan4
					};
					break;
				case 6:
					workPlan.AllowOverSqueeze = true;
					workPlan2.AllowDown = true;
					workPlan2.AllowKill = true;
					workPlan2.SqueezeValue = 2.1f;
					workPlan4.AllowOverSqueeze = true;
					workPlan4.SqueezeValue = 1.5f;
					this.policy.plans = new List<WorkPlan>
					{
						item,
						workPlan3,
						workPlan2,
						workPlan,
						workPlan4
					};
					break;
				default:
					this.policy.plans = new List<WorkPlan>
					{
						item,
						workPlan3,
						workPlan2,
						workPlan,
						workPlan4
					};
					break;
				}
			}
		}

		// Token: 0x040000A0 RID: 160
		public float ManaFallPerTick = 1E-05f;

		// Token: 0x040000A1 RID: 161
		public float ManaFallFactor = 1f;

		// Token: 0x040000A2 RID: 162
		public float JoyThreshold = 0.6f;

		// Token: 0x040000A3 RID: 163
		public float JoyTolerancesFactor = 1f;

		// Token: 0x040000A4 RID: 164
		public WorkPlanStack policy;

		// Token: 0x040000A5 RID: 165
		public float TotalManaGained;

		// Token: 0x040000A6 RID: 166
		public RelationType tmpTargetType;

		// Token: 0x040000A7 RID: 167
		public bool tmpJoyMode;

		// Token: 0x040000A8 RID: 168
		public const float JoyModeSqueezeAmount = 0.2f;
	}
}
