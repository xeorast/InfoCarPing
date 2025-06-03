using System.Text.Json.Serialization;

namespace InfoCarPing.Contract;

#nullable disable warnings

internal class JwkResponse
{
    [JsonPropertyName("keys")]
    public Key[] Keys { get; set; }
}

internal class Key
{
    [JsonPropertyName("kty")]
    public string Kty { get; set; }

    [JsonPropertyName("e")]
    public string E { get; set; }

    [JsonPropertyName("kid")]
    public string Kid { get; set; }

    [JsonPropertyName("n")]
    public string N { get; set; }
}
