# Starcounter.Xunit.Runner
`Xunit` console runner for executing tests in the same domain as the Starcounter database which makes it possible to use any of the Starcounter platform functionality within a test. Hence, it fills the gap where Starcounter version `2.3` lacks the functionality to connect to the Starcounter database programmatically, also known as Self-hosting apps. 

`Starcounter.Xunit.Runner` essentially executes all `Xunit` tests found within the calling assembly.

## How to use it
* Create a normal Starcounter App, version 2.3. Make sure it targets `.NET Framework 4.5.2` or above.
* Add reference to `StarcounterXunitRunner.dll` through nuget: `Install-Package Starcounter.Xunit.Runner`
* Set `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` for the added `weaver.ignore` content file. 
* Include namespace `Starcounter.Xunit.Runner` and create an instance of `StarcounterXunitRunner` in `Main()`
```c#
StarcounterXunitRunner runner = new StarcounterXunitRunner();
runner.Start();
```
* Write Xunit tests, please see the [documentation](https://xunit.github.io/) on how to write tests.
* Any time the Starcounter database is accessed, the code needs to be wrapped around 
```c#
Scheduling.ScheduleTask(() => 
{ 
    /* Test code here */
}, waitForCompletion: true);
```
Make sure to set `waitForCompletion = true` because `Xunit` may execute tests in parallel, i.e. when `StarcounterXunitRunner.RunTestsInParallel=true`.
* Compile and deploy the Starcounter test App as any other Starcounter App, `star.exe <StarcounterTestApp>.exe`. The test result should be displayed in the console.

## StarcounterXunitRunner
Dependencies:
* .NET Framework 4.5.2
* xunit 2.2.0
* xunit.runner.console 2.2.0
* more.xunit.runner.reporters 2.2.2
* more.xunit.runner.utility 2.2.2

A `Xunit` runner which executes tests as a xunit console application in the same AppDomain as the Starcounter database. 

### Starcounter.Xunit.Runner.StarcounterXunitRunner API
```c#
public class StarcounterXunitRunner
{
    //
    // Summary:
    //     A Xunit runner for executing tests from the calling assembly in the same AppDomain
    //     as the hosted Starcounter database. This runner should be created inside a Starcounter
    //     Application.
    //
    // Parameters:
    //   triggerOnInstanceCreation:
    //     If true (default: false): tests will be executed on this instance creation.
    //
    //   testCaseFilter:
    //     Set to be able to filter the test cases to decide which ones to run. If this
    //     is not set, then all test cases will be run.
    //
    //   runTestsInParallel:
    //     Set to true (default: false) to run test collections in parallel; set to false
    //     to run them sequentially.
    //
    //   developerMode:
    //     Set to true (default: false) to print executed test cases to console, including
    //     any test case stack trace. Note that by doing this the tests will be run in sequence
    //     i.e. RunTestsInParallel will be overridden to false!
    public StarcounterXunitRunner(bool triggerOnInstanceCreation = false, Func<ITestCase, bool> testCaseFilter = null, bool runTestsInParallel = false, bool developerMode = false);

    //
    // Summary:
    //     Set to be able to filter the test cases to decide which ones to run. If this
    //     is not set, then all test cases will be run.
    public Func<ITestCase, bool> TestCaseFilter { get; set; }
    //
    // Summary:
    //     Set to true (default: true) to run test collections in parallel; set to false
    //     to run them sequentially.
    public bool RunTestsInParallel { get; set; }
    //
    // Summary:
    //     Set to true (default: false) to print executed test cases to console, including
    //     any test case stack trace. Note that by doing this the tests will be run in sequence
    //     i.e. RunTestsInParallel will be overridden to false!
    public bool DeveloperMode { get; set; }

    //
    // Summary:
    //     Starts test collection execution.
    //
    // Parameters:
    //   typeName:
    //     If null (default: null): All tests will be run. Otherwise only executing the
    //     tests within the typeName class i.e. typeName="NameSpace.ClassName". Starcounter.Xunit.Runner.StarcounterXunitRunner.TestCaseFilter
    //     will still be taken into account though.
    //
    //   testReportName:
    //     If null (default: null): No test report will be generated. Set the desired name
    //     (FullPath or only FileName) to generate test report, file type is not needed.
    public void Start(string typeName = null, string testReportName = null);
}
```

## Demo\ScTestApp
A Starcounter version `2.3` test App which references `StarcounterXunitRunner`. It illustrates how to use `StarcounterXunitRunner` together with an App which contains a Starcounter database, `ScApp.ScAppDb`, for different `StarcounterXunitRunner` configurations.

Using:
* .NET Framework 4.5.2
* StarcounterXunitRunner 1.0.1
* xunit 2.2.0

Test result from running `star.exe ScTestApp.exe`.
```
  Discovering: ScTestApp
  Discovered:  ScTestApp
  Starting:    ScTestApp (parallel test collections = off, max threads = 8)
    ScTestApp.TestSetAddingDatabaseEntries.TestCase_AddingNewRow_3
    ScTestApp.TestSetAddingDatabaseEntries.TestCase_AddingNewRow_1
    ScTestApp.TestSetAddingDatabaseEntries.TestCase_AddingNewRow_2
    ScTestApp.TestSetWithSavedContextAndPriority.TestCaseWithHighestPriority
    ScTestApp.TestSetWithSavedContextAndPriority.TestCaseWithMiddlePriority
    ScTestApp.TestSetWithSavedContextAndPriority.TestCaseWithNoPriority
    ScTestApp.TestSetWithSavedContextAndPriority.TestCaseWithLowestPriority
    ScTestApp.TestSetDoNotSaveContext.TestSetDoNotSaveContext_Test
    ScTestApp.TestSetDoNotSaveContext.TestSetDoNotSaveContext_Test2
    ScTestApp.TestSetDoNotSaveContext.TestSetDoNotSaveContext_Test3
  Finished:    ScTestApp

=== TEST EXECUTION SUMMARY ===
   ScTestApp  Total: 10, Errors: 0, Failed: 0, Skipped: 0, Time: 0,105s
   Test report generated: C:\XunitTestReport\TestReportUsingTestCaseFilter.xml
   Test report generated: C:\XunitTestReport\TestReportUsingTestCaseFilter.html


  Discovering: ScTestApp
  Discovered:  ScTestApp
  Starting:    ScTestApp (parallel test collections = off, max threads = 8)
    ScTestApp.TestSetAlwaysFailing.TestCase_AlwaysFailing_1
      This assertion will always fail
      Actual:   False
      Expected: True
      Stack Trace:
        C:\Github\Starcounter\Starcounter.Xunit.Runner\Demo\ScTestApp\TestSetAlwaysFailing.cs(13,0): at ScTestApp.TestSetAlwaysFailing.TestCase_AlwaysFailing_1()
    ScTestApp.TestSetAlwaysFailing.TestCase_AlwaysFailing_2
      This assertion will always fail
      Actual:   False
      Stack Trace:
      Expected: True
        C:\Github\Starcounter\Starcounter.Xunit.Runner\Demo\ScTestApp\TestSetAlwaysFailing.cs(19,0): at ScTestApp.TestSetAlwaysFailing.TestCase_AlwaysFailing_2()
    ScTestApp.TestSetAlwaysFailing.TestCase_AlwaysFailing_3
      Actual:   False
      This assertion will always fail
      Stack Trace:
      Expected: True
        C:\Github\Starcounter\Starcounter.Xunit.Runner\Demo\ScTestApp\TestSetAlwaysFailing.cs(25,0): at ScTestApp.TestSetAlwaysFailing.TestCase_AlwaysFailing_3()
  Finished:    ScTestApp

=== TEST EXECUTION SUMMARY ===
   ScTestApp  Total: 3, Errors: 0, Failed: 3, Skipped: 0, Time: 0,014s
   Test report generated: C:\Program Files\Starcounter\TestReportUsingTypeName.xml
   Test report generated: C:\Program Files\Starcounter\TestReportUsingTypeName.html
```

## Demo\ScApp
A dummy Starcounter version `2.3` App which only has one database-table. Used for testing purposes only.
```c#
[Database]
public class ScAppDb
{
    public string Name { get; set; }
    public int Integer { get; set; }
    public DateTime DateCreated { get; set; }
}
```

It has handlers for the following
* `/ScApp/CreateNewEntry` - create a new row with random data
* `/ScApp/CreateNewEntry/<# of new entries>` - create multiple new rows with random data
* `/ScApp/DeleteAll` - delete all rows
* `/ScApp/ListAll` - list all rows in text format