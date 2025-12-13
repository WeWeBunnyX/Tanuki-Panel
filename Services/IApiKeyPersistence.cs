using TanukiPanel.Models;

namespace TanukiPanel.Services;

/// <summary>
/// Service for persisting and loading API key configuration.
/// </summary>
public interface IApiKeyPersistence
{
    /// <summary>
    /// Saves the API key to persistent storage.
    /// </summary>
    /// <param name="apiKey">The API key to save.</param>
    /// <returns>True if save was successful; false otherwise.</returns>
    bool SaveApiKey(string apiKey);

    /// <summary>
    /// Loads the saved API key from persistent storage.
    /// </summary>
    /// <returns>The API key configuration if found; null otherwise.</returns>
    ApiKeyConfig? LoadApiKey();
}
