using System.Reflection;
using UnityEngine;
using Verse;


namespace RimTest.Core;
// ReSharper disable InconsistentNaming

/// <summary>
/// Loads and stores textures from the HugsLib /Textures folder
/// </summary>
[StaticConstructorOnStartup]
internal static class RimTestTextures
{
	
	public static Texture2D testSuiteIcon;

	static RimTestTextures()
	{
		foreach (FieldInfo fieldInfo in typeof(RimTestTextures).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			fieldInfo.SetValue(null, ContentFinder<Texture2D>.Get(fieldInfo.Name));
		}
	}
}