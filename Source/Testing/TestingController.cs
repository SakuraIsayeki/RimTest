using RimTest.Core;
using UnityEngine;
using Verse;

namespace RimTest.Testing;
// ReSharper disable UnusedType.Global

/// <summary>
/// Manages the display of the test suite menu from the debug toolbar.
/// </summary>
[StaticConstructorOnStartup]
public static class TestingController
{
	internal static void DrawDebugToolbarButton(WidgetRow widgets)
	{
		const string testSuiteButtonTooltip = "Open the RimTest testing suite";

		if (widgets.ButtonIcon(RimTestTextures.testSuiteIcon, testSuiteButtonTooltip))
		{
			WindowStack stack = Find.WindowStack;

//			if (stack.IsOpen<Dialog_QuickstartSettings>())
//			{
//				stack.TryRemove(typeof(Dialog_QuickstartSettings));
//			}
//			else
//			{
//				stack.Add(new Dialog_QuickstartSettings());
//			}
		}
	}
}