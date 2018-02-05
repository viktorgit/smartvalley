﻿using System.Collections.Generic;
using System.IO;
using IcoLab.Common.Web.WebApi;
using IcoLab.Web.Common.Extensions;
using IcoLab.Web.Common.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.Signer;
using Nethereum.Web3;
using SmartValley.Application;
using SmartValley.Application.Contracts;
using SmartValley.Application.Contracts.Options;
using SmartValley.Application.Contracts.Scorings;
using SmartValley.Application.Contracts.SmartValley.Application.Contracts;
using SmartValley.Application.Contracts.Votings;
using SmartValley.Data.SQL.Core;
using SmartValley.Data.SQL.Repositories;
using SmartValley.Domain.Interfaces;
using SmartValley.WebApi.Admin;
using SmartValley.WebApi.Applications;
using SmartValley.WebApi.Authentication;
using SmartValley.WebApi.Estimates;
using SmartValley.WebApi.ExceptionHandler;
using SmartValley.WebApi.Projects;
using SmartValley.WebApi.Scoring;
using SmartValley.WebApi.Votings;
using SmartValley.WebApi.WebApi;
using Swashbuckle.AspNetCore.Swagger;

namespace SmartValley.WebApi
{
    public class Startup
    {
        private const string CorsPolicyName = "SVPolicy";

        private readonly IHostingEnvironment _currentEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment currentEnvironment)
        {
            Configuration = configuration;
            _currentEnvironment = currentEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // ReSharper disable once UnusedMember.Global
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureOptions(Configuration, typeof(NethereumOptions), typeof(SiteOptions));

            ConfigureCorsPolicy(services);
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "SmartValley API", Version = "v1"}); });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                                  {
                                      options.RequireHttpsMetadata = _currentEnvironment.IsProduction();
                                      options.TokenValidationParameters = new TokenValidationParameters
                                                                          {
                                                                              ValidateIssuer = true,
                                                                              ValidIssuer = SiteOptions.Issuer,
                                                                              ValidateAudience = true,
                                                                              ValidAudience = SiteOptions.Audience,
                                                                              ValidateLifetime = true,
                                                                              IssuerSigningKey = SiteOptions.GetSymmetricSecurityKey(),
                                                                              ValidateIssuerSigningKey = true
                                                                          };
                                  });

            services.AddSingleton(provider => InitializeWeb3(provider.GetService<NethereumOptions>().RpcAddress));
            services.AddSingleton<IClock, UtcClock>();
            services.AddSingleton<EthereumMessageSigner>();
            services.AddSingleton<EthereumClient>();
            services.AddSingleton<EthereumContractClient>();
            services.AddSingleton<ITokenContractClient, TokenContractClient>(
                provider => new TokenContractClient(provider.GetService<EthereumContractClient>(), provider.GetService<NethereumOptions>().TokenContract));
            services.AddSingleton<IVotingSprintContractClient, VotingSprintContractClient>(
                provider => new VotingSprintContractClient(provider.GetService<EthereumContractClient>(),
                                                           provider.GetService<NethereumOptions>().VotingSprintContract,
                                                           provider.GetService<ITokenContractClient>()));
            services.AddSingleton<IVotingManagerContractClient, VotingManagerContractClient>(
                provider => new VotingManagerContractClient(provider.GetService<EthereumContractClient>(), provider.GetService<NethereumOptions>().VotingManagerContract));
            services.AddSingleton<IScoringContractClient, ScoringContractClient>(
                provider => new ScoringContractClient(provider.GetService<EthereumContractClient>(), provider.GetService<NethereumOptions>().ScoringContract));
            services.AddSingleton<IEtherManagerContractClient, EtherManagerContractClient>(
                provider => new EtherManagerContractClient(provider.GetService<EthereumContractClient>(), provider.GetService<NethereumOptions>().EtherManagerContract));
            services.AddSingleton<IScoringManagerContractClient, ScoringManagerContractClient>(
                provider => new ScoringManagerContractClient(provider.GetService<EthereumContractClient>(), provider.GetService<NethereumOptions>().ScoringManagerContract));

            services.AddMvc(options =>
                            {
                                options.Filters.Add(new AppErrorsExceptionFilter());
                                options.Filters.Add(new ModelStateFilter());
                            });

            var builder = new DbContextOptionsBuilder<AppDBContext>();
            builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            var dbOptions = builder.Options;
            services.AddDbContext<AppDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient(x => AppDBContext.CreateEditable(dbOptions));
            services.AddTransient(x => AppDBContext.CreateReadOnly(dbOptions));
            services.AddTransient<ITeamMemberRepository, TeamMemberRepository>();
            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IScoringRepository, ScoringRepository>();
            services.AddTransient<IEstimateCommentRepository, EstimateCommentRepository>();
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IVotingService, VotingService>();
            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IEstimationService, EstimationService>();
            services.AddTransient<IScoringService, ScoringService>();
            services.AddTransient<IQuestionRepository, QuestionRepository>();
            services.AddTransient<IVotingService, VotingService>();
            services.AddTransient<IVotingRepository, VotingRepository>();
            services.AddTransient<IVotingProjectRepository, VotingProjectRepository>();
            services.AddTransient<IAdminService, AdminService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors(CorsPolicyName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartValley API V1"); });
            }

            app.UseAuthentication();
            app.Use(async (context, next) =>
                    {
                        await next();
                        if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
                        {
                            context.Request.Path = "/index.html";
                            await next();
                        }
                    })
               .UseDefaultFiles(new DefaultFilesOptions {DefaultFileNames = new List<string> {"index.html"}})
               .UseStaticFiles()
               .UseMvc();
        }

        private void ConfigureCorsPolicy(IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            var siteOptions = sp.GetService<SiteOptions>();
            var url = siteOptions.BaseUrl;
            if (!_currentEnvironment.IsProduction())
            {
                url = "*";
            }

            var corsPolicyBuilder = new CorsPolicyBuilder();
            corsPolicyBuilder.WithOrigins(url);
            corsPolicyBuilder.AllowAnyHeader();
            corsPolicyBuilder.AllowAnyMethod();
            corsPolicyBuilder.WithExposedHeaders(Headers.XEthereumAddress, Headers.XSignedText, Headers.XSignature);
            corsPolicyBuilder.AllowCredentials();

            services.AddCors(options => { options.AddPolicy(CorsPolicyName, corsPolicyBuilder.Build()); });
        }

        private static Web3 InitializeWeb3(string rpcAddress)
        {
            if (!string.IsNullOrEmpty(rpcAddress))
                return new Web3(rpcAddress);

            var ipcClient = new IpcClient("./geth.ipc");
            return new Web3(ipcClient);
        }
    }
}