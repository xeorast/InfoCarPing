using InfoCarPing.ApiClient.Contract;
using Microsoft.IdentityModel.Tokens;
using Polly;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;

namespace InfoCarPing.ApiClient;

internal class ICOpenIdConnectClientOptions
{
	public required TokenValidationParameters JwtValidationParameters { get; set; }
}

internal partial class ICOpenIdConnectClient
{
	private const string LoginUrl = "https://info-car.pl/oauth2/login";
	private const string LoginPageUrl = LoginUrl;
	private const string LoginValidRedirectUrl = "https://info-car.pl/new/";
	private const string OAuthAuthorizeUrl = "https://info-car.pl/oauth2/authorize";

	private readonly ICOpenIdConnectClientOptions _options;
	private readonly HttpClient _http;
	private readonly JwtSecurityTokenHandler _jwtHandler;
	private DateTimeOffset _tokenIssued;
	private DateTimeOffset _tokenValidTo;

	private ICJwkKeysStore _keysStore;

	public ICOpenIdConnectClient( ICOpenIdConnectClientOptions options, HttpClient http )
	{
		_options = options;
		_http = http;
		_jwtHandler = new();
		_tokenValidTo = DateTimeOffset.Now;
		_keysStore = new( options, http, TimeSpan.FromDays( 1 ) );
	}

	public string? Token { get; private set; }

	public bool HasLoggedIn => Token is not null;

	#region Login
	public async Task LogIn( string username, string password )
	{
		var csrf = await GetCsrfToken();
		LoginDto dto = new()
		{
			Csrf = csrf,
			UserName = username,
			Password = password
		};

		var resp = await _http.PostAsync( LoginUrl, new FormUrlEncodedContent( dto ) );
		if ( resp is not { StatusCode: HttpStatusCode.Found, Headers.Location.AbsoluteUri: LoginValidRedirectUrl } )
		{
			throw new InfoCarPingException( "Invalid credentials" );
		}

		await GetTokenForSession();
	}

	private async Task<string> GetCsrfToken()
	{
		var loginFormResp = await _http.GetAsync( LoginPageUrl );
		var loginForm = await loginFormResp.Content.ReadAsStringAsync();
		var csrfMatch = CsrfInputRx().Match( loginForm );
		var csrfToken = csrfMatch.Groups[1].Value;
		return csrfToken;
	}

	[GeneratedRegex( """<input type="hidden" name="_csrf" value="([\da-f\-]*)" />""" )]
	private static partial Regex CsrfInputRx();

	private async Task GetTokenForSession()
	{
		await _keysStore.EnsureNotExpired();

		var sentState = Base64UrlEncoder.Encode( RandomNumberGenerator.GetBytes( 45 ) );
		var sentNonce = sentState;

		string uri = OAuthAuthorizeUrl +
			$"?response_type=id_token%20token" +
			$"&client_id=client" +
			$"&state={sentState}" +
			$"&redirect_uri=https%3A%2F%2Finfo-car.pl%2Fnew%2Fassets%2Frefresh.html" +
			$"&scope=openid%20profile%20email%20resource.read" +
			$"&nonce={sentNonce}" +
			$"&prompt=none";

		var respMsg = await _http.GetAsync( uri );

		await ExtractToken( respMsg, sentState, sentNonce );
	}

	#endregion

	#region Refresh token
	private async Task RefreshToken()
	{
		await _keysStore.EnsureNotExpired();

		var sentState = Base64UrlEncoder.Encode( RandomNumberGenerator.GetBytes( 45 ) );
		var sentNonce = sentState;

		var url = OAuthAuthorizeUrl +
			$"?response_type=id_token%20token" +
			$"&client_id=client" +
			$"&state={sentState}" +
			$"&redirect_uri=https%3A%2F%2Finfo-car.pl%2Fnew%2Fassets%2Frefresh.html" +
			$"&scope=openid%20profile%20email%20resource.read" +
			$"&nonce={sentNonce}" +
			$"&prompt=none";

		HttpRequestMessage reqMsg = new( HttpMethod.Get, url )
		{
			Headers =
			{
				{ "Authorization", $"Bearer {Token}" },
			}
		};

		var respMsg = await _http.SendAsync( reqMsg );

		await ExtractToken( respMsg, sentState, sentNonce );
	}

