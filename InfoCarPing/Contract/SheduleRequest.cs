using System.Text.Json.Serialization;

namespace InfoCarPing.Contract;

internal class SheduleRequest
{
    [JsonPropertyName("category")]
    public required string Category { get; set; }

    [JsonPropertyName("wordId")]
    [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public required int WordId { get; set; }

    [JsonPropertyName("startDate")]
    public required DateTimeOffset StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public required DateTimeOffset EndDate { get; set; }
}
