#region original

//HttpClient http = new();
//SheduleRequest req = new()
//{
//	Category = "B",
//	EndDate = "2023-10-04T08:49:50.186Z",
//	StartDate = "2023-08-03T08:49:50.186Z",
//	WordId = "42"
//};

//JsonContent reqCtnt = JsonContent.Create( req );

//while ( true )
//{
//	HttpRequestMessage reqMsg = new( HttpMethod.Put, "https://info-car.pl/api/word/word-centers/exam-schedule" )
//	{
//		Content = reqCtnt,
//		Headers =
//	{
//		{"Authorization", "Bearer eyJraWQiOiJpYy0xIiwiYWxnIjoiUlMyNTYifQ.eyJzdWIiOiJwcnplbXlzbGF3dXJiYW5lazFAZ21haWwuY29tIiwidWlkIjoiYmZlMDBlZGMtNzAzMi00MDM3LWIzOWMtNzFjYjZjZDVkZTI0Iiwic2NwIjpbImVtYWlsIiwib3BlbmlkIiwicHJvZmlsZSIsInJlc291cmNlLnJlYWQiXSwiaGFzQWNjIjp0cnVlLCJhenAiOiJjbGllbnQiLCJpc3MiOiJodHRwczpcL1wvaW5mby1jYXIucGxcL29hdXRoMlwvIiwiZXhwIjoxNjkwODkyMTE3LCJpYXQiOjE2OTA4ODg1MTcsImp0aSI6ImY4NWU1ZmQ2LWNmMWItNGFkZS1hYWYwLTVjYTc1MTkzNmNmNCIsImF1dGhvcml0aWVzIjpbIlJPTEVfVVNFUiJdfQ.Bp9aLkp89tNviJFL0B64T4jSNCuog6OJppB1Mu9FpDII4TCbVRReaiNwW4Z4TqsKjxib_pPLJCk56C7Kh7xH75DULBc78Lo_WZFjR9Qa3CfzRBpAPtCYAWxKjWhT-ipuWZcmYm1YnS6wKCZ7pIuECMsliKA8P-pvzWWUbI0Czu4" }
//		}
//	};
//	var respMsg = await http.SendAsync( reqMsg );
//	var resp = await respMsg.Content.ReadFromJsonAsync<SheduleResponse>();

//	var days = resp!.Schedule.ScheduledDays
//		.Select( d => new
//		{
//			d.Day,
//			Exams = d.ScheduledHours
//				.Select( h => new { h.Time, h.PracticeExams } )
//				.Where( h => h.PracticeExams.Sum( e => e.Places ) > 0 )
//				.ToArray()
//		} )
//		.Where( d => d.Exams.Length > 0 )
//		.Where( d => DateTime.Parse( d.Day ) - DateTime.Now < TimeSpan.FromDays( 21 ) )
//		.ToArray();

//	Console.WriteLine( $"Running at {DateTime.Now:g}" );
//	foreach ( var day in days )
//	{
//		Console.WriteLine( $"{day.Day}:" );
//		foreach ( var exam in day.Exams )
//		{
//			Console.WriteLine( $"  {exam.Time}: {exam.PracticeExams.Sum( e => e.Places )}" );
//		}
//	}
//	Console.WriteLine();

//	await Task.Delay( 60_000 );
//}

#endregion
