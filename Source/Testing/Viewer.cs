using RimTest.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;
using static RimTest.Testing.Assembly2TestSuiteLink;
using static RimTest.Testing.AssemblyExplorer;
using static RimTest.Testing.AssemblyStatusExtension;
using static RimTest.Testing.TestExplorer;
using static RimTest.Testing.TestStatusExtension;
using static RimTest.Testing.TestSuite2TestLink;
using static RimTest.Testing.TestSuiteExplorer;
using static RimTest.Testing.TestSuiteStatusExtension;
namespace RimTest.Testing;

internal static class Viewer
{
    private static void LogDetailledErrors(ICollection<Assembly> asms)
    {
        foreach (Assembly asm in asms)
        {
            AssemblyStatus asmStatus = GetAssemblyStatus(asm);
            string asmName = asm.GetName().Name;
            
            Action log = asmStatus switch
            {
                AssemblyStatus.WARNING => () => Log.Warning($"[{StatusSymbol(asmStatus)}] {asmName} > {GetAssemblyError(asm).Message}"),
                AssemblyStatus.ERROR => () => Log.Error($"[{StatusSymbol(asmStatus)}] {asmName} > {GetAssemblyError(asm)}"),
                AssemblyStatus.UNKNOWN =>  () => Log.Message($"[{StatusSymbol(asmStatus)}] {asmName} > Not Run Yet"),
                _ => null
            };

            log?.Invoke();
                
            //Errored tests display
            foreach (Type testSuite in GetTestSuites(asm))
            {
                TestSuiteStatus tsStatus = GetTestSuiteStatus(testSuite);
                
                switch (tsStatus)
                {
                    case TestSuiteStatus.PASS: 
                        continue;
                    
                    case TestSuiteStatus.WARNING or TestSuiteStatus.SKIP:
                        Log.Warning($"    [{StatusSymbol(tsStatus)}] {testSuite.Name} > {GetTestSuiteError(testSuite)}");
                        break;
                    case TestSuiteStatus.ERROR:
                        Log.Error($"    [{StatusSymbol(tsStatus)}] {testSuite.Name} > {GetTestSuiteError(testSuite)}");
                        break;
                    case TestSuiteStatus.UNKNOWN:
                        Log.Message($"    [{StatusSymbol(tsStatus)}] {testSuite.Name} > Not Run Yet");
                        break;
                    default:
                        break;
                }

                foreach (MethodInfo test in GetTests(testSuite))
                {
                    TestStatus tStatus = GetTestStatus(test);
                    
                    switch (tStatus)
                    {
                        case TestStatus.SKIP:
                            Log.Warning($"        [{StatusSymbol(tStatus)}] {testSuite.Name}.{test.Name} > {GetTestError(test)}");
                            continue;
                        case TestStatus.ERROR:
                            Log.Error($"        [{StatusSymbol(tStatus)}] {testSuite.Name}.{test.Name} > {GetTestError(test)}");
                            continue;
                        case TestStatus.UNKNOWN:
                            Log.Message($"        [{StatusSymbol(tStatus)}] {testSuite.Name}.{test.Name} > Not Run Yet");
                            continue;
                    }
                }
            }
        }
    }


    private static string BuildAsmSummary(Assembly asm, Tally<TestSuiteStatus> tsTally, Tally<TestStatus> tTally)
    {
        StringBuilder builder = new();

        builder.Append($"[{StatusSymbol(GetAssemblyStatus(asm))}] {asm.GetName().Name} ");
        builder.Append($"|| Test Suites :");
        foreach (TestSuiteStatus status in Enum.GetValues(typeof(TestSuiteStatus)))
        {
            int tally = tsTally[status];
            if (tally != 0)
            {
                builder.Append($" {tally} {StatusSymbol(status)} ");
            }
        }

        builder.Append($"|| Tests :");
        foreach (TestStatus status in Enum.GetValues(typeof(TestStatus)))
        {
            int tally = tTally[status];
            if (tally != 0)
            {
                builder.Append($" {tally} {StatusSymbol(status)} ");
            }
        }

        return builder.ToString();
    }

    private static void LogSummary(ICollection<Assembly> asms)
    {
        foreach (Assembly asm in asms)
        {
            Tally<TestSuiteStatus> tsTally = new();
            Tally<TestStatus> tTally = new();

            // test results tallying
            foreach (Type testSuite in GetTestSuites(asm))
            {
                tsTally[GetTestSuiteStatus(testSuite)]++;

                foreach (MethodInfo test in GetTests(testSuite))
                {
                    tTally[GetTestStatus(test)]++;
                }
            }

            string asmResult = BuildAsmSummary(asm, tsTally, tTally);

            switch (GetAssemblyStatus(asm))
            {
                case AssemblyStatus.UNKNOWN or AssemblyStatus.PASS:
                    Log.Message(asmResult);
                    continue;
                case AssemblyStatus.WARNING:
                    Log.Warning(asmResult);
                    break;
                case AssemblyStatus.ERROR:
                    Log.Error(asmResult);
                    break;
            }
        }
    }

    public static void LogTestsResults()
    {
        List<Assembly> asms = GetAssemblies();
        asms.SortBy(asm => asm.FullName);
        
        if (!RimTestMod.Settings.RunOwnTests)
        {
            asms.Remove(Assembly.GetExecutingAssembly());
        }

        Log.Message("==TESTING START");
        Log.Message("__SUMMARY");
        LogSummary(asms);
        Log.Message("__ERRORS");
        LogDetailledErrors(asms);
        Log.Message("==TESTING END");

    }
}