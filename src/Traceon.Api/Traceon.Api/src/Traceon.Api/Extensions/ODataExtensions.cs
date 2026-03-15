using Microsoft.AspNetCore.OData.Extensions;
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
        context.DefaultQueryConfigurations.MaxTop = ValidationSettings.MaxTop;
        context.DefaultQueryConfigurations.EnableFilter = true;
        context.DefaultQueryConfigurations.EnableOrderBy = true;
        context.DefaultQueryConfigurations.EnableCount = true;
        var options = new ODataQueryOptions<T>(context, request);
        options.Validate(ValidationSettings);

        var result = (IQueryable<T>)options.ApplyTo(queryable);

        var totalCount = request.HttpContext.ODataFeature().TotalCount;
        if (totalCount.HasValue)
            request.HttpContext.Response.Headers["X-Total-Count"] = totalCount.Value.ToString();

        return result;
    }
}

