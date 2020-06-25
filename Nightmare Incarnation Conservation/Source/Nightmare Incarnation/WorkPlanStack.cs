using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x0200001F RID: 31
	public class WorkPlanStack : IExposable
	{
		// Token: 0x0600007F RID: 127 RVA: 0x00006E6F File Offset: 0x0000506F
		public WorkPlanStack(CompSuccubusRace Comp)
		{
			this.compRace = Comp;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00006E8C File Offset: 0x0000508C
		public void ExposeData()
		{
			Scribe_Collections.Look<WorkPlan>(ref this.plans, "plans", 2, new object[0]);
			bool flag = Scribe.mode == 3;
			if (flag)
			{
				bool flag2 = this.plans.RemoveAll((WorkPlan x) => x == null) != 0;
				if (flag2)
				{
					Log.Error("Some manaSqueezePolicy were null after loading", false);
				}
				for (int i = 0; i < this.plans.Count; i++)
				{
					this.plans[i].workPlanStack = this;
				}
			}
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00006F30 File Offset: 0x00005130
		public void Reorder(WorkPlan plan, int offset)
		{
			int num = this.plans.IndexOf(plan);
			num += offset;
			bool flag = num >= 0;
			if (flag)
			{
				this.plans.Remove(plan);
				this.plans.Insert(num, plan);
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00006F78 File Offset: 0x00005178
		public int IndexOf(WorkPlan plan)
		{
			return this.plans.IndexOf(plan);
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00006F98 File Offset: 0x00005198
		public WorkPlan DoListing(Rect rect, Func<List<FloatMenuOption>> recipeOptionsMaker, ref Vector2 scrollPosition, ref float viewHeight)
		{
			WorkPlan result = null;
			GUI.BeginGroup(rect);
			Rect rect2;
			rect2..ctor(0f, 0f, 150f, 29f);
			Rect rect3;
			rect3..ctor(180f, 5f, 150f, 24f);
			Text.Font = 2;
			Widgets.Label(rect2, "魔力収集方針");
			Text.Font = 1;
			Widgets.Label(rect3, "獲得した魔力：" + this.compRace.TotalManaGained.ToString("F2"));
			Text.Anchor = 0;
			GUI.color = Color.white;
			Rect rect4;
			rect4..ctor(0f, 35f, rect.width, rect.height - 35f);
			Rect rect5;
			rect5..ctor(0f, 0f, rect4.width - 16f, viewHeight);
			Widgets.BeginScrollView(rect4, ref scrollPosition, rect5, true);
			float num = 0f;
			for (int i = 0; i < this.plans.Count; i++)
			{
				WorkPlan workPlan = this.plans[i];
				Rect rect6 = workPlan.DoInterface(0f, num, rect5.width, i);
				bool flag = Mouse.IsOver(rect6);
				if (flag)
				{
					result = workPlan;
				}
				num += rect6.height + 6f;
			}
			bool flag2 = Event.current.type == 8;
			if (flag2)
			{
				viewHeight = num + 60f;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			return result;
		}

		// Token: 0x040000A9 RID: 169
		[Unsaved]
		public CompSuccubusRace compRace;

		// Token: 0x040000AA RID: 170
		public List<WorkPlan> plans = new List<WorkPlan>();
	}
}
