using Azure;
using Azure.Security.KeyVault.Secrets;
using System.Diagnostics.CodeAnalysis;

namespace Beers.Common.Helpers;

[ExcludeFromCodeCoverage]
public static class SecretHelper
{
    public static SecretClient SecretClient { get; set; } = default!;

    /// <summary>
    /// Retrieves a secret by name
    /// </summary>
    /// <param name="secretName"></param>
    /// <returns></returns>
    public static string GetSecret(string secretName)
    {
        if (SecretClient == null)
        {
            throw new InvalidOperationException(
                "The secret client is not configured.  " +
                "This should not be called until after the call to LoadAuthenticatorConfiguration is completed");
        }

        try
        {
            var result = SecretClient.GetSecret(secretName);

            return result?.Value?.Value!;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve Azure Key Vault secret '{secretName}'. Status: {ex.Status}, ErrorCode: {ex.ErrorCode}.",
                ex);
        }
    }

    /// <summary>
    /// Retrieves a required secret by name and fails fast when empty.
    /// </summary>
    /// <param name="secretName"></param>
    /// <returns></returns>
    public static string GetRequiredSecret(string secretName)
    {
        var value = GetSecret(secretName);

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Missing required Azure Key Vault secret '{secretName}'. Please verify your Key Vault values.");
    }

    /// <summary>
    /// Asynchronously retrieves a secret by name
    /// </summary>
    /// <param name="secretName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (SecretClient == null)
        {
            throw new InvalidOperationException(
                "The secret client is not configured.  " +
                "This should not be called until after the call to LoadAuthenticatorConfiguration is completed");
        }

        try
        {
            var result = await SecretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);

            return result?.Value?.Value!;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve Azure Key Vault secret '{secretName}'. Status: {ex.Status}, ErrorCode: {ex.ErrorCode}.",
                ex);
        }
    }

}
