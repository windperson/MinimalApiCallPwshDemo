using System.Management.Automation;
using System.Management.Automation.Runspaces;
using DemoWebApi.DTOs;

namespace DemoWebApi.Services;

public class DuplicateFileFinderInvokeService : IPowerShellInvokeService, IDisposable
{
    private readonly RunspacePool _runspacePool;
    private readonly ILogger<DuplicateFileFinderInvokeService> _logger;
    private bool _disposedValue;

    public DuplicateFileFinderInvokeService(ILogger<DuplicateFileFinderInvokeService> logger)
    {
        _runspacePool = RunspaceFactory.CreateRunspacePool();
        _runspacePool.Open();
        _logger = logger;
    }

    public async Task<IList<DuplicateFileResult>> RunScriptAsync(ApiInputDto funcInput,
        CancellationToken cancellationToken)
    {
        using var ps = PowerShell.Create();
        ps.RunspacePool = _runspacePool;
        cancellationToken.Register(() =>
        {
            // ReSharper disable once AccessToDisposedClosure
            if (ps.InvocationStateInfo.State == PSInvocationState.Running)
            {
                // ReSharper disable once AccessToDisposedClosure
                ps.Stop();
            }
        });

        ps.Streams.Verbose.DataAdded += (_, args) =>
        {
            VerboseRecord verboseRecord = ps.Streams.Verbose[args.Index];
            _logger.LogTrace("VERBOSE: {verboseRecord}", verboseRecord);
        };
        ps.Streams.Information.DataAdded += (_, args) =>
        {
            InformationRecord infoRecord = ps.Streams.Information[args.Index];
            _logger.LogInformation("INFO: {infoRecord}", infoRecord);
        };

        var psInvocationSettings = new PSInvocationSettings
        {
            AddToHistory = false,
            ErrorActionPreference = ActionPreference.Stop,
        };
        try
        {
            ps.AddScript("Import-Module ./PwshScripts/DuplicateFilesFinder.psm1 -Verbose").AddStatement();

            ps.AddCommand("Get-DuplicateFile")
                .AddParameter("SourcePath", funcInput.SourceFolder)
                .AddParameter("ComparePath", funcInput.CompareFolder);
            if (!string.IsNullOrEmpty(funcInput.CompareType))
            {
                ps.AddParameter("CompareType", funcInput.CompareType);
            }

            var result =
                ps.Invoke(input: null, settings: psInvocationSettings).FirstOrDefault()?.BaseObject;

            if (result == null)
            {
                return new List<DuplicateFileResult>();
            }

            var duplicateFileResults = new List<DuplicateFileResult>(((object[])result).Length);
            foreach (dynamic duplicateFileInfo in (object[])result)
            {
                var duplicateFileResult = new DuplicateFileResult
                {
                    SourceFilePath = duplicateFileInfo.FilePath1.ToString(),
                    CompareFilePath = duplicateFileInfo.FilePath2.ToString()
                };
                duplicateFileResults.Add(duplicateFileResult);
            }

            return duplicateFileResults;
        }
        catch (PipelineStoppedException ex)
        {
            _logger.LogError(ex, "Pipeline stopped exception");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while running script");
            throw;
        }
    }

    // Make sure we dispose of the runspace pool properly
    void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _runspacePool.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}