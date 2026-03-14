using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Traceon.Contracts.ActionEntries;
using Traceon.Contracts.ActionFields;
using Traceon.Contracts.FieldDefinitions;
using Traceon.Contracts.Tags;
using Traceon.Contracts.TrackedActions;

namespace Traceon.Api.Extensions;

internal static class ODataExtensions
{
    private static readonly ODataValidationSettings ValidationSettings = new()
    {
        AllowedQueryOptions = AllowedQueryOptions.Filter | AllowedQueryOptions.OrderBy |
                              AllowedQueryOptions.Top | AllowedQueryOptions.Skip |
                              AllowedQueryOptions.Count,
        MaxTop = 100
    };

    public static IEdmModel BuildTraceonEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntityType<TrackedActionResponse>();
        builder.EntityType<FieldDefinitionResponse>();
        builder.EntityType<TagResponse>();
        builder.EntityType<ActionFieldResponse>();
        builder.EntityType<ActionEntryResponse>();
        builder.ComplexType<ActionEntryFieldResponse>();
        return builder.GetEdmModel();
    }

    public static IQueryable<T> ApplyODataQuery<T>(
        this IQueryable<T> queryable,
        HttpRequest request,
        IEdmModel edmModel) where T : class
    {
        var context = new ODataQueryContext(edmModel, typeof(T), null);
        var options = new ODataQueryOptions<T>(context, request);
        options.Validate(ValidationSettings);
        return (IQueryable<T>)options.ApplyTo(queryable);
    }
}
