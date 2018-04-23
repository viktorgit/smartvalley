﻿using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using Autofac;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.Web3;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport.SQLServer;
using SmartValley.Application;
using SmartValley.Application.Email;
using SmartValley.Application.Templates;
using SmartValley.Data.SQL.Core;
using SmartValley.Data.SQL.Repositories;
using SmartValley.Domain.Contracts;
using SmartValley.Domain.Interfaces;
using SmartValley.Domain.Services;
using SmartValley.Ethereum;
using SmartValley.Ethereum.Contracts.ScoringExpertsManager;
using SmartValley.Ethereum.Contracts.ScoringManager;
using SmartValley.Ethereum.Contracts.SmartValley.Application.Contracts;
using SmartValley.Messages.Events;

namespace SmartValley.WebApi
{
    public static class EndpointConfigurator
    {
        private const string EndpointName = "SmartValley.Api";

        public static Task<IEndpointInstance> StartAsync(IConfiguration configuration, string contentRootPath, IDataProtectionProvider dataProtectionProvider)
        {
            var endpointConfiguration = new EndpointConfiguration(EndpointName);

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            ConfigureTransport(endpointConfiguration, connectionString);

            endpointConfiguration
                .Conventions()
                .DefiningCommandsAs(type => type.Namespace != null && type.Namespace.EndsWith("Commands"))
                .DefiningEventsAs(type => type.Namespace != null && type.Namespace.EndsWith("Events"));

            ConfigurePersistence(endpointConfiguration, connectionString);

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            ConfigureContainer(endpointConfiguration, connectionString, configuration, contentRootPath, dataProtectionProvider);

            endpointConfiguration
                .Recoverability()
                .Immediate(settings => settings.NumberOfRetries(3))
                .Delayed(settings => settings.NumberOfRetries(5));

            endpointConfiguration.EnableOutbox();
            endpointConfiguration.EnableInstallers();

            endpointConfiguration.LimitMessageProcessingConcurrencyTo(1);

            endpointConfiguration.UnitOfWork()
                                 .WrapHandlersInATransactionScope(isolationLevel: IsolationLevel.ReadCommitted);

            return Endpoint.Start(endpointConfiguration);
        }

        private static void ConfigureTransport(EndpointConfiguration endpointConfiguration, string connectionString)
        {
            var transport = endpointConfiguration
                            .UseTransport<SqlServerTransport>()
                            .ConnectionString(connectionString)
                            .DefaultSchema("msg")
                            .Transactions(TransportTransactionMode.SendsAtomicWithReceive);

            transport.Routing()
                     .RegisterPublisher(typeof(TransactionCompleted).Assembly, EndpointName);
        }

        private static void ConfigurePersistence(EndpointConfiguration endpointConfiguration, string connectionString)
        {
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();

            persistence.SqlDialect<SqlDialect.MsSqlServer>()
                       .Schema("nsb");

            persistence.ConnectionBuilder(() => new SqlConnection(connectionString));

            persistence.SubscriptionSettings()
                       .DisableCache();
        }

        private static void ConfigureContainer(EndpointConfiguration endpointConfiguration,
                                               string connectionString,
                                               IConfiguration configuration,
                                               string contentRootPath,
                                               IDataProtectionProvider dataProtectionProvider)
        {
            var containerBuilder = new ContainerBuilder();

            // Options
            RegisterOptions<NethereumOptions>(configuration, containerBuilder);
            RegisterOptions<SmtpOptions>(configuration, containerBuilder);
            RegisterOptions<SiteOptions>(configuration, containerBuilder);

            // DB context
            var contextOptionsBuilder = new DbContextOptionsBuilder<AppDBContext>().UseSqlServer(connectionString);
            containerBuilder.Register(context => AppDBContext.CreateEditable(contextOptionsBuilder.Options))
                            .As<IEditableDataContext>();
            containerBuilder.Register(context => AppDBContext.CreateReadOnly(contextOptionsBuilder.Options))
                            .As<IReadOnlyDataContext>();

            // Repositories
            containerBuilder.RegisterType<UserRepository>().As<IUserRepository>();
            containerBuilder.RegisterType<ProjectRepository>().As<IProjectRepository>();
            containerBuilder.RegisterType<ScoringRepository>().As<IScoringRepository>();
            containerBuilder.RegisterType<ScoringOffersRepository>().As<IScoringOffersRepository>();

            // Ethereum
            containerBuilder.Register(context => InitializeWeb3(context.Resolve<NethereumOptions>().RpcAddress)).AsSelf();

            containerBuilder.RegisterType<EthereumContractClient>().AsSelf();
            containerBuilder.RegisterType<EthereumClient>().AsSelf();

            containerBuilder.Register(context => new ScoringExpertsManagerContractClient(
                                          context.Resolve<EthereumContractClient>(),
                                          context.Resolve<NethereumOptions>().ScoringExpertsManagerContract))
                            .As<IScoringExpertsManagerContractClient>();

            containerBuilder.Register(context => new ScoringManagerContractClient(
                                          context.Resolve<EthereumContractClient>(),
                                          context.Resolve<NethereumOptions>().ScoringManagerContract))
                            .As<IScoringManagerContractClient>();

            // Services
            containerBuilder.RegisterType<UtcClock>().As<IClock>();
            containerBuilder.RegisterType<MailTokenService>().AsSelf();
            containerBuilder.RegisterType<MailService>().AsSelf();
            containerBuilder.RegisterType<MailSender>().AsSelf();
            containerBuilder.Register(context => new TemplateProvider(contentRootPath)).As<ITemplateProvider>();
            containerBuilder.RegisterInstance(dataProtectionProvider).As<IDataProtectionProvider>();
            containerBuilder.RegisterType<ScoringService>().As<IScoringService>();

            var container = containerBuilder.Build();

            endpointConfiguration.UseContainer<AutofacBuilder>(customizations => customizations.ExistingLifetimeScope(container));
        }

        private static void RegisterOptions<TOptions>(IConfiguration configuration, ContainerBuilder containerBuilder)
            where TOptions : class
        {
            var instance = Activator.CreateInstance<TOptions>();
            configuration.Bind(typeof(TOptions).ShortDisplayName(), instance);
            containerBuilder.RegisterInstance(instance).As<TOptions>();
        }

        private static Web3 InitializeWeb3(string rpcAddress)
            => !string.IsNullOrEmpty(rpcAddress)
                   ? new Web3(rpcAddress)
                   : new Web3(new IpcClient("./geth.ipc"));
    }
}