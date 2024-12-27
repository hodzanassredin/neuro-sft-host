using System;

namespace LlmCommon.Dtos
{
    public class User 
    {
        public User(Ids.Id id, string name)
        {
            Id = id;
            Name = name;
        }
        public Ids.Id Id { get; set; }
        public string Name { get; set; }

        public static readonly User Empty = new User(Ids.Empty, "anonymouse");
    }
}
