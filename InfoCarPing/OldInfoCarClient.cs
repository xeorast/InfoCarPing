//using InfoCarPing.Contract;
//using Microsoft.IdentityModel.Tokens;
//using System.Diagnostics;
//using System.IdentityModel.Tokens.Jwt;
//using System.Net;
//using System.Net.Http.Json;
//using System.Security.Cryptography;
//using System.Text.RegularExpressions;
//using System.Web;

//namespace InfoCarPing;

//public partial class InfoCarClient
//{
//	private readonly InfoCarClientOptions _options;
//	private readonly HttpClient _http;
//	private readonly JwtSecurityTokenHandler _jwtHandler;
//	private readonly TokenValidationParameters _jwtValidationParameters;
//	private string? _token;
//	private DateTimeOffset _tokenIssued;
//	private DateTimeOffset _tokenValidTo;

//	public InfoCarClient( InfoCarClientOptions options )
//	{
//		_options = options;
//		_http = new( new HttpClientHandler()
//		{
//			AllowAutoRedirect = false,
//			CookieContainer = new()
//		} );
//		_jwtHandler = new();
//		_jwtValidationParameters = new()
//		{
//			ValidIssuer = "https://info-car.pl/oauth2/",
//			ValidAudience = "client",
//		};
//		_tokenValidTo = DateTimeOffset.Now;
//	}

//	public async Task<ExamDayDto[]> GetExams( string category, int wordId, TimeSpan maxWaitingTime )
//	{
//		await RefreshTokenIfNeeded();

//		var startDate = DateTimeOffset.UtcNow.AddDays( 2 );
//		var endDate = startDate.AddDays( 2 * 31 );

//		SheduleRequest req = new()
//		{
//			Category = category,
//			WordId = wordId,
//			StartDate = startDate,
//			EndDate = endDate,
//		};

//		HttpRequestMessage reqMsg = new( HttpMethod.Put, "https://info-car.pl/api/word/word-centers/exam-schedule" )
//		{
//			Content = JsonContent.Create( req ),
//			Headers =
//			{
//				{ "Authorization", $"Bearer {_token}" }
//			}
//		};

//		var respMsg = await _http.SendAsync( reqMsg );
//		if ( respMsg 
//			is { StatusCode: HttpStatusCode.Found, Headers.Location.Authority: "maintenance.info-car.pl" }
//			or { StatusCode: HttpStatusCode.ServiceUnavailable } )
//		{
//			throw new TransientPingException( "Service reported maintenance break. If this error is not gone in two minutes, restarting the application may help." );
//		}

//		respMsg.EnsureSuccessStatusCode();

//		var resp = await respMsg.Content.ReadFromJsonAsync<SheduleResponse>();

//		var days = resp!.Schedule.ScheduledDays
//			.Select( d => new ExamDayDto(
//				d.Day,
//				d.ScheduledHours
//					.Select( h => new ExamHourDto( h.Time, h.PracticeExams.Sum( e => e.Places ) ) )
//					.Where( h => h.Places > 0 )
//					.Where( IsWithinTime )
//					.ToArray()
//			) )
//			.Where( d => d.Exams.Length > 0 )
//			.Where( d => d.Day.ToDateTime( TimeOnly.MinValue ) - DateTimeOffset.Now < maxWaitingTime )
//			.ToArray();

//		return days;
//	}

//	public async Task KeepAlive() => await RefreshTokenIfNeeded();

//	#region Authentication

//	#region Login
//	public async Task LogIn()
//	{
//		var csrf = await GetCsrfToken();
//		LoginDto dto = new()
//		{
//			Csrf = csrf,
//			UserName =_options.Username,
//			Password = _options.Password
//		};

//		var resp = await _http.PostAsync( "https://info-car.pl/oauth2/login", new FormUrlEncodedContent( dto ) );
//		if ( resp is not { StatusCode: HttpStatusCode.Found, Headers.Location.AbsoluteUri: "https://info-car.pl/new/" } )
//		{
//			throw new InfoCarPingException( "Invalid credentials" );
//		}

//		await GetTokenForSession();
//	}

//	private async Task<string> GetCsrfToken()
//	{
//		var loginFormResp = await _http.GetAsync( "https://info-car.pl/oauth2/login" );
//		var loginForm = await loginFormResp.Content.ReadAsStringAsync();
//		var csrfMatch = CsrfInputRx().Match( loginForm );
//		var csrfToken = csrfMatch.Groups[1].Value;
//		return csrfToken;
//	}

//	[GeneratedRegex( """<input type="hidden" name="_csrf" value="([\da-f\-]*)" />""" )]
//	private static partial Regex CsrfInputRx();

//	private async Task GetTokenForSession()
//	{
//		await FetchJwkKeysIfNeeded();

//		var sentState = Base64UrlEncoder.Encode( RandomNumberGenerator.GetBytes( 45 ) );
//		var sentNonce = sentState;

