using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Serialization
{
    public static class IndianStandardTimeJsonOptionsExtensions
    {
        private static readonly TimeZoneInfo IndianStandardTimeZone = ResolveIndianStandardTimeZone();

        public static void AddIndianStandardTimeConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new IndianStandardTimeDateTimeJsonConverter());
            options.Converters.Add(new IndianStandardTimeNullableDateTimeJsonConverter());
        }

        internal static DateTime ConvertUtcStorageToIst(DateTime value)
        {
            if (value == default)
            {
                return value;
            }

            var utcValue = value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utcValue, IndianStandardTimeZone);
        }

        internal static DateTime ParseIncomingDateTime(string rawValue)
        {
            if (DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
            {
                return dto.UtcDateTime;
            }

            if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
            {
                return dateTime.Kind switch
                {
                    DateTimeKind.Utc => dateTime,
                    DateTimeKind.Local => dateTime.ToUniversalTime(),
                    _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                };
            }

            throw new JsonException($"Invalid date/time value: {rawValue}");
        }

        private static TimeZoneInfo ResolveIndianStandardTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
        }
    }

    public sealed class IndianStandardTimeDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Date/time values must be sent as strings.");
            }

            var rawValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                throw new JsonException("Date/time value is required.");
            }

            return IndianStandardTimeJsonOptionsExtensions.ParseIncomingDateTime(rawValue);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var istValue = IndianStandardTimeJsonOptionsExtensions.ConvertUtcStorageToIst(value);
            writer.WriteStringValue(istValue.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
        }
    }

    public sealed class IndianStandardTimeNullableDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Date/time values must be sent as strings.");
            }

            var rawValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            return IndianStandardTimeJsonOptionsExtensions.ParseIncomingDateTime(rawValue);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            var istValue = IndianStandardTimeJsonOptionsExtensions.ConvertUtcStorageToIst(value.Value);
            writer.WriteStringValue(istValue.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
        }
    }
}
