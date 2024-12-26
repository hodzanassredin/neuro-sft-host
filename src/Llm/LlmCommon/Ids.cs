using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace LlmCommon
{
    public class Ids
    {
        public abstract class Directory
        {
            public abstract Id GenerateId();
        }
        public class StdDirectory : Ids.Directory
        {
            public override Ids.Id GenerateId()
            {
                return new Ids.Id(NanoidDotNet.Nanoid.Generate());
            }
        }

        public static Directory dir { get; private set; } = new StdDirectory();
        public static void SetDir(Directory d)
        {
            Debug.Assert(d != null);
            dir = d;
        }

        public static readonly Id Empty = new(String.Empty);

        //private static Random random = new(Guid.NewGuid().GetHashCode());
        //private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        //public static string GenerateId(int length = 7)
        //{
        //    var sb = new StringBuilder(length);
        //    for (int i = 0; i < length; i++)
        //    {
        //        sb.Append(chars[random.Next(chars.Length)]);
        //    }
        //    return sb.ToString();
        //}


        public static Id Parse(string? id)
        {
            if (id == null)
            {
                return Empty;
            }
            return new Id(id);
        }
        public class IdJsonConverter : JsonConverter<Ids.Id>
        {
            public override Ids.Id Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options) =>
                    Ids.Parse(reader.GetString());

            public override void Write(
                Utf8JsonWriter writer,
                Ids.Id dateTimeValue,
                JsonSerializerOptions options) =>
                    writer.WriteStringValue(dateTimeValue.ToString());
        }

        [JsonConverter(typeof(IdJsonConverter))]
        public class Id
        {

            [JsonInclude]
            private string id;
            public Id(string id)
            {
                ArgumentOutOfRangeException.ThrowIfEqual(id, null);
                this.id = id;
            }

            public override string ToString()
            {
                return id;
            }

            public bool Equals(Id kd)
            {
                return kd.id == id;
            }
            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj != null && obj is Id kd)
                    return Equals(kd);
                return false;
            }
            public override int GetHashCode()
            {
                return this.id.GetHashCode();
            }

            public static bool operator ==(Id? b1, Id? b2)
            {
                if (Object.ReferenceEquals(b1, null) || Object.ReferenceEquals(b2, null)) return Object.ReferenceEquals(b1, b2);
                return b1.Equals(b2);
            }

            public static bool operator !=(Id? b1, Id? b2)
            {
                return !(b1 == b2);
            }

        }
    }
}
