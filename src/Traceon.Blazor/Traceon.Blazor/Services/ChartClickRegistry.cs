using Microsoft.JSInterop;

namespace Traceon.Blazor.Services;

/// <summary>
/// Bridge between ApexCharts' raw <c>chart.events.dataPointSelection</c> JS callback
/// and per-renderer .NET handlers, keyed by chart id. The package's typed
/// <c>OnDataPointSelection</c> event is broken on chart shapes we use (it dereferences
/// a null internal series list); we drive clicks through this registry instead.
///
/// Keys are arbitrary strings so callers can namespace handlers when multiple charts
/// share a source id (e.g. a SignedSum rule produces both a cross-field table AND a
/// running-balance area chart — keyed as "cf-{id}" and "bal-{id}" respectively).
/// </summary>
public static class ChartClickRegistry
{
    private static readonly Dictionary<string, Action<int, int>> _handlers = [];

    public static void Register(string chartId, Action<int, int> handler)
        => _handlers[chartId] = handler;

    public static void Unregister(string chartId)
        => _handlers.Remove(chartId);

    public static void Register(Guid chartId, Action<int, int> handler)
        => Register(chartId.ToString(), handler);

    public static void Unregister(Guid chartId)
        => Unregister(chartId.ToString());

    [JSInvokable]
    public static void OnApexChartClick(string chartId, int seriesIndex, int dataPointIndex)
    {
        if (_handlers.TryGetValue(chartId, out var handler))
            handler(seriesIndex, dataPointIndex);
    }
}
