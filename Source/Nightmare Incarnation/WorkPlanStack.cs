using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
    public class WorkPlanStack : IExposable
    {
        [Unsaved]
        public CompSuccubusRace compRace;

        public List<WorkPlan> plans = new List<WorkPlan>();

        public WorkPlanStack(CompSuccubusRace Comp)
        {
            this.compRace = Comp;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<WorkPlan>(ref this.plans, "plans", LookMode.Deep, new object[0]);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                if (this.plans.RemoveAll((WorkPlan x) => x == null) != 0)
                {
                    Log.Error("Some manaSqueezePolicy were null after loading");
                }
                for (int i = 0; i < this.plans.Count; i++)
                {
                    this.plans[i].workPlanStack = this;
                }
            }
        }

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

        public int IndexOf(WorkPlan plan)
        {
            return this.plans.IndexOf(plan);
        }

        public WorkPlan DoListing(Rect rect, Func<List<FloatMenuOption>> recipeOptionsMaker, ref Vector2 scrollPosition, ref float viewHeight)
        {
            WorkPlan result = null;
            GUI.BeginGroup(rect);
            Rect manaCollectionPolicyLabel = new Rect(0f, 0f, 150f, 29f);
            Rect manaCollectionResultLabel = new Rect(180f, 5f, 150f, 24f);
            Text.Font = GameFont.Medium;
            Widgets.Label(manaCollectionPolicyLabel, "Mana Collection Policy");
            Text.Font = GameFont.Small;
            Widgets.Label(manaCollectionResultLabel, "Mana Collected：" + this.compRace.TotalManaGained.ToString("F2"));
            Text.Anchor = 0;
            GUI.color = Color.white;
            Rect scrollbox = new Rect(0f, 35f, rect.width, rect.height - 35f);
            Rect scrollSelection = new Rect(0f, 0f, scrollbox.width - 16f, viewHeight);
            Widgets.BeginScrollView(scrollbox, ref scrollPosition, scrollSelection, true);
            float num = 0f;
            for (int i = 0; i < this.plans.Count; i++)
            {
                WorkPlan workPlan = this.plans[i];
                Rect scrollSelectionItem = workPlan.DoInterface(0f, num, scrollSelection.width, i);
                if (Mouse.IsOver(scrollSelectionItem))
                {
                    result = workPlan;
                }
                num += scrollSelectionItem.height + 6f;
            }
            if (Event.current.type == EventType.Layout)
            {
                viewHeight = num + 60f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            return result;
        }
    }
}
