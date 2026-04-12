using Traceon.Contracts.Enums;

namespace Traceon.Infrastructure.Onboarding;

public sealed class ActionTemplate
{
    public required string Name { get; init; }
    public required string NameKey { get; init; }
    public string? Description { get; init; }
    public string? DescriptionKey { get; init; }
    public required List<FieldTemplate> Fields { get; init; }
    public List<AnalyticsRuleTemplate> AnalyticsRules { get; init; } = [];
}

public sealed class FieldTemplate
{
    public required string Name { get; init; }
    public required string NameKey { get; init; }
    public required FieldType Type { get; init; }
    public string? Unit { get; init; }
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public bool IsRequired { get; init; }
    public string? DropdownValues { get; init; }
    public string? DropdownValuesKey { get; init; }
    public string? DefaultValue { get; init; }
    public decimal? TargetValue { get; init; }
    public int Order { get; init; }

    /// <summary>Name of a sibling numeric field to use as the value axis for dropdown trend charts.</summary>
    public string? DropdownTrendValueFieldName { get; init; }
    /// <summary>Aggregation for the dropdown trend chart (matches TrendAggregation enum). Default: Sum (4).</summary>
    public int DropdownTrendAggregation { get; init; } = 4;
    /// <summary>Chart type for the dropdown trend (matches TrendChartType enum). Default: Line (0).</summary>
    public int DropdownTrendChartType { get; init; }
}

/// <summary>
/// Defines a cross-field analytics rule to be created when a template is installed.
/// Field references use the field Name (matched after creation).
/// </summary>
public sealed class AnalyticsRuleTemplate
{
    /// <summary>Name of the measure field (must match a FieldTemplate.Name in the same action).</summary>
    public required string MeasureFieldName { get; init; }
    /// <summary>Name of the group-by field.</summary>
    public required string GroupByFieldName { get; init; }
    /// <summary>Name of the sign field (for SignedSum). Null if not applicable.</summary>
    public string? SignFieldName { get; init; }
    /// <summary>Comma-separated values from the sign field that should negate the measure.</summary>
    public string? NegativeValues { get; init; }
    /// <summary>Name of the filter field. Null if no filter.</summary>
    public string? FilterFieldName { get; init; }
    /// <summary>Value to filter by (matched against the filter field's entry value).</summary>
    public string? FilterValue { get; init; }
    /// <summary>Aggregation type (matches AnalyticsAggregation enum int value).</summary>
    public int Aggregation { get; init; }
    /// <summary>Display type (matches AnalyticsDisplayType enum int value).</summary>
    public int DisplayType { get; init; } = 1;
    /// <summary>Optional label for the rule.</summary>
    public string? Label { get; init; }
    /// <summary>Optional label localization key.</summary>
    public string? LabelKey { get; init; }
    public int SortOrder { get; init; }
}

public sealed class TagTemplate
{
    public required string Name { get; init; }
    public string? NameKey { get; init; }
    public required string Color { get; init; }
}

public sealed class TemplatePack
{
    public required string Id { get; init; }
    public required string NameKey { get; init; }
    public required string DescriptionKey { get; init; }
    public required string Icon { get; init; }
    public required string Color { get; init; }
    public required List<TagTemplate> Tags { get; init; }
    public required List<ActionTemplate> Actions { get; init; }
}

public static class TemplatePackCatalog
{
    public static IReadOnlyList<TemplatePack> All => _packs;

