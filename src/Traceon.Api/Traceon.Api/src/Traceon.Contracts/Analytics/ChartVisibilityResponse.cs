namespace Traceon.Contracts.Analytics;

public sealed record ChartVisibilityResponse(List<string> HiddenKeys);

public sealed record UpdateChartVisibilityRequest(List<string> HiddenKeys);
