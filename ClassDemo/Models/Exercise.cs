using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assignment3.Models
{
    public class Exercise
    {
        public int Id { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringToIntConverter))]
        public int Reps { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringToIntConverter))]
        public int Sets { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        // Foreign key to WorkoutPlan
        public int? RoutineId { get; set; }
        public Routine? Routine { get; set; }
    }

    public class JsonStringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (int.TryParse(stringValue, out int value))
                {
                    return value;
                }
                else if (string.IsNullOrEmpty(stringValue))
                {
                    return 0; // Default value for empty string
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            throw new JsonException($"Unable to convert {reader.GetString()} to {typeToConvert}");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
