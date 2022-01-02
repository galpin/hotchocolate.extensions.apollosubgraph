// ReSharper disable MemberCanBePrivate.Global

using System.Threading.Tasks;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static HotChocolate.Extensions.ApolloFederation.Test;

namespace HotChocolate.Extensions.ApolloFederation.Directives;

public class KeyDirectiveAnnotationsTests
{
    [Fact]
    public async Task When_key_is_specified_on_class()
    {
        var schema = await BuildSchemaAsync(builder =>
        {
            builder.AddObjectType<ProductWithClassDirective>();
            builder.AddQueryType();
        });
        var sut = schema.GetType<ObjectType>(nameof(ProductWithClassDirective));

        Assert.Collection(
            sut.Directives,
            x => AssertEx.Directive(x, x.Name, ("fields", "\"upc\"")));
        await schema.QuerySdlAndMatchSnapshotAsync();
    }

    [Fact]
    public async Task When_key_is_specified_on_class_multiple_times()
    {
        var schema = await BuildSchemaAsync(builder =>
        {
            builder.AddObjectType<ProductWithMultipleClassDirectives>();
            builder.AddQueryType();
        });
        var sut = schema.GetType<ObjectType>(nameof(ProductWithMultipleClassDirectives));

        Assert.Collection(
            sut.Directives,
            x => AssertEx.Directive(x, "key", ("fields", "\"upc\"")),
            x => AssertEx.Directive(x, "key", ("fields", "\"id\"")));
        await schema.QuerySdlAndMatchSnapshotAsync();
    }

    [Fact]
    public async Task When_key_is_specified_on_property()
    {
        var schema = await BuildSchemaAsync(builder =>
        {
            builder.AddObjectType<ProductWithPropertyDirective>();
            builder.AddQueryType();
        });
        var sut = schema.GetType<ObjectType>(nameof(ProductWithPropertyDirective));

        Assert.Collection(
            sut.Directives,
            x => AssertEx.Directive(x, "key", ("fields", "\"upc\"")));
        await schema.QuerySdlAndMatchSnapshotAsync();
    }

    [GraphQLKey("upc")]
    public class ProductWithClassDirective
    {
        public string? Upc { get; set; }
    }

    [GraphQLKey("upc")]
    [GraphQLKey("id")]
    public class ProductWithMultipleClassDirectives
    {
        public string? Upc { get; set; }
        public string? Id { get; set; }
    }

    public class ProductWithPropertyDirective
    {
        [GraphQLKey]
        public string? Upc { get; set; }
    }
}