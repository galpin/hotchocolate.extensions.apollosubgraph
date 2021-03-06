using System;
using System.Collections.Generic;
using System.Threading;
using HotChocolate.Resolvers;

namespace HotChocolate.Extensions.ApolloSubgraph;

/// <summary>
/// Specifies the resolution context for an entity being resolved.
/// </summary>
public interface IEntityResolverContext
{
    /// <summary>
    /// Gets the resolver context for the parent <c>_entities</c> field.
    /// </summary>
    IResolverContext FieldContext { get; }

    /// <summary>
    /// Gets the scoped request service provider.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// Gets the entity representation (provided to the <c>_entities</c> field).
    /// </summary>
    IReadOnlyDictionary<string, object?> Representation { get; }

    /// <summary>
    /// Gets a cancellation token that notifies when the request is cancelled.
    /// </summary>
    CancellationToken RequestAborted { get; }

    /// <summary>
    /// Gets a required service from the service provider.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service.</returns>
    /// <exception cref="InvalidOperationException">
    /// There is no service of type <typeparamref name="T"/>
    /// </exception>
    T Service<T>();
}
