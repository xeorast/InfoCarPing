using System.Text.Json.Serialization;

namespace InfoCarPing.Contract;

#nullable disable warnings

internal class SheduleResponse
{
    //[JsonPropertyName( "organizationId" )]
    //public string OrganizationId { get; set; }

    //[JsonPropertyName( "isOskVehicleReservationEnabled" )]
    //public bool IsOskVehicleReservationEnabled { get; set; }

    //[JsonPropertyName( "isRescheduleReservation" )]
    //public bool IsRescheduleReservation { get; set; }

    //[JsonPropertyName( "category" )]
    //public string Category { get; set; }

    [JsonPropertyName("schedule")]
    public Schedule Schedule { get; set; }
}

internal class Schedule
{
    [JsonPropertyName("scheduledDays")]
    public Scheduledday[] ScheduledDays { get; set; }
}

internal class Scheduledday
{
    [JsonPropertyName("day")]
    public DateOnly Day { get; set; }

    [JsonPropertyName("scheduledHours")]
    public Scheduledhour[] ScheduledHours { get; set; }
}

internal class Scheduledhour
{
    [JsonPropertyName("time")]
    public TimeOnly Time { get; set; }

    //[JsonPropertyName( "theoryExams" )]
    //public ExamData[] TheoryExams { get; set; }

    [JsonPropertyName("practiceExams")]
    public ExamData[] PracticeExams { get; set; }

    //[JsonPropertyName( "linkedExamsDto" )]
    //public ExamData[] LinkedExamsDto { get; set; }
}

internal class ExamData
{
    //[JsonPropertyName( "id" )]
    //public string Id { get; set; }

    [JsonPropertyName("places")]
    public int Places { get; set; }

    //[JsonPropertyName( "date" )]
    //public DateTimeOffset Date { get; set; }

    //[JsonPropertyName( "amount" )]
    //public int Amount { get; set; }

    //[JsonPropertyName( "additionalInfo" )]
    //public object AdditionalInfo { get; set; }
}
