using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimTest.Testing;
using Verse;

namespace RimTest.Patches;
// ReSharper disable UnusedMember.Global
// ReSharper disable once UnusedType.Global


/// <summary>
/// Adds an entry point to draw and additional debug button on the toolbar.
/// The infix is necessary to catch the WidgetRow that the stock buttons are drawn to.
/// </summary>
/// <remarks>
///	Original patch from HugsLib
/// </remarks>
[HarmonyPatch(typeof(DebugWindowsOpener))]
[HarmonyPatch("DrawButtons")]
internal static class DebugWindowsOpener_Patch
{
	private static bool _patched;

	[HarmonyPrepare]
	public static bool Prepare()
	{
		LongEventHandler.ExecuteWhenFinished(() =>
			{
				if (!_patched)
				{
					Log.Warning("DebugWindowsOpener_Patch could not be applied.");
				}
				else
				{
					Log.Message("DebugWindowsOpener_Patch applied.");
				}
			}
		);

		return true;
	}

	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> DrawAdditionalButtons(IEnumerable<CodeInstruction> instructions)
	{
		_patched = false;
		CodeInstruction[] instructionsArr = instructions.ToArray();
		FieldInfo widgetRowField = AccessTools.Field(typeof(DebugWindowsOpener), "widgetRow");

		Log.Message($"DebugWindowsOpener_Patch: widgetRowField: {widgetRowField}");
		
		foreach (CodeInstruction inst in instructionsArr)
		{
			Log.Message($"DebugWindowsOpener_Patch: inst: {inst}");
			
			if (!_patched && widgetRowField is not null && inst.opcode == OpCodes.Bne_Un)
			{
				Log.Message($"Not patched DebugWindowsOpener_Patch: inst: {inst}");
				
				yield return new(OpCodes.Ldarg_0);
				yield return new(OpCodes.Ldfld, widgetRowField);
				yield return new(OpCodes.Call,
					((Action<WidgetRow>)TestingController.DrawDebugToolbarButton).Method
				);

				_patched = true;
			}

			yield return inst;
		}
	}
}

/// <summary>
/// Extends the width of the immediate window the dev toolbar buttons are drawn to to accommodate an additional button
/// </summary>
[HarmonyPatch(typeof(DebugWindowsOpener))]
[HarmonyPatch("DevToolStarterOnGUI")]
internal class DevToolStarterOnGUI_Patch
{
	private static bool _patched;

	[HarmonyPrepare]
	public static bool Prepare()
	{
		LongEventHandler.ExecuteWhenFinished(() =>
			{
				if (!_patched)
				{
					Log.Error("DevToolStarterOnGUI_Patch could not be applied.");
				}
				else
				{
					Log.Message("DevToolStarterOnGUI_Patch applied.");
				}
			}
		);

		return true;
	}

	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> ExtendButtonsWindow(IEnumerable<CodeInstruction> instructions)
	{
		_patched = false;

		foreach (CodeInstruction inst in instructions)
		{
			if (!_patched && inst.opcode == OpCodes.Ldc_R4 && 28f.Equals(inst.operand))
			{
				// add one to the number of expected buttons
				yield return new(OpCodes.Ldc_R4, 1f);
				yield return new(OpCodes.Add);

				_patched = true;
			}

			yield return inst;
		}
	}
}