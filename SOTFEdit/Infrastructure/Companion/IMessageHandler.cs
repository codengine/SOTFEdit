using System;
using SOTFEdit.Companion.Shared.Messages;

namespace SOTFEdit.Infrastructure.Companion;

public interface IMessageHandler
{
    Type MessageType { get; }
    public void Handle(ICompanionMessage companionMessage);
}