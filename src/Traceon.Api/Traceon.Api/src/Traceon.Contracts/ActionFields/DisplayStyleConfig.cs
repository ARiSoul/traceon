namespace Traceon.Contracts.ActionFields;

/// <summary>
/// Per-ActionField configuration that complements the chosen <see cref="Enums.DisplayStyle"/>.
/// All properties are optional; each style consumes the subset relevant to it (Rating uses
/// <see cref="Count"/> + <see cref="ValuePerIcon"/>, etc.).
/// </summary>
public sealed record DisplayStyleConfig(
    int? Count = null,
    decimal? ValuePerIcon = null,
    string? Icon = null,
    decimal? Step = null,
    string? OffIcon = null,
    string? OnColor = null,
    string? OffColor = null,
    int? Rows = null);
