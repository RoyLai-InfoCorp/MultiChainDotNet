using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MultiChainDotNet.Api.Service.Controllers;
using MultiChainDotNet.Core;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service
{
	public class Startup
	{
		readonly string AllowAllOrigins = "_allowAllOrigins";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			var container = services.BuildServiceProvider();
			var configRoot = container.GetRequiredService<IConfiguration>();
			var allowedOrigins = configRoot.GetSection("AllowedOrigins").Get<string>();

			services.AddSignalR();

			services.AddCors(options =>
			{
				options.AddPolicy(name: AllowAllOrigins,
					builder =>
					{
						builder
							.WithOrigins(allowedOrigins.Split(','))
							.AllowCredentials()
							.AllowAnyHeader()
							.AllowAnyMethod();
					});
			});

			services
				.AddControllers()
				.AddNewtonsoftJson();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "MultiChainDotNet Api", Version = "v1" });
			});

			var mcConfig = configRoot.GetSection("MultiChainConfiguration").Get<MultiChainConfiguration>();
			services.AddSingleton(mcConfig);
			services
				.AddScoped<JsonRpcCommand>()
					.AddHttpClient<JsonRpcCommand>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
						.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
						.AddPolicyHandler(ExceptionPolicyHandler.RetryPolicy())
						.AddPolicyHandler(ExceptionPolicyHandler.TimeoutPolicy())
						.ConfigurePrimaryHttpMessageHandler(() =>
						{
							return new HttpClientHandler()
							{
								Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
							};
						});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MultiChainDotNet Api"));
			}

			app.UseRouting();
			app.UseCors(AllowAllOrigins);
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHub<TransactionHub>("/transaction");
			});

		}
	}
}
