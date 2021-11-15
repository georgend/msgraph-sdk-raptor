# msgraph-sdk-raptor

[![Dependabot Status](https://api.dependabot.com/badges/status?host=github&repo=microsoftgraph/msgraph-sdk-raptor)](https://dependabot.com)

This repository consists of test projects which are broadly categorized into 2.

1. compilation tests
2. execution tests

The compilation tests, test the successful compilation of the language-specific snippets from Microsoft Graph documentation. For each snippet, there is an NUnit test case that outputs compilation result i.e whether a test compiled successfully or not.

The execution tests, other than testing compilation of the snippets, use the compiled binary to make a request to the demo tenant and reports error if there's a service exception i.e 4XX or 5xx response. Otherwise reports success.

A test result for both compilation and execution tests includes:
- Root cause, if it is a known failure
- Documentation page where the snippet appears
- Piece of code that is to be compiled with line numbers
- Compiler error message

There are 7 C# test projects in total as noted below. The first 4 C# tests below are compilation tests, the next 2 are execution tests and finally an arbitraryDllTest.

1. CsharpBetaTests
2. CsharpBetaKnownFailureTests
3. CsharpV1Tests
4. CsharpV1KnownFailureTests

5. CsharpBetaExecutionTests
6. CsharpV1ExecutionTests

7. CSharpArbitraryDllTests

 The arbitraryDllTest is useful in running customized tests for an unpublished dll, which can consist of a proposed metadata or generator changes.

There are also 4 Java test projects, as listed below. These are all compilation tests

1. JavaBetaTests
2. JavaBetaKnownFailureTests
3. JavaV1Tests
4. JavaV1KnownFailureTests


## How to debug in VSCode locally or in Github Codespaces

1. Set two environment variables, either locally or in your [codespaces settings](https://github.com/settings/codespaces)
     - `BUILD_SOURCESDIRECTORY`: Where the documentation repository will be checked out. Value should be `/workspaces` for Codespaces.
     - `RAPTOR_CONFIGCONNECTIONSTRING`: Connection string to Azure App Configuration containing settings for execution. An empty App Config works fine for compilation tests.
1. Create a new codespace or open cloned msgraph-sdk-raptor folder in VSCode
1. Make sure to use PowerShell (pwsh) as your terminal (automatic in Codespaces)
1. Make sure that the C# compilation tools are installed (automatic in Codespaces)
1. Clone documentation repo using the predefined task:
    - open Command Palette (`Ctrl + Shift + P` or `Cmd + Shift + P`)
    - select `Run Task`
    - select `checkout docs repo`
    - select your branch from docs, leave `main` if you want to keep default branch
    - confirm with `YES`
      - confirmation is to prevent deleting local changes in case of subsequent runs of the task
1. Build the solution
    - open Command Palette
    - select `Run Task`
    - select `build`
1. Run all the tests from a single project (e.g. CsharpV1Tests)
    - open Command Palette
    - select `Run Test Task`
    - select `Run CsharpV1Tests`
    - hit Enter when test filter option shows `.` (i.e. all the tests)
1. Run individual tests
    - open Command Palette
    - select `Run Test Task`
    - select `Run CsharpV1Tests`
    - enter a test name filter, e.g. `workbook`
1. Debug individual tests
    - Open Command Palette
    - select `Run Test Task`
    - select `Debug CsharpV1Tests`
    - enter a test name filter, e.g. `get-workbookcomment-csharp-V1-compiles`
    - there will be a process id to attach to in terminal output e.g.:
      ```
      Process Id: 24536, Name: dotnet
      ```
    - start debugger using `.NET Attach`
    - select process from the terminal output in the process dropdown
    - find bugs and fix them :)


## Pipelines
The repository also contains a couple of CI pipelines. The CI pipelines run the tests outlined above and the output of these tests are then used to report success or failure rates in a graphical and analytical approach.
The pipelines are running in a private Azure DevOps instance [here](https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_build?view=folders&treeState=XFJhcHRvcg%3D%3D)
There exist pipelines that run when a PR is created on `msgraph-sdk-raptor` repo and others that run on a schedule. Pipelines that are triggered by PR creation are broadly categorized into
- those that run **excluding** known issues
- those that run on known issues (these tests are appended the suffix "Known Issues")

The pipelines with tests that run excluding known issues, can be used to report whether any new issues were introduced. Pipelines containing tests that run on known issues, can report whether any known issues have been fixed. There exists a list of known issues, within the `TestsCommon/TestDataGenerator.cs` file, which is useful in identifying known issues.

The pipelines are:
- Beta C# Snippets  (runs c# compilation tests)
- V1 C# Snippets  (runs c# compilation tests)

- Beta C# Snippet Execution Tests
- V1 C# Snippet Execution Tests

- V1 C# Snippets - Known Issues
- Beta C# Snippets - Known Issues

And the equivalent pipelines for running java tests are
- V1 Java Snippet Compilation Tests
- Beta Java Snippet Compilation Tests

- Beta Java Snippet Compilation Tests - Known Issues
- V1 Java Snippet Compilation Tests - Known Issues

The scheduled pipelines are categorized into daily and weekly schedules. A single scheduled pipeline can contain both categories of tests in a single run to report all failures including known issues. Azure DevOps tooling allows us to have these categories reflected in the test results page.

### Known Issue Distribution and Detailed Description Tables
- [C# V1 Execution Tests - Known Issues Distribution](./report/V1-csharp-execution-known-issues-report.html)
- [C# V1 Execution Tests - Known Issues Table](./report/V1-csharp-execution-known-issues)
- [C# V1 Compilation Tests - Known Issues Distribution](./report/V1-csharp-compilation-known-issues-report.html)
- [C# V1 Compilation Tests - Known Issues Table](./report/V1-csharp-compilation-known-issues)
- [C# Beta Compilation Tests - Known Issues Distribution](./report/Beta-csharp-compilation-known-issues-report.html)
- [C# Beta Compilation Tests - Known Issues Table](./report/Beta-csharp-compilation-known-issues)
