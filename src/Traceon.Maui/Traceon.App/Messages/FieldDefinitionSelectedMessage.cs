using Arisoul.Traceon.Maui.Core.Entities;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Arisoul.Traceon.App.Messages;

public class FieldDefinitionSelectedMessage(FieldDefinition value) 
    : ValueChangedMessage<FieldDefinition>(value)
{
}
