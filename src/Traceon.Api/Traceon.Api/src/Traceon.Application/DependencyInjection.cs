using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Traceon.Application.Services;

namespace Traceon.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ITrackedActionService>(ServiceLifetime.Scoped);

        services.AddScoped<ITrackedActionService, TrackedActionService>();
        services.AddScoped<IFieldDefinitionService, FieldDefinitionService>();
        services.AddScoped<IActionFieldService, ActionFieldService>();
        services.AddScoped<IActionEntryService, ActionEntryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IFieldAnalyticsRuleService, FieldAnalyticsRuleService>();
        services.AddScoped<IFieldDependencyRuleService, FieldDependencyRuleService>();

        return services;
    }
}
