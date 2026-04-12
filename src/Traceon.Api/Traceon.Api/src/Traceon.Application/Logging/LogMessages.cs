using Microsoft.Extensions.Logging;

namespace Traceon.Application.Logging;

internal static partial class LogMessages
{
    // TrackedAction
    [LoggerMessage(Level = LogLevel.Information, Message = "Tracked action '{Name}' created with ID '{Id}' for user '{UserId}'.")]
    public static partial void TrackedActionCreated(this ILogger logger, string name, Guid id, string userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tracked action '{Id}' updated.")]
    public static partial void TrackedActionUpdated(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tracked action '{Id}' deleted.")]
    public static partial void TrackedActionDeleted(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tracked action '{Id}' restored.")]
    public static partial void TrackedActionRestored(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tracked action with ID '{Id}' was not found.")]
    public static partial void TrackedActionNotFound(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tracked action with name '{Name}' already exists for user '{UserId}'.")]
    public static partial void TrackedActionDuplicateName(this ILogger logger, string name, string userId);

    // FieldDefinition
    [LoggerMessage(Level = LogLevel.Information, Message = "Field definition '{DefaultName}' created with ID '{Id}'.")]
    public static partial void FieldDefinitionCreated(this ILogger logger, string defaultName, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Field definition '{Id}' updated.")]
    public static partial void FieldDefinitionUpdated(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Field definition '{Id}' deleted.")]
    public static partial void FieldDefinitionDeleted(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Field definition '{Id}' restored.")]
    public static partial void FieldDefinitionRestored(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Field definition with ID '{Id}' was not found.")]
    public static partial void FieldDefinitionNotFound(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Field definition '{Id}' cannot be deleted because it is in use.")]
    public static partial void FieldDefinitionInUse(this ILogger logger, Guid id);

    // ActionField
    [LoggerMessage(Level = LogLevel.Information, Message = "Action field '{Name}' created with ID '{Id}' for tracked action '{TrackedActionId}'.")]
    public static partial void ActionFieldCreated(this ILogger logger, string name, Guid id, Guid trackedActionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Action field '{Id}' updated.")]
    public static partial void ActionFieldUpdated(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Action field '{Id}' deleted.")]
    public static partial void ActionFieldDeleted(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Action field '{Id}' restored.")]
    public static partial void ActionFieldRestored(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Action field with ID '{Id}' was not found.")]
    public static partial void ActionFieldNotFound(this ILogger logger, Guid id);

    // ActionEntry
    [LoggerMessage(Level = LogLevel.Information, Message = "Action entry created with ID '{Id}' for tracked action '{TrackedActionId}'.")]
    public static partial void ActionEntryCreated(this ILogger logger, Guid id, Guid trackedActionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Action entry '{Id}' updated.")]
    public static partial void ActionEntryUpdated(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Action entry '{Id}' deleted.")]
    public static partial void ActionEntryDeleted(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Action entry '{Id}' restored.")]
    public static partial void ActionEntryRestored(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Action entry with ID '{Id}' was not found.")]
    public static partial void ActionEntryNotFound(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Count} action entries bulk-deleted for tracked action '{TrackedActionId}'.")]
    public static partial void ActionEntriesBulkDeleted(this ILogger logger, int count, Guid trackedActionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Count} action entries bulk-updated for tracked action '{TrackedActionId}'.")]
    public static partial void ActionEntriesBulkUpdated(this ILogger logger, int count, Guid trackedActionId);

    // Tag
    [LoggerMessage(Level = LogLevel.Information, Message = "Tag '{Name}' created with ID '{Id}'.")]
    public static partial void TagCreated(this ILogger logger, string name, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tag '{Id}' updated.")]
    public static partial void TagUpdated(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tag '{Id}' deleted.")]
    public static partial void TagDeleted(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tag '{Id}' restored.")]
    public static partial void TagRestored(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tag with ID '{Id}' was not found.")]
    public static partial void TagNotFound(this ILogger logger, Guid id);

    // FieldAnalyticsRule
    [LoggerMessage(Level = LogLevel.Information, Message = "Analytics rule created with ID '{Id}' for tracked action '{TrackedActionId}'.")]
    public static partial void FieldAnalyticsRuleCreated(this ILogger logger, Guid id, Guid trackedActionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Analytics rule '{Id}' updated.")]
    public static partial void FieldAnalyticsRuleUpdated(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Analytics rule '{Id}' deleted.")]
    public static partial void FieldAnalyticsRuleDeleted(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Analytics rule with ID '{Id}' was not found.")]
    public static partial void FieldAnalyticsRuleNotFound(this ILogger logger, Guid id);

    // FieldDependencyRule
    [LoggerMessage(Level = LogLevel.Information, Message = "Dependency rule created with ID '{Id}' for tracked action '{TrackedActionId}'.")]
    public static partial void FieldDependencyRuleCreated(this ILogger logger, Guid id, Guid trackedActionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dependency rule '{Id}' updated.")]
    public static partial void FieldDependencyRuleUpdated(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dependency rule '{Id}' deleted.")]
    public static partial void FieldDependencyRuleDeleted(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dependency rule with ID '{Id}' was not found.")]
    public static partial void FieldDependencyRuleNotFound(this ILogger logger, Guid id);


    // ConnectedActionRule
    [LoggerMessage(Level = LogLevel.Information, Message = "Connected action rule created with ID '{Id}' for tracked action '{TrackedActionId}'.")]
    public static partial void ConnectedActionRuleCreated(this ILogger logger, Guid id, Guid trackedActionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connected action rule '{Id}' updated.")]
    public static partial void ConnectedActionRuleUpdated(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connected action rule '{Id}' deleted.")]
    public static partial void ConnectedActionRuleDeleted(this ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Connected action rule with ID '{Id}' was not found.")]
    public static partial void ConnectedActionRuleNotFound(this ILogger logger, Guid id);
}