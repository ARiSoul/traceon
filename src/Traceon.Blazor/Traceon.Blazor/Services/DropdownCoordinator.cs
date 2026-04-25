namespace Traceon.Blazor.Services;

public sealed class DropdownCoordinator
{
    private readonly Dictionary<Guid, Action> _registrations = [];

    public Guid Register(Action close)
    {
        var token = Guid.NewGuid();
        _registrations[token] = close;
        return token;
    }

    public void Unregister(Guid token) => _registrations.Remove(token);

    public void NotifyOpened(Guid openedToken)
    {
        foreach (var (token, close) in _registrations)
        {
            if (token != openedToken)
                close();
        }
    }
}
