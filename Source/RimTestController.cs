using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RimTest;

/// <summary>
/// Controller implementation for the library.
/// This is a workaround to get Patches to work.
/// </summary>
public class RimTestController
{
	private readonly HashSet<Assembly> _autoHarmonyPatchedAssemblies = new();
	private static bool _earlyInitializationCompleted;

	public static RimTestController Instance { get; } = new();
	internal Harmony HarmonyInst { get; private set; }

	// most of the initialization happens during Verse.Mod instantiation. Pretty much no vanilla data is yet loaded at this point.
	internal static void EarlyInitialize()
	{
		try
		{
			if (_earlyInitializationCompleted)
			{
				Log.Warning("Attempted repeated early initialization of controller: " + Environment.StackTrace);

				return;
			}

			_earlyInitializationCompleted = true;
			Instance.InitializeController();
		}
		catch (Exception e)
		{
			Log.Error("An exception occurred during early initialization: " + e);
		}
	}

	private void InitializeController()
	{
		ApplyHarmonyPatches();
	}

	private void ApplyHarmonyPatches()
	{
		try
		{
			if (ShouldHarmonyAutoPatch(typeof(RimTestController).Assembly, "RimTest"))
			{
				Harmony.DEBUG = GenCommandLine.CommandLineArgPassed("harmony_debug");
				HarmonyInst = new("latrisstitude.rimtest");
				HarmonyInst.PatchAll(typeof(RimTestController).Assembly);
			}
		}
		catch (Exception e)
		{
			Log.Error(e.ToString());
		}
	}

	internal bool ShouldHarmonyAutoPatch(Assembly assembly, string modId)
	{
		if (_autoHarmonyPatchedAssemblies.Contains(assembly))
		{
			Log.Warning($"The {assembly.GetName().Name} assembly contains multiple ModBase mods with HarmonyAutoPatch set to true. This warning was caused by modId {modId}.");

			return false;
		}

		_autoHarmonyPatchedAssemblies.Add(assembly);

		return true;
	}
}