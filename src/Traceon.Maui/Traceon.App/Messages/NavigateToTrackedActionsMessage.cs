using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Arisoul.Traceon.App.Messages;

public class NavigateToTrackedActionsMessage(IUnitOfWork unitOfWork, bool isSelectionMode, IEnumerable<Guid> fieldsToHide)
    : ValueChangedMessage<(IUnitOfWork, bool, IEnumerable<Guid>)>((unitOfWork, isSelectionMode, fieldsToHide))
{
}
