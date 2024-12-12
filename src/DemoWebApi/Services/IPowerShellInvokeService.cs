using DemoWebApi.DTOs;

namespace DemoWebApi.Services;

public interface IPowerShellInvokeService
{
    Task<IList<DuplicateFileResult>> RunScriptAsync(ApiInputDto funcInput, CancellationToken cancellationToken);
}