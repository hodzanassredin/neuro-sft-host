﻿using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class UserJoinEvent : Event
    {
        public Ids.Id ChatId { get; set; }

        public UserJoinEvent(Ids.Id chatId, User user)
        {
            ChatId = chatId;
            User = user;
        }

        public User User { get; set; }

        public override Task Accept(IEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
