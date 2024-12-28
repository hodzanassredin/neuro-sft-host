﻿

using LlmCommon.Abstractions;

namespace LlmCommon.Commands.Chat
{
    public class AddChatCommand(string name) : Command
    {
        public string Name { get; } = name;

        public override Task Accept(IExecutor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ChangeChatCommand(Ids.Id chatId, string text) : Command
    {
        public Ids.Id ChatId { get; } = chatId;
        public string Text { get; } = text;
        public override Task Accept(IExecutor visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class RemoveChatCommand(Ids.Id chatId) : Command
    {
        public Ids.Id ChatId { get; } = chatId;
        public override Task Accept(IExecutor visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class RemoveMessageCommand(Ids.Id chatId, Ids.Id messageId) : Command
    {
        public Ids.Id ChatId { get; } = chatId;
        public Ids.Id MessageId { get; } = messageId;
        public override Task Accept(IExecutor visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class AddMessageCommand(Ids.Id chatId, string text) : Command
    {
        public Ids.Id ChatId { get; } = chatId;
        public string Text { get; } = text;
        public override Task Accept(IExecutor visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class ChangeMessageCommand(Ids.Id chatId, Ids.Id messageId, string text) : Command
    {
        public Ids.Id ChatId { get; } = chatId;
        public Ids.Id MessageId { get; } = messageId;
        public string Text { get; } = text;
        public override Task Accept(IExecutor visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class JoinCommand(Ids.Id chatId) : Command
    {
        public Ids.Id ChatId { get; } = chatId;
        public override Task Accept(IExecutor visitor)
        {
            return visitor.Visit(this);
        }
    }
    public class LeaveCommand(Ids.Id chatId) : Command
    {
        public Ids.Id ChatId { get; } = chatId;
        public override Task Accept(IExecutor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
