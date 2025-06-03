using InfoCarPing.ApiClient.Contract;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace InfoCarPing.ApiClient;

internal class ICJwkKeysStore
{
	private const string KeysUrl = "https://info-car.pl/oauth2/jwk";
	private readonly HttpClient _http;
	private readonly ICOpenIdConnectClientOptions _options;
	private readonly TimeSpan _keysTimeToLive;
	private DateTimeOffset _keysFetched;

	public ICJwkKeysStore( ICOpenIdConnectClientOptions options, HttpClient http, TimeSpan keysTimeToLive )
	{
		_options = options;
		_http = http;
		_keysTimeToLive = keysTimeToLive;
	}

	public async Task Refresh() => await FetchJwkKeys();

	public async ValueTask EnsureNotExpired() => await FetchJwkKeysIfNeeded();

	private async Task FetchJwkKeys()
	{
		var keysResponseMsg = await _http.GetAsync( KeysUrl );
		var keysResponse = await keysResponseMsg.Content.ReadFromJsonAsync<JwkResponse>();
		var keys = keysResponse!.Keys.Select( RsaKeyFromResponseKey ).ToArray();

		_options.JwtValidationParameters.IssuerSigningKeys = keys;
		_keysFetched = DateTimeOffset.Now;
	}

	private async ValueTask FetchJwkKeysIfNeeded()
	{
		if ( _options.JwtValidationParameters.IssuerSigningKeys?.Any() is not true
			|| DateTimeOffset.Now - _keysFetched > _keysTimeToLive )
		{
			await FetchJwkKeys();
		}
	}

	private static RsaSecurityKey RsaKeyFromResponseKey( JwkResponseKey key )
	{
		RSAParameters rsaParams = new()
		{
			Modulus = Base64UrlEncoder.DecodeBytes( key.N ),
			Exponent = Base64UrlEncoder.DecodeBytes( key.E )
		};

		return new RsaSecurityKey( rsaParams )
		{
			KeyId = key.Kid
		};
	}
}
