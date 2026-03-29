using Traceon.Contracts.Enums;

namespace Traceon.Infrastructure.Onboarding;

public sealed class ActionTemplate
{
    public required string Name { get; init; }
    public required string NameKey { get; init; }
    public required string? Description { get; init; }
    public required List<FieldTemplate> Fields { get; init; }
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
    public string? DefaultValue { get; init; }
    public decimal? TargetValue { get; init; }
    public int Order { get; init; }
}

public sealed class TagTemplate
{
    public required string Name { get; init; }
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
            Tags = [new() { Name = "Health", Color = "#dc3545" }, new() { Name = "Vitals", Color = "#e35d6a" }],
            Actions =
            [
                new ActionTemplate
                {
                    Name = "Blood Pressure", NameKey = "Template_Health_BloodPressure",
                    Description = "Track systolic, diastolic and pulse.",
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
                    Description = "Track your body weight over time.",
                    Fields =
                    [
                        new() { Name = "Weight", NameKey = "Template_Field_Weight", Type = FieldType.Decimal, Unit = "kg", MinValue = 20, MaxValue = 300, IsRequired = true, Order = 0 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Blood Glucose", NameKey = "Template_Health_BloodGlucose",
                    Description = "Monitor blood sugar levels.",
                    Fields =
                    [
                        new() { Name = "Glucose", NameKey = "Template_Field_Glucose", Type = FieldType.Decimal, Unit = "mg/dL", MinValue = 20, MaxValue = 600, IsRequired = true, Order = 0 },
                        new() { Name = "Meal", NameKey = "Template_Field_Meal", Type = FieldType.Dropdown, DropdownValues = "Fasting,Before meal,After meal,Bedtime", Order = 1 },
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
            Tags = [new() { Name = "Fitness", Color = "#fd7e14" }, new() { Name = "Exercise", Color = "#e8590c" }],
            Actions =
            [
                new ActionTemplate
                {
                    Name = "Workout", NameKey = "Template_Fitness_Workout",
                    Description = "Track exercise type, duration and calories.",
                    Fields =
                    [
                        new() { Name = "Workout Type", NameKey = "Template_Field_WorkoutType", Type = FieldType.Dropdown, DropdownValues = "Strength,Cardio,HIIT,Yoga,Swimming,Cycling,Other", IsRequired = true, Order = 0 },
                        new() { Name = "Duration", NameKey = "Template_Field_Duration", Type = FieldType.Integer, Unit = "min", MinValue = 1, MaxValue = 600, IsRequired = true, Order = 1 },
                        new() { Name = "Calories", NameKey = "Template_Field_Calories", Type = FieldType.Integer, Unit = "kcal", MinValue = 0, MaxValue = 5000, Order = 2 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Running", NameKey = "Template_Fitness_Running",
                    Description = "Log distance and time.",
                    Fields =
                    [
                        new() { Name = "Distance", NameKey = "Template_Field_Distance", Type = FieldType.Decimal, Unit = "km", MinValue = 0, MaxValue = 200, IsRequired = true, Order = 0 },
                        new() { Name = "Duration", NameKey = "Template_Field_Duration", Type = FieldType.Integer, Unit = "min", MinValue = 1, MaxValue = 600, IsRequired = true, Order = 1 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Steps", NameKey = "Template_Fitness_Steps",
                    Description = "Count your daily steps.",
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
            Tags = [new() { Name = "Habits", Color = "#198754" }, new() { Name = "Wellness", Color = "#20c997" }],
            Actions =
            [
                new ActionTemplate
                {
                    Name = "Sleep", NameKey = "Template_Habits_Sleep",
                    Description = "Track hours and quality of sleep.",
                    Fields =
                    [
                        new() { Name = "Hours", NameKey = "Template_Field_Hours", Type = FieldType.Decimal, Unit = "h", MinValue = 0, MaxValue = 24, IsRequired = true, TargetValue = 8, Order = 0 },
                        new() { Name = "Quality", NameKey = "Template_Field_Quality", Type = FieldType.Dropdown, DropdownValues = "Poor,Fair,Good,Excellent", Order = 1 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Water Intake", NameKey = "Template_Habits_Water",
                    Description = "Count glasses of water per day.",
                    Fields =
                    [
                        new() { Name = "Glasses", NameKey = "Template_Field_Glasses", Type = FieldType.Integer, Unit = "glasses", MinValue = 0, MaxValue = 30, IsRequired = true, TargetValue = 8, Order = 0 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Reading", NameKey = "Template_Habits_Reading",
                    Description = "Log pages or minutes read.",
                    Fields =
                    [
                        new() { Name = "Pages", NameKey = "Template_Field_Pages", Type = FieldType.Integer, Unit = "pages", MinValue = 0, MaxValue = 1000, Order = 0 },
                        new() { Name = "Duration", NameKey = "Template_Field_Duration", Type = FieldType.Integer, Unit = "min", MinValue = 0, MaxValue = 600, Order = 1 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Meditation", NameKey = "Template_Habits_Meditation",
                    Description = "Track meditation sessions.",
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
            Tags = [new() { Name = "Finance", Color = "#0d6efd" }],
            Actions =
            [
                new ActionTemplate
                {
                    Name = "Expense", NameKey = "Template_Finance_Expense",
                    Description = "Log spending with categories.",
                    Fields =
                    [
                        new() { Name = "Amount", NameKey = "Template_Field_Amount", Type = FieldType.Decimal, Unit = "€", MinValue = 0, IsRequired = true, Order = 0 },
                        new() { Name = "Category", NameKey = "Template_Field_Category", Type = FieldType.Dropdown, DropdownValues = "Food,Transport,Housing,Health,Entertainment,Shopping,Other", IsRequired = true, Order = 1 },
                    ]
                },
                new ActionTemplate
                {
                    Name = "Income", NameKey = "Template_Finance_Income",
                    Description = "Record income from various sources.",
                    Fields =
                    [
                        new() { Name = "Amount", NameKey = "Template_Field_Amount", Type = FieldType.Decimal, Unit = "€", MinValue = 0, IsRequired = true, Order = 0 },
                        new() { Name = "Source", NameKey = "Template_Field_Source", Type = FieldType.Dropdown, DropdownValues = "Salary,Freelance,Investment,Gift,Other", Order = 1 },
                    ]
                },
            ]
        },
    ];
}
