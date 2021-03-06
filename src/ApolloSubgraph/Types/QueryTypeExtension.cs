using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;

namespace HotChocolate.Extensions.ApolloSubgraph;

/// <summary>
/// The root query extensions are used for schema introspection and for resolving entities from external services.
/// </summary>
public sealed class QueryTypeExtension : ObjectTypeExtension
{
    private readonly IEntityResolverRegistry _entityResolverRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryTypeExtension"/> class.
    /// </summary>
    /// <param name="entityResolverRegistry">The entity resolver registry.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="entityResolverRegistry"/> is <see langword="null"/>.
    /// </exception>
    public QueryTypeExtension(IEntityResolverRegistry entityResolverRegistry)
    {
        if (entityResolverRegistry is null)
        {
            throw new ArgumentNullException(nameof(entityResolverRegistry));
        }

        _entityResolverRegistry = entityResolverRegistry;
    }

    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name(Names.Query);

        descriptor.Field(Names.Entities)
            .Type<NonNullType<ListType<EntityType>>>()
            .Argument(
                Names.Representations,
                x => x.Type<NonNullType<ListType<NonNullType<AnyType>>>>())
            .Resolve(ResolveEntitiesAsync);

        descriptor.Field(Names.Service)
            .Type<ServiceType>()
            .Resolve(new object());
    }

    private Task<object?[]> ResolveEntitiesAsync(IResolverContext context)
    {
        var representations = context.ArgumentValue<object[]>(Names.Representations);
        var tasks = new List<Task<object?>>(representations.Length);
        foreach (IReadOnlyDictionary<string, object?> representation in representations)
        {
            tasks.Add(ResolveAsync(representation));
        }
        return Task.WhenAll(tasks);

        async Task<object?> ResolveAsync(IReadOnlyDictionary<string, object?> representation)
        {
            if (!representation.TryGetValue(Names.Typename, out var value))
            {
                throw ThrowHelper.Entities_Representation_Typename_Missing();
            }
            if (value is not string name)
            {
                throw ThrowHelper.Entities_Representation_Typename_Invalid(value);
            }
            if (!_entityResolverRegistry.TryGet(name, out var resolver))
            {
                throw ThrowHelper.Entities_Representation_Entity_NotFound(name);
            }
            try
            {
                var resolverContext = new EntityResolverContext(context, representation);
                return await resolver!(resolverContext).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw ThrowHelper.Entities_Representation_Resolver_Error(name, exception);
            }
        }
    }

    private static class Names
    {
        public const string Query = "Query";
        public const string Service = "_service";
        public const string Entities = "_entities";
        public const string Representations = "representations";
        public const string Typename = "__typename";
    }
}