    public static TemplatePack? GetById(string id) =>
        _packs.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    private static readonly List<TemplatePack> _packs =
    [
        new TemplatePack
        {
            Id = "health",
            NameKey = "Template_Health_Name",
            DescriptionKey = "Template_Health_Desc",
            Icon = "bi-heart-pulse",
            Color = "#dc3545",
            Tags = [new() { Name = "Health", NameKey = "Template_Tag_Health", Color = "#dc3545" }, new() { Name = "Vitals", NameKey = "Template_Tag_Vitals", Color = "#e35d6a" }],
            Actions =
            [
                new ActionTemplate
                {
                    Name = "Blood Pressure", NameKey = "Template_Health_BloodPressure",
                    Description = "Track systolic, diastolic and pulse.", DescriptionKey = "Template_Health_BloodPressure_Desc",
                    Fields =
                    [
                        new() { Name = "Systolic", NameKey = "Template_Field_Systolic", Type = FieldType.Integer, Unit = "mmHg", MinValue = 60, MaxValue = 250, IsRequired = true, TargetValue = 120, Order = 0 },
                        new() { Name = "Diastolic", NameKey = "Template_Field_Diastolic", Type = FieldType.Integer, Unit = "mmHg", MinValue = 40, MaxValue = 150, IsRequired = true, TargetValue = 80, Order = 1 },
                        new() { Name = "Pulse", NameKey = "Template_Field_Pulse", Type = FieldType.Integer, Unit = "bpm", MinValue = 30, MaxValue = 220, Order = 2 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Weight", NameKey = "Template_Health_Weight",
                    Description = "Track your body weight over time.", DescriptionKey = "Template_Health_Weight_Desc",
                    Fields =
                    [
                        new() { Name = "Weight", NameKey = "Template_Field_Weight", Type = FieldType.Decimal, Unit = "kg", MinValue = 20, MaxValue = 300, IsRequired = true, Order = 0 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Blood Glucose", NameKey = "Template_Health_BloodGlucose",
                    Description = "Monitor blood sugar levels.", DescriptionKey = "Template_Health_BloodGlucose_Desc",
                    Fields =
                    [
                        new() { Name = "Glucose", NameKey = "Template_Field_Glucose", Type = FieldType.Decimal, Unit = "mg/dL", MinValue = 20, MaxValue = 600, IsRequired = true, Order = 0 },
                        new() { Name = "Meal", NameKey = "Template_Field_Meal", Type = FieldType.Dropdown, DropdownValues = "Fasting|Before meal|After meal|Bedtime", DropdownValuesKey = "Template_Dropdown_Meal", Order = 1, DropdownTrendValueFieldName = "Glucose" },
                    ]
                },
            ]
        },
        new TemplatePack
        {
            Id = "fitness",
            NameKey = "Template_Fitness_Name",
            DescriptionKey = "Template_Fitness_Desc",
            Icon = "bi-trophy",
            Color = "#fd7e14",
            Tags = [new() { Name = "Fitness", NameKey = "Template_Tag_Fitness", Color = "#fd7e14" }, new() { Name = "Exercise", NameKey = "Template_Tag_Exercise", Color = "#e8590c" }],
            Actions =
            [
                new ActionTemplate
                {
                    Name = "Workout", NameKey = "Template_Fitness_Workout",
                    Description = "Track exercise type, duration and calories.", DescriptionKey = "Template_Fitness_Workout_Desc",
                    Fields =
                    [
                        new() { Name = "Workout Type", NameKey = "Template_Field_WorkoutType", Type = FieldType.Dropdown, DropdownValues = "Strength|Cardio|HIIT|Yoga|Swimming|Cycling|Other", DropdownValuesKey = "Template_Dropdown_WorkoutType", IsRequired = true, Order = 0, DropdownTrendValueFieldName = "Duration" },
                        new() { Name = "Duration", NameKey = "Template_Field_Duration", Type = FieldType.Integer, Unit = "min", MinValue = 1, MaxValue = 600, IsRequired = true, Order = 1 },
                        new() { Name = "Calories", NameKey = "Template_Field_Calories", Type = FieldType.Integer, Unit = "kcal", MinValue = 0, MaxValue = 5000, Order = 2 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Running", NameKey = "Template_Fitness_Running",
                    Description = "Log distance and time.", DescriptionKey = "Template_Fitness_Running_Desc",
                    Fields =
                    [
                        new() { Name = "Distance", NameKey = "Template_Field_Distance", Type = FieldType.Decimal, Unit = "km", MinValue = 0, MaxValue = 200, IsRequired = true, Order = 0 },
                        new() { Name = "Duration", NameKey = "Template_Field_Duration", Type = FieldType.Integer, Unit = "min", MinValue = 1, MaxValue = 600, IsRequired = true, Order = 1 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Steps", NameKey = "Template_Fitness_Steps",
                    Description = "Count your daily steps.", DescriptionKey = "Template_Fitness_Steps_Desc",
                    Fields =
                    [
                        new() { Name = "Steps", NameKey = "Template_Field_Steps", Type = FieldType.Integer, Unit = "steps", MinValue = 0, MaxValue = 100000, IsRequired = true, TargetValue = 10000, Order = 0 },
                    ]
                },
            ]
        },
        new TemplatePack
        {
            Id = "habits",
            NameKey = "Template_Habits_Name",
            DescriptionKey = "Template_Habits_Desc",
            Icon = "bi-calendar-check",
            Color = "#198754",
            Tags = [new() { Name = "Habits", NameKey = "Template_Tag_Habits", Color = "#198754" }, new() { Name = "Wellness", NameKey = "Template_Tag_Wellness", Color = "#20c997" }],
            Actions =
            [
                new ActionTemplate
                {
                    Name = "Sleep", NameKey = "Template_Habits_Sleep",
                    Description = "Track hours and quality of sleep.", DescriptionKey = "Template_Habits_Sleep_Desc",
                    Fields =
                    [
                        new() { Name = "Hours", NameKey = "Template_Field_Hours", Type = FieldType.Decimal, Unit = "h", MinValue = 0, MaxValue = 24, IsRequired = true, TargetValue = 8, Order = 0 },
                        new() { Name = "Quality", NameKey = "Template_Field_Quality", Type = FieldType.Dropdown, DropdownValues = "Poor|Fair|Good|Excellent", DropdownValuesKey = "Template_Dropdown_Quality", Order = 1, DropdownTrendValueFieldName = "Hours" },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Water Intake", NameKey = "Template_Habits_Water",
                    Description = "Count glasses of water per day.", DescriptionKey = "Template_Habits_Water_Desc",
                    Fields =
                    [
                        new() { Name = "Glasses", NameKey = "Template_Field_Glasses", Type = FieldType.Integer, Unit = "glasses", MinValue = 0, MaxValue = 30, IsRequired = true, TargetValue = 8, Order = 0 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Reading", NameKey = "Template_Habits_Reading",
                    Description = "Log pages or minutes read.", DescriptionKey = "Template_Habits_Reading_Desc",
                    Fields =
                    [
                        new() { Name = "Pages", NameKey = "Template_Field_Pages", Type = FieldType.Integer, Unit = "pages", MinValue = 0, MaxValue = 1000, Order = 0 },
                        new() { Name = "Duration", NameKey = "Template_Field_Duration", Type = FieldType.Integer, Unit = "min", MinValue = 0, MaxValue = 600, Order = 1 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Meditation", NameKey = "Template_Habits_Meditation",
                    Description = "Track meditation sessions.", DescriptionKey = "Template_Habits_Meditation_Desc",
                    Fields =
                    [
                        new() { Name = "Duration", NameKey = "Template_Field_Duration", Type = FieldType.Integer, Unit = "min", MinValue = 1, MaxValue = 180, IsRequired = true, Order = 0 },
                    ]
                },
            ]
        },
        new TemplatePack
        {
            Id = "finance",
            NameKey = "Template_Finance_Name",
            DescriptionKey = "Template_Finance_Desc",
            Icon = "bi-wallet2",
            Color = "#0d6efd",
            Tags = [new() { Name = "Finance", NameKey = "Template_Tag_Finance", Color = "#0d6efd" }],
            Actions =
            [
                new ActionTemplate
                {
                    Name = "Transaction", NameKey = "Template_Finance_Transaction",
                    Description = "Track income and expenses with a running balance.", DescriptionKey = "Template_Finance_Transaction_Desc",
                    Fields =
                    [
                        new() { Name = "Amount", NameKey = "Template_Field_Amount", Type = FieldType.Decimal, Unit = "€", MinValue = 0, IsRequired = true, Order = 0 },
                        new() { Name = "Type", NameKey = "Template_Field_TransactionType", Type = FieldType.Dropdown, DropdownValues = "Income|Expense", DropdownValuesKey = "Template_Dropdown_TransactionType", IsRequired = true, Order = 1 },
                        new() { Name = "Category", NameKey = "Template_Field_Category", Type = FieldType.Dropdown, DropdownValues = "Salary|Freelance|Food|Transport|Housing|Health|Entertainment|Shopping|Investment|Gift|Other", DropdownValuesKey = "Template_Dropdown_FinanceCategory", IsRequired = true, Order = 2, DropdownTrendValueFieldName = "Amount" },
                    ],
                    AnalyticsRules =
                    [
                        new() { MeasureFieldName = "Amount", GroupByFieldName = "Category", SignFieldName = "Type", NegativeValues = "Expense", Aggregation = 5 /* SignedSum */, DisplayType = 0 /* Table */, Label = "Balance by Category", LabelKey = "Template_Rule_BalanceByCategory", SortOrder = 0 },
                        new() { MeasureFieldName = "Amount", GroupByFieldName = "Type", Aggregation = 0 /* Sum */, DisplayType = 1 /* BarChart */, Label = "Total by Type", LabelKey = "Template_Rule_TotalByType", SortOrder = 1 },
                        new() { MeasureFieldName = "Amount", GroupByFieldName = "Category", FilterFieldName = "Type", FilterValue = "Expense", Aggregation = 0 /* Sum */, DisplayType = 2 /* PieChart */, Label = "Spending by Category", LabelKey = "Template_Rule_SpendingByCategory", SortOrder = 2 },
                        new() { MeasureFieldName = "Amount", GroupByFieldName = "Category", FilterFieldName = "Type", FilterValue = "Income", Aggregation = 0 /* Sum */, DisplayType = 2 /* PieChart */, Label = "Income by Category", LabelKey = "Template_Rule_IncomeByCategory", SortOrder = 3 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Expense", NameKey = "Template_Finance_Expense",
                    Description = "Log spending with categories.", DescriptionKey = "Template_Finance_Expense_Desc",
                    Fields =
                    [
                        new() { Name = "Amount", NameKey = "Template_Field_Amount", Type = FieldType.Decimal, Unit = "€", MinValue = 0, IsRequired = true, Order = 0 },
                        new() { Name = "Category", NameKey = "Template_Field_Category", Type = FieldType.Dropdown, DropdownValues = "Food|Transport|Housing|Health|Entertainment|Shopping|Other", DropdownValuesKey = "Template_Dropdown_ExpenseCategory", IsRequired = true, Order = 1, DropdownTrendValueFieldName = "Amount" },
                    ],
                    AnalyticsRules =
                    [
                        new() { MeasureFieldName = "Amount", GroupByFieldName = "Category", Aggregation = 0 /* Sum */, DisplayType = 1 /* BarChart */, Label = "Spending by Category", LabelKey = "Template_Rule_SpendingByCategory", SortOrder = 0 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Income", NameKey = "Template_Finance_Income",
                    Description = "Record income from various sources.", DescriptionKey = "Template_Finance_Income_Desc",
                    Fields =
                    [
                        new() { Name = "Amount", NameKey = "Template_Field_Amount", Type = FieldType.Decimal, Unit = "€", MinValue = 0, IsRequired = true, Order = 0 },
                        new() { Name = "Source", NameKey = "Template_Field_Source", Type = FieldType.Dropdown, DropdownValues = "Salary|Freelance|Investment|Gift|Other", DropdownValuesKey = "Template_Dropdown_IncomeSource", Order = 1, DropdownTrendValueFieldName = "Amount" },
                    ],
                    AnalyticsRules =
                    [
                        new() { MeasureFieldName = "Amount", GroupByFieldName = "Source", Aggregation = 0 /* Sum */, DisplayType = 2 /* PieChart */, Label = "Income by Source", LabelKey = "Template_Rule_IncomeBySource", SortOrder = 0 },
                    ]
                },
            ]
        },
    ];
}
