using System;

namespace LlmCommon.Dtos
{
    public class User
    {
        public User(Ids.Id id, string name, bool isAdmin = false)
        {
            Id = id;
            Name = name;
            IsAdmin = isAdmin;
        }
        public Ids.Id Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }

        public static readonly User Empty = new User(Ids.Empty, "anonymouse");
    }
}
