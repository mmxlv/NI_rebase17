using System;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
	/// <summary>
	/// Paths to the squeeze policy icons
	/// </summary>
	[StaticConstructorOnStartup]
	internal class IconTex
	{
		public static readonly Texture2D Policy_AllowJoy = ContentFinder<Texture2D>.Get("System/PolicyIcon/Joy", true);

		public static readonly Texture2D Policy_DisallowJoy = ContentFinder<Texture2D>.Get("System/PolicyIcon/JoyD", true);

		public static readonly Texture2D Policy_AllowOverSqueeze = ContentFinder<Texture2D>.Get("System/PolicyIcon/Over", true);

		public static readonly Texture2D Policy_DisallowOverSqueeze = ContentFinder<Texture2D>.Get("System/PolicyIcon/OverD", true);

		public static readonly Texture2D Policy_AllowDown = ContentFinder<Texture2D>.Get("System/PolicyIcon/Down", true);

		public static readonly Texture2D Policy_DisallowDown = ContentFinder<Texture2D>.Get("System/PolicyIcon/DownD", true);

		public static readonly Texture2D Policy_AllowKill = ContentFinder<Texture2D>.Get("System/PolicyIcon/Kill", true);

		public static readonly Texture2D Policy_DisallowKill = ContentFinder<Texture2D>.Get("System/PolicyIcon/KillD", true);
	}
}
