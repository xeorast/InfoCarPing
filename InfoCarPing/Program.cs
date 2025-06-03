using InfoCarPing.ApiClient;
using System.Text.Json;

internal class Program
{
	private static async Task Main()
	{
		Console.WriteLine( "CarInfoPing by xeorast" );
		Console.WriteLine();

		var config = File.ReadAllText( "config.json" );
		var options = JsonSerializer.Deserialize<InfoCarClientApiOptions>( config )!;

		InfoCarApiClient infoCar = new( options );

		try
		{
			await infoCar.LogIn();
		}
		catch ( Exception e )
		{
			PrintException( e );
			Beep( 3 );
			Console.ReadKey();
			return;
		}

		while ( true )
		{
			ExamDayDto[] days;
			try
			{
				days = await infoCar.GetExams( options.Category, 42, options.MaxWaitingTime );
			}
			catch ( TransientPingException e )
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine( e.Message + Environment.NewLine );
				Console.ResetColor();
				await Task.Delay( Randomize( 30 * 1000, 10 * 1000 ) );
				continue;
			}
			catch ( Exception e )
			{
				PrintException( e );
				Beep( 3 );
				await Task.Delay( Randomize( 30 * 1000, 10 * 1000 ) );
				continue;
			}

			Console.WriteLine( $"Status z {DateTime.Now:g}:" );

			PrintExams( days );

			if ( days.Any() )
			{
				Beep( 4 );
			}

			Console.WriteLine();

			await Task.Delay( Randomize( 60 * 1000, 10 * 1000 ) );
		}
	}

	static void PrintException( Exception e )
	{
		Console.BackgroundColor = ConsoleColor.Red;
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine( e is InfoCarPingException ? e.Message : e );
		Console.WriteLine();
		Console.ResetColor();
	}

	static void PrintExams( ExamDayDto[] examDays )
	{
		Console.BackgroundColor = ConsoleColor.Green;
		Console.ForegroundColor = ConsoleColor.Black;

		foreach ( var day in examDays )
		{
			Console.WriteLine( $"{day.Day}:" );

			foreach ( var exam in day.Exams )
			{
				Console.WriteLine( $"  {exam.Time}: {exam.Places} miejsc" );
			}
		}

		Console.ResetColor();

		if ( examDays.Length == 0 )
		{
			Console.WriteLine( "  Brak wolnych miejsc." );
		}
	}

	static void Beep( int times )
	{
		for ( int i = 0; i < times; i++ )
		{
			Console.Beep();
		}
	}

	static int Randomize( int middle, int maxOffset )
	{
		var multiplier = Random.Shared.NextDouble() * 2 - 1; // -1 to 1
		return middle + (int)( maxOffset * multiplier );
	}
}