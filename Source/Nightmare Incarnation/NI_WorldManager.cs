using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x02000006 RID: 6
	public class NI_WorldManager : WorldComponent
	{
		// Token: 0x06000014 RID: 20 RVA: 0x00002AA6 File Offset: 0x00000CA6
		public NI_WorldManager(World world) : base(world)
		{
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002AB1 File Offset: 0x00000CB1
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.TotalManaSended, "TotalManaSended", 0f, false);
		}

		// Token: 0x0400004E RID: 78
		public static List<int> loadIDs = new List<int>();

		// Token: 0x0400004F RID: 79
		public float TotalManaSended;
	}
}
