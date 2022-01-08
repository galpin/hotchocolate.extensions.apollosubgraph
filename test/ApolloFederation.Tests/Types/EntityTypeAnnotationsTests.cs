using System.Threading.Tasks;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static HotChocolate.Extensions.ApolloFederation.Test;

namespace HotChocolate.Extensions.ApolloFederation.Types;

public class EntityTypeAnnotationsTests
{
    [Fact]
    public async Task When_key_is_specified_on_object()
    {
        var schema = await BuildSchemaAsync(x => x.AddQueryType<QueryWhenSingle>());

        var sut = schema.GetType<EntityType>("_Entity");

        Assert.Collection(
            sut.Types.Values,
            x => Assert.Equal("Product", x.Name));
    }

    [Fact]
    public async Task When_key_is_specified_on_multiple_objects()
    {
        var schema = await BuildSchemaAsync(x => x.AddQueryType<QueryWhenMultiple>());

        var sut = schema.GetType<EntityType>("_Entity");

        Assert.Collection(
            sut.Types.Values,
            x => Assert.Equal("Product", x.Name),
            x => Assert.Equal("Review", x.Name));
    }

    [Fact]
    public async Task When_key_is_specified_on_object_extension()
    {
        var schema = await BuildSchemaAsync(builder =>
        {
            builder.AddQueryType<QueryWhenObjectExtension>();
            builder.AddTypeExtension<ProductExtension>();
        });

        var sut = schema.GetType<EntityType>("_Entity");

        Assert.Collection(
            sut.Types.Values,
            x => Assert.Equal("Product", x.Name));
    }

    public class QueryWhenSingle
    {
        public Product? GetProduct(int id)
        {
            return default;
        }
    }

    public class QueryWhenMultiple
    {
        public Product? GetProduct(int id)
        {
            return default;
        }

        public Review? GetReview(int id)
        {
            return default;
        }
    }

    public class Product
    {
        [GraphQLKey]
        public int Id { get; set; }
    }

    [GraphQLKey("id")]
    public class Review
    {
        public int Id { get; set; }
    }

    public class QueryWhenObjectExtension
    {
        public ProductWhenObjectExtension? GetProduct(string upc)
        {
            return default;
        }
    }

    [GraphQLName("Product")]
    public class ProductWhenObjectExtension
    {
    }

    [ExtendObjectType(typeof(ProductWhenObjectExtension))]
    public class ProductExtension
    {
        [GraphQLKey]
        public string? Upc { get; set; }
    }
}