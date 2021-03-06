using System;
using System.Collections.Generic;
using System.Threading;
using HotChocolate.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static HotChocolate.Extensions.ApolloSubgraph.Test;

namespace HotChocolate.Extensions.ApolloSubgraph.Resolvers;

public class EntityResolverContextTests
{
    private Context Ctx { get; } = new();

    [Fact]
    public void Ctor_correctly_initializes_members()
    {
        Assert.Same(Ctx.FieldContext.Object, Ctx.Sut.FieldContext);
        Assert.Same(Ctx.Representation, Ctx.Sut.Representation);
        Assert.Same(Ctx.Services, Ctx.Sut.Services);
    }

    [Fact]
     public void Services_resolves_bound_service()
     {
         var expected = Ctx.MyService;

         var actual = Ctx.Sut.Services.GetService<MyService>();

         Assert.Same(expected, actual);
     }

     [Fact]
     public void Service_delegates_to_services()
     {
         var expected = Ctx.MyService;

         var actual = Ctx.Sut.Service<MyService>();

         Assert.Same(expected, actual);
     }

     [Fact]
     public void Service_delegates_to_required_service()
     {
         Assert.Throws<InvalidOperationException>(() => Ctx.Sut.Service<MyOtherService>());
     }

     [Fact]
     public void RequestAborted_returns_field_cancellation_token()
     {
         var cts = new CancellationTokenSource();
         Ctx.FieldContext.SetupRequestAborted(cts.Token);

         cts.Cancel();

         Assert.True(Ctx.Sut.RequestAborted.IsCancellationRequested);
     }

     private sealed class Context
    {
        public Context()
        {
            MyService = new MyService();
            Services = BuildServiceProvider(x => x.AddSingleton(MyService));
            FieldContext = new MockResolverContext();
            FieldContext.SetupServiceProvider(Services);
            Representation = CreateRepresentation(("__typename", "User"));
            Sut = new EntityResolverContext(FieldContext.Object, Representation);
        }

        public MyService MyService { get; }

        public IServiceProvider Services { get; }

        public IReadOnlyDictionary<string, object?> Representation { get; }

        public MockResolverContext FieldContext { get; }

        public EntityResolverContext Sut { get; }
    }

    private sealed class MockResolverContext
    {
        private readonly Mock<IResolverContext> _mock = new();

        public void SetupServiceProvider(IServiceProvider services)
        {
            _mock.SetupGet(x => x.Services).Returns(services);
        }

        public IResolverContext Object => _mock.Object;

        public void SetupRequestAborted(CancellationToken token)
        {
            _mock.SetupGet(x => x.RequestAborted).Returns(token);
        }
    }

    private sealed class MyService
    {
    }

    private sealed class MyOtherService
    {
    }
}