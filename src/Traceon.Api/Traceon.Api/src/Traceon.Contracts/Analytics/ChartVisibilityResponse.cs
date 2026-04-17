namespace Traceon.Contracts.Analytics;

public sealed record ChartVisibilityResponse(List<string> HiddenKeys, List<string> ChartOrder);

public sealed record UpdateChartVisibilityRequest(List<string> HiddenKeys);

public sealed record UpdateChartOrderRequest(List<string> ChartOrder);
