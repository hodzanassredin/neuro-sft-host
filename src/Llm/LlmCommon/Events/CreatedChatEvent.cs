﻿using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class CreatedChatEvent : ChatEvent
    {
        public CreatedChatEvent(Ids.Id chatId, string name, User owner) : base(chatId)
        {

            Name = name;
            Owner = owner;
        }
        public string Name { get; set; }
        public User Owner { get; set; }

        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
