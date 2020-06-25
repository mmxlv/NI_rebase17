using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x02000004 RID: 4
	public class ITab_ManaPolicy : ITab
	{
		// Token: 0x0600000E RID: 14 RVA: 0x000024D0 File Offset: 0x000006D0
		public ITab_ManaPolicy()
		{
			this.size = ITab_ManaPolicy.WinSize;
			this.labelKey = Translator.Translate("NItxt_TabManaPolicy");
			this.tutorTag = "吸魔方針";
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000F RID: 15 RVA: 0x00002524 File Offset: 0x00000724
		public CompSuccubusRace CompRace
		{
			get
			{
				return base.SelPawn.GetComp<CompSuccubusRace>();
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002544 File Offset: 0x00000744
		protected override void FillTab()
		{
			Rect rect;
			rect..ctor(ITab_ManaPolicy.WinSize.x - ITab_ManaPolicy.PasteX, ITab_ManaPolicy.PasteY, ITab_ManaPolicy.PasteSize, ITab_ManaPolicy.PasteSize);
			Rect rect2 = GenUI.ContractedBy(new Rect(0f, 0f, ITab_ManaPolicy.WinSize.x, ITab_ManaPolicy.WinSize.y), 10f);
			Func<List<FloatMenuOption>> recipeOptionsMaker = delegate()
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				bool flag = !GenCollection.Any<FloatMenuOption>(list);
				if (flag)
				{
					list.Add(new FloatMenuOption(Translator.Translate("NoneBrackets"), null, 4, null, null, 0f, null, null));
				}
				return list;
			};
			this.mouseoverPlan = this.CompRace.policy.DoListing(rect2, recipeOptionsMaker, ref this.scrollPosition, ref this.viewHeight);
		}

		// Token: 0x04000004 RID: 4
		private float viewHeight = 1000f;

		// Token: 0x04000005 RID: 5
		private Vector2 scrollPosition = default(Vector2);

		// Token: 0x04000006 RID: 6
		private WorkPlan mouseoverPlan;

		// Token: 0x04000007 RID: 7
		private static readonly Vector2 WinSize = new Vector2(420f, 480f);

		// Token: 0x04000008 RID: 8
		[TweakValue("Interface", 0f, 128f)]
		private static float PasteX = 48f;

		// Token: 0x04000009 RID: 9
		[TweakValue("Interface", 0f, 128f)]
		private static float PasteY = 3f;

		// Token: 0x0400000A RID: 10
		[TweakValue("Interface", 0f, 32f)]
		private static float PasteSize = 24f;
	}
}
