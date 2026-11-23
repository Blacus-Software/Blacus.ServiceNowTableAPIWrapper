namespace Blacus.ServiceNowTableAPIWrapper.Abstractions;

using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public interface IServiceNowCredentialProvider
{
    ValueTask<AuthenticationHeaderValue?> GetAuthorizationHeaderAsync(CancellationToken ct = default);
}
