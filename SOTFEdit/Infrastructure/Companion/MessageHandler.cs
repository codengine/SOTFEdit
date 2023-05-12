using System;
using SOTFEdit.Companion.Shared.Messages;

namespace SOTFEdit.Infrastructure.Companion;

public abstract class MessageHandler<T> : IMessageHandler where T : ICompanionMessage
{
    public void Handle(ICompanionMessage companionMessage)
    {
        if (companionMessage is not T typedMessage)
        {
            throw new ArgumentException($"Expected a message of type {typeof(T)}, but got {companionMessage.GetType()}",
                nameof(companionMessage));
        }

        Handle(typedMessage);
    }

    public Type MessageType => typeof(T);

    protected abstract void Handle(T message);
}