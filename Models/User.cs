using System.Text.Json.Serialization;

namespace TanukiPanel.Models;

public class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = "";

    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";
}
