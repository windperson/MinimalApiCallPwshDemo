using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace DemoWebApi.Tests;

public class PwshModuleTest
{
    [Fact]
    public void TestDuplicateFileFinderPwshFunction()
    {
        // Arrange
        using var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();
        using var ps = PowerShell.Create(runspace);
        var psInvocationSettings = new PSInvocationSettings
        {
            AddToHistory = false,
            ErrorActionPreference = ActionPreference.Stop,
        };

        #region TestScript

        const string testScript =
            """
            $tempModulePath = $env:TEMP + "\PwshModule$((Get-Date).Ticks)"
            if(-not (Test-Path $tempModulePath)) {
                New-Item -Path $tempModulePath -ItemType Directory -ErrorAction Stop | Out-Null
            }
            else {
                Remove-Item -Path $tempModulePath -Recurse -Force -ErrorAction Stop | Out-Null
                New-Item -Path $tempModulePath -ItemType Directory -ErrorAction Stop | Out-Null
            }
            Save-Module -Name Pester -Path $tempModulePath -ErrorAction Stop
            Import-Module $tempModulePath\Pester -Force -ErrorAction Stop

            $currentDir = [System.IO.Directory]::GetCurrentDirectory()
            $TestScriptPath = "$currentDir\..\..\..\PwshScripts\DuplicateFilesFinder.Tests.ps1"
            if(-not (Test-Path $TestScriptPath)) {
                throw "Test script not found at $TestScriptPath"
            }
            $result = Invoke-Pester -Path $TestScriptPath -PassThru
            return $result
            """;

        #endregion

        ps.AddScript(testScript);

        // Act
        var psResult = ps.Invoke(input: null, settings: psInvocationSettings).FirstOrDefault();

        // Assert
        Assert.NotNull(psResult);
        var testResult = psResult.Properties["Result"].Value as string;
        var totalCount = psResult.Properties["TotalCount"].Value as int?;
        var passedCount = psResult.Properties["PassedCount"].Value as int?;
        var failedCount = psResult.Properties["FailedCount"].Value as int?;
        Assert.NotNull(testResult);
        Assert.NotNull(totalCount);
        Assert.NotNull(passedCount);
        Assert.NotNull(failedCount);

        Assert.Equal("Passed", testResult);
        Assert.True(passedCount > 0);
        Assert.Equal(totalCount, passedCount);
        Assert.Equal(0, failedCount);
    }
}