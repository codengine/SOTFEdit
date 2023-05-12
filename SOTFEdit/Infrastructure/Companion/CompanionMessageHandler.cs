using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using NLog;
using SOTFEdit.Companion.Shared.Messages;
using WatsonWebsocket;

namespace SOTFEdit.Infrastructure.Companion;

public class CompanionMessageHandler
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<Type, IMessageHandler> _messageHandlers;

    public CompanionMessageHandler(IEnumerable<IMessageHandler> messageHandlers)
    {
        _messageHandlers = messageHandlers.ToDictionary(handler => handler.MessageType);
    }

    public void Handle(object? sender, MessageReceivedEventArgs e)
    {
        var message = MessagePackSerializer.Deserialize<ICompanionMessage>(e.Data);
        var messageType = message.GetType();

        if (!_messageHandlers.TryGetValue(messageType, out var handler))
        {
            Logger.Warn($"No message handler defined for type {messageType}");
            return;
        }

        handler.Handle(message);
    }
}