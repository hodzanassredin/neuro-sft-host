using System;

namespace LlmCommon.Dtos
{
    public class User 
    {
        public Ids.Id Id { get; set; }
        public string Name { get; set; }

        public static readonly User Empty = new User() { Id = Ids.Empty, Name = "anonymouse"};
    }
}