	public async Task EnsureSessionUpToDate()
	{
		if ( Token is null )
		{
			throw new InfoCarPingException( "You must first log in." );
		}

		var now = DateTimeOffset.Now;
		if ( _tokenValidTo - now < now - _tokenIssued || ( now - _tokenIssued ) > TimeSpan.FromMinutes( 7.5 ) )
		{
			await RefreshToken();
		}
	}

	#endregion

	#region Helpers

	IAsyncPolicy? refreshKeysAndRetryPolicy;

	private async ValueTask ExtractToken( HttpResponseMessage respMsg, string sentState, string sentNonce )
	{
		Debug.Assert( respMsg.StatusCode == HttpStatusCode.Found );
		Debug.Assert( respMsg.Headers.Location is not null );

		var fragment = respMsg.Headers.Location.Fragment.TrimStart( '#' );
		var fragmentDictionary = HttpUtility.ParseQueryString( fragment );
		var error = fragmentDictionary["error"];
		if ( error is not null )
		{
			throw new InfoCarAuthException( $"token error: {error}" );
		}

		var accessToken = fragmentDictionary["access_token"] ?? throw new InfoCarAuthException( "No access_token received. It may be necessary to log in again." );
		var receivedState = fragmentDictionary["state"] ?? throw new InfoCarAuthException( "No state received. It may be necessary to log in again." );
		var expiresInStr = fragmentDictionary["expires_in"] ?? throw new InfoCarAuthException( "No expires_in received. It may be necessary to log in again." );
		var idToken = fragmentDictionary["id_token"] ?? throw new InfoCarAuthException( "No id_token received. It may be necessary to log in again." );

		if ( receivedState != sentState )
		{
			throw new InfoCarAuthException( "Received state does not match." );
		}

		refreshKeysAndRetryPolicy ??= Policy
			.Handle<SecurityTokenInvalidSignatureException>()
			.RetryAsync( 1, async ( token, n ) => await _keysStore.Refresh() );

		SecurityToken jwt = await refreshKeysAndRetryPolicy.ExecuteAsync( () =>
		{
			_jwtHandler.ValidateToken( idToken, _options.JwtValidationParameters, out var jwt );
			return Task.FromResult( jwt );
		} );

		var receivedNonce = ( (JwtSecurityToken)jwt ).Claims.FirstOrDefault( c => c.Type == "nonce" )?.Value;

		if ( receivedNonce != sentNonce )
		{
			throw new InfoCarAuthException( "Received nonce does not match." );
		}

		var expiresIn = int.Parse( expiresInStr );
		_tokenIssued = DateTimeOffset.Now;
		_tokenValidTo = _tokenIssued.AddSeconds( expiresIn );
		Token = accessToken;
	}

	#endregion
}

[Serializable]
public class InfoCarPingException : Exception
{
	public InfoCarPingException() { }
	public InfoCarPingException( string message ) : base( message ) { }
	public InfoCarPingException( string message, Exception inner ) : base( message, inner ) { }
	protected InfoCarPingException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
}

[Serializable]
public class InfoCarAuthException : Exception
{
	public InfoCarAuthException() { }
	public InfoCarAuthException( string message ) : base( message ) { }
	public InfoCarAuthException( string message, Exception inner ) : base( message, inner ) { }
	protected InfoCarAuthException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
}

[Serializable]
public class TransientPingException : Exception
{
	public TransientPingException() { }
	public TransientPingException( string message ) : base( message ) { }
	public TransientPingException( string message, Exception inner ) : base( message, inner ) { }
	protected TransientPingException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
}
