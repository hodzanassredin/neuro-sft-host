using System;

namespace LlmCommon.Dtos
{
    public class User 
    {
        public User(string id, string name)
        {
            Id = Ids.Parse(id);
            Name = name;
        }
        public Ids.Id Id { get; set; }
        public string Name { get; set; }

        public static readonly User Empty = new User(String.Empty, "anonymouse");
    }
}
