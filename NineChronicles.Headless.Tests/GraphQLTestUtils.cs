using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using NCAction = Libplanet.Action.PolymorphicAction<Nekoyume.Action.ActionBase>;

namespace NineChronicles.Headless.Tests
{
    public static class GraphQLTestUtils
    {
        public static Task<ExecutionResult> ExecuteQueryAsync<TObjectGraphType>(
            string query,
            IDictionary<string, object>? userContext = null,
            object? source = null,
            StandaloneContext? standaloneContext = null)
            where TObjectGraphType : class, IObjectGraphType
        {
            return ExecuteQueryAsync(
                typeof(TObjectGraphType),
                query,
                userContext,
                source,
                standaloneContext
            );
        }

        public static Task<ExecutionResult> ExecuteQueryAsync(
            Type objectGraphType,
            string query,
            IDictionary<string, object>? userContext = null,
            object? source = null,
            StandaloneContext? standaloneContext = null)
        {
            var services = new ServiceCollection();
            services.AddSingleton(objectGraphType);
            if (!(standaloneContext is null))
            {
                services.AddSingleton(standaloneContext);
            }

            services.AddLibplanetExplorer<NCAction>();

            var serviceProvider = services.BuildServiceProvider();
            return ExecuteQueryAsync(
                objectGraphType,
                serviceProvider,
                query,
                userContext,
                source);
        }

        public static Task<ExecutionResult> ExecuteQueryAsync<TObjectGraphType>(
            IServiceProvider serviceProvider,
            string query,
            IDictionary<string, object>? userContext = null,
            object? source = null)
            where TObjectGraphType : IObjectGraphType
        {
            return ExecuteQueryAsync(typeof(TObjectGraphType), serviceProvider, query, userContext, source);
        }

        public static Task<ExecutionResult> ExecuteQueryAsync(
            Type objectGraphType,
            IServiceProvider serviceProvider,
            string query,
            IDictionary<string, object>? userContext = null,
            object? source = null)
        {
            var graphType = (IObjectGraphType)serviceProvider.GetService(objectGraphType);
            var documentExecutor = new DocumentExecuter();
            return documentExecutor.ExecuteAsync(new ExecutionOptions
            {
                Query = query,
                Schema = new Schema
                {
                    Query = graphType,
                },
                UserContext = userContext,
                Root = source,
            });
        }
    }
}
