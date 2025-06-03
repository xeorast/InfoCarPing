using System.Text.Json.Serialization;

namespace InfoCarPing.ApiClient.Contract;

#nullable disable warnings

internal class JwkResponse
{
	[JsonPropertyName( "keys" )]
	public JwkResponseKey[] Keys { get; set; }
}

internal class JwkResponseKey
{
	[JsonPropertyName( "kty" )]
	public string Kty { get; set; }

	[JsonPropertyName( "e" )]
	public string E { get; set; }

	[JsonPropertyName( "kid" )]
	public string Kid { get; set; }

	[JsonPropertyName( "n" )]
	public string N { get; set; }
}
