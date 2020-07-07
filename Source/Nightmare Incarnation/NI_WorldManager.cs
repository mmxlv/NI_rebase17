using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace NightmareIncarnation
{
	public class NI_WorldManager : WorldComponent
	{
		public static List<int> loadIDs = new List<int>();

		public float TotalManaSended;
		public NI_WorldManager(World world) : base(world)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.TotalManaSended, "TotalManaSended", 0f, false);
		}

	}
}
