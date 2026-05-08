using Microsoft.JSInterop;

namespace Traceon.Blazor.Services;

/// <summary>
/// Bridge between ApexCharts' raw <c>chart.events.dataPointSelection</c> JS callback
/// and per-renderer .NET handlers, keyed by chart id. The package's typed
/// <c>OnDataPointSelection</c> event is broken on chart shapes we use (it dereferences
/// a null internal series list); we drive clicks through this registry instead.
/// </summary>
public static class ChartClickRegistry
{
    private static readonly Dictionary<Guid, Action<int, int>> _handlers = [];

    public static void Register(Guid chartId, Action<int, int> handler)
        => _handlers[chartId] = handler;

    public static void Unregister(Guid chartId)
        => _handlers.Remove(chartId);

    [JSInvokable]
    public static void OnApexChartClick(string chartId, int seriesIndex, int dataPointIndex)
    {
        if (Guid.TryParse(chartId, out var id) && _handlers.TryGetValue(id, out var handler))
            handler(seriesIndex, dataPointIndex);
    }
}
