using System;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x02000019 RID: 25
	[StaticConstructorOnStartup]
	internal class IconTex
	{
		// Token: 0x04000098 RID: 152
		public static readonly Texture2D Policy_AllowJoy = ContentFinder<Texture2D>.Get("System/PolicyIcon/Joy", true);

		// Token: 0x04000099 RID: 153
		public static readonly Texture2D Policy_DisallowJoy = ContentFinder<Texture2D>.Get("System/PolicyIcon/JoyD", true);

		// Token: 0x0400009A RID: 154
		public static readonly Texture2D Policy_AllowOverSqueeze = ContentFinder<Texture2D>.Get("System/PolicyIcon/Over", true);

		// Token: 0x0400009B RID: 155
		public static readonly Texture2D Policy_DisallowOverSqueeze = ContentFinder<Texture2D>.Get("System/PolicyIcon/OverD", true);

		// Token: 0x0400009C RID: 156
		public static readonly Texture2D Policy_AllowDown = ContentFinder<Texture2D>.Get("System/PolicyIcon/Down", true);

		// Token: 0x0400009D RID: 157
		public static readonly Texture2D Policy_DisallowDown = ContentFinder<Texture2D>.Get("System/PolicyIcon/DownD", true);

		// Token: 0x0400009E RID: 158
		public static readonly Texture2D Policy_AllowKill = ContentFinder<Texture2D>.Get("System/PolicyIcon/Kill", true);

		// Token: 0x0400009F RID: 159
		public static readonly Texture2D Policy_DisallowKill = ContentFinder<Texture2D>.Get("System/PolicyIcon/KillD", true);
	}
}
