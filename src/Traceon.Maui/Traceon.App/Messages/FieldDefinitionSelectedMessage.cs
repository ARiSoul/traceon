using Arisoul.Traceon.Maui.Core.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Arisoul.Traceon.App.Messages;

public class FieldDefinitionSelectedMessage(FieldDefinition value) 
    : ValueChangedMessage<FieldDefinition>(value)
{
}
