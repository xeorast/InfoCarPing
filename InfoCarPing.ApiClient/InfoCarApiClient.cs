using InfoCarPing.ApiClient.Contract;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;

namespace InfoCarPing.ApiClient;

public partial class InfoCarApiClient
{
	private readonly InfoCarClientApiOptions _options;
	private readonly HttpClient _http;
	private readonly JwtSecurityTokenHandler _jwtHandler;
	private readonly TokenValidationParameters _jwtValidationParameters;
	private readonly ICOpenIdConnectClient _oidcClient;

	public InfoCarApiClient( InfoCarClientApiOptions options )
	{
		_options = options;
		_http = new( new HttpClientHandler()
		{
			AllowAutoRedirect = false,
			CookieContainer = new()
		} );
		_jwtHandler = new();
		_jwtValidationParameters = new()
		{
			ValidIssuer = "https://info-car.pl/oauth2/",
			ValidAudience = "client",
		};
		_oidcClient = new( new() { JwtValidationParameters = _jwtValidationParameters }, _http );
	}

	public async Task LogIn() => await _oidcClient.LogIn( _options.Username, _options.Password );

	IAsyncPolicy<HttpResponseMessage>? logInAndRetryHttpPolicy;
	IAsyncPolicy<HttpResponseMessage>? waitAndRetryHttpPolicy;
	IAsyncPolicy<HttpResponseMessage>? wrappedHttpPolicy;
	public async Task<ExamDayDto[]> GetExams( string category, int wordId, TimeSpan maxWaitingTime )
	{
		await EnsureSession();

		logInAndRetryHttpPolicy ??= HttpPolicyExtensions
			.HandleTransientHttpError()
			.RetryAsync( 1, ( token, n ) => LogIn() );

		waitAndRetryHttpPolicy ??= Policy<HttpResponseMessage>
			.Handle<TransientPingException>()
			.WaitAndRetryAsync( 3, i => i * TimeSpan.FromSeconds( 10 ) );

		wrappedHttpPolicy ??= Policy.WrapAsync( waitAndRetryHttpPolicy, logInAndRetryHttpPolicy );

		var respMsg = await wrappedHttpPolicy.ExecuteAsync( () => FetchExamShedule( category, wordId ) );

		var resp = await respMsg.Content.ReadFromJsonAsync<SheduleResponse>();

		var days = resp!.Schedule.ScheduledDays
			.Select( d => new ExamDayDto(
				d.Day,
				d.ScheduledHours
					.Select( h => new ExamHourDto( h.Time, h.PracticeExams.Sum( e => e.Places ) ) )
					.Where( h => h.Places > 0 )
					.Where( IsWithinTime )
					.ToArray()
			) )
			.Where( d => d.Exams.Length > 0 )
			.Where( d => d.Day.ToDateTime( TimeOnly.MinValue ) - DateTimeOffset.Now < maxWaitingTime )
			.ToArray();

		return days;
	}

	private bool IsWithinTime( ExamHourDto exam )
	{
		return exam.Time >= _options.MinExamTime && exam.Time <= _options.MaxExamTime;
	}

	private async Task<HttpResponseMessage> FetchExamShedule( string category, int wordId )
	{
		var startDate = DateTimeOffset.UtcNow.AddDays( 2 );
		var endDate = startDate.AddDays( 2 * 31 );

		SheduleRequest req = new()
		{
			Category = category,
			WordId = wordId,
			StartDate = startDate,
			EndDate = endDate,
		};

		HttpRequestMessage reqMsg = new( HttpMethod.Put, "https://info-car.pl/api/word/word-centers/exam-schedule" )
		{
			Content = JsonContent.Create( req ),
			Headers =
			{
				{ "Authorization", $"Bearer {_oidcClient.Token}" }
			}
		};

		var respMsg = await _http.SendAsync( reqMsg );
		if ( respMsg
			is { StatusCode: HttpStatusCode.Found, Headers.Location.Authority: "maintenance.info-car.pl" }
			or { StatusCode: HttpStatusCode.ServiceUnavailable } )
		{
			throw new TransientPingException( "Service reported maintenance break. If this error is not gone in two minutes, restarting the application may help (but probably won't)." );
		}

		return respMsg;
	}

	public async Task KeepAlive() => await EnsureSession();

	Polly.Retry.AsyncRetryPolicy? logInAndRetryPolicy;
	private async Task EnsureSession()
	{

		if ( !_oidcClient.HasLoggedIn )
		{
			await LogIn();
			return;
		}

		logInAndRetryPolicy ??= Policy
			.Handle<InfoCarAuthException>()
			.RetryAsync( 1, ( token, n ) => LogIn() );

		await logInAndRetryPolicy.ExecuteAsync( _oidcClient.EnsureSessionUpToDate );
	}

}

public record ExamDayDto( DateOnly Day, ExamHourDto[] Exams );

public record ExamHourDto( TimeOnly Time, int Places );
