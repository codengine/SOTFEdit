using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Companion.Shared.Messages;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Infrastructure.Companion;

public class CompanionNetworkPlayerManager : MessageHandler<CompanionNetworkPlayerUpdateMessage>
{
    public readonly HashSet<int> InstanceIds = new();

    protected override void Handle(CompanionNetworkPlayerUpdateMessage message)
    {
        foreach (var i in message.Added ?? Enumerable.Empty<int>())
        {
            InstanceIds.Add(i);
        }

        foreach (var i in message.Deleted ?? Enumerable.Empty<int>())
        {
            InstanceIds.Remove(i);
        }

        PoiMessenger.Instance.Send(message);
    }
}