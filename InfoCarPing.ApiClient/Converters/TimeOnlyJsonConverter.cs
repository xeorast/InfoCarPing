using System.Text.Json;
using System.Text.Json.Serialization;

namespace InfoCarPing.ApiClient.Converters;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
	public override TimeOnly Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		var token = reader.GetString() ?? throw new JsonException( "Time can not be null." );
		if ( TimeOnly.TryParse( token, out var time ) )
		{
			return time;
		}

		throw new JsonException( $"'{token}' is not valid time format." );
	}

	public override void Write( Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options )
	{
		var token = value.ToLongTimeString();
		writer.WriteStringValue( token );
	}
}
