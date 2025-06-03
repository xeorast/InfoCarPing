using InfoCarPing.ApiClient.Converters;
using System.Text.Json.Serialization;

namespace InfoCarPing.ApiClient;

public class InfoCarClientApiOptions
{
	public required string Category { get; set; }
	public required string Username { get; set; }
	public required string Password { get; set; }
	public required int MaxWaitingTimeInDays { get; set; }
    public required int WordId { get; set; }

    [JsonConverter( typeof( TimeOnlyJsonConverter ) )]
	public TimeOnly MinExamTime { get; set; } = new( 6, 0 );

	[JsonConverter( typeof( TimeOnlyJsonConverter ) )]
	public TimeOnly MaxExamTime { get; set; } = new( 21, 0 );

	public TimeSpan MaxWaitingTime => TimeSpan.FromDays( MaxWaitingTimeInDays );
}