//		string uri = $"https://info-car.pl/oauth2/authorize?" +
//			$"response_type=id_token%20token" +
//			$"&client_id=client" +
//			$"&state={sentState}" +
//			$"&redirect_uri=https%3A%2F%2Finfo-car.pl%2Fnew%2Fassets%2Frefresh.html" +
//			$"&scope=openid%20profile%20email%20resource.read" +
//			$"&nonce={sentNonce}" +
//			$"&prompt=none";

//		var respMsg = await _http.GetAsync( uri );

//		ExtractToken( respMsg, sentState, sentNonce );
//	}

//	#endregion

//	#region Refresh token
//	private async Task RefreshToken()
//	{
//		await FetchJwkKeysIfNeeded();

//		var sentState = Base64UrlEncoder.Encode( RandomNumberGenerator.GetBytes( 45 ) );
//		var sentNonce = sentState;

//		var url = $"https://info-car.pl/oauth2/authorize?" +
//			$"response_type=id_token%20token" +
//			$"&client_id=client" +
//			$"&state={sentState}" +
//			$"&redirect_uri=https%3A%2F%2Finfo-car.pl%2Fnew%2Fassets%2Frefresh.html" +
//			$"&scope=openid%20profile%20email%20resource.read" +
//			$"&nonce={sentNonce}" +
//			$"&prompt=none";

//		HttpRequestMessage reqMsg = new( HttpMethod.Get, url )
//		{
//			Headers =
//			{
//				{ "Authorization", $"Bearer {_token}" },
//			}
//		};

//		var respMsg = await _http.SendAsync( reqMsg );

//		ExtractToken( respMsg, sentState, sentNonce );
//	}

//	private async Task RefreshTokenIfNeeded()
//	{
//		if ( _token is null )
//		{
//			throw new InfoCarPingException( "You must first log in." );
//		}

//		var now = DateTimeOffset.Now;
//		if ( _tokenValidTo - now < now - _tokenIssued || ( now - _tokenIssued ) > TimeSpan.FromMinutes( 7.5 ) )
//		{
//			await RefreshToken();
//		}
//	}

//	#endregion

//	#region Helpers
//	private void ExtractToken( HttpResponseMessage respMsg, string sentState, string sentNonce )
//	{
//		Debug.Assert( respMsg.StatusCode == HttpStatusCode.Found );
//		Debug.Assert( respMsg.Headers.Location is not null );

//		var fragment = respMsg.Headers.Location.Fragment.TrimStart( '#' );
//		var fragmentDictionary = HttpUtility.ParseQueryString( fragment );
//		var error = fragmentDictionary["error"];
//		if ( error is not null )
//		{
//			throw new InfoCarPingException( $"token error: {error}" );
//		}

//		var accessToken = fragmentDictionary["access_token"] ?? throw new InfoCarPingException( "No access_token received. It may be necessary to log in again." );
//		var receivedState = fragmentDictionary["state"] ?? throw new InfoCarPingException( "No state received. It may be necessary to log in again." );
//		var expiresInStr = fragmentDictionary["expires_in"] ?? throw new InfoCarPingException( "No expires_in received. It may be necessary to log in again." );
//		var idToken = fragmentDictionary["id_token"] ?? throw new InfoCarPingException( "No id_token received. It may be necessary to log in again." );

//		if ( receivedState != sentState )
//		{
//			throw new Exception( "Received state does not match." );
//		}

//		_jwtHandler.ValidateToken( idToken, _jwtValidationParameters, out var jwt );
//		var receivedNonce = ( (JwtSecurityToken)jwt ).Claims.FirstOrDefault( c => c.Type == "nonce" )?.Value;

//		if ( receivedNonce != sentNonce )
//		{
//			throw new Exception( "Received nonce does not match." );
//		}

//		var expiresIn = int.Parse( expiresInStr );
//		_tokenIssued = DateTimeOffset.Now;
//		_tokenValidTo = _tokenIssued.AddSeconds( expiresIn );
//		_token = accessToken;
//	}

//	private async Task FetchJwkKeys()
//	{
//		var keysResponseMsg = await _http.GetAsync( "https://info-car.pl/oauth2/jwk" );
//		var keysResponse = await keysResponseMsg.Content.ReadFromJsonAsync<JwkResponse>();
//		var keys = keysResponse!.Keys.Select( k => new RsaSecurityKey(
//			new RSAParameters()
//			{
//				Modulus = Base64UrlEncoder.DecodeBytes( k.N ),
//				Exponent = Base64UrlEncoder.DecodeBytes( k.E )
//			} )
//		{
//			KeyId = k.Kid
//		} ).ToArray();

//		_jwtValidationParameters.IssuerSigningKeys = keys;
//	}

//	private async Task FetchJwkKeysIfNeeded()
//	{
//		if ( _jwtValidationParameters.IssuerSigningKeys?.Any() is not true )
//		{
//			await FetchJwkKeys();
//		}
//	}

//	#endregion

//	#endregion

//	private bool IsWithinTime( ExamHourDto exam )
//	{
//		return exam.Time >= _options.MinExamTime && exam.Time <= _options.MaxExamTime;
//	}

//}

//public record ExamDayDto( DateOnly Day, ExamHourDto[] Exams );

//public record ExamHourDto( TimeOnly Time, int Places );
