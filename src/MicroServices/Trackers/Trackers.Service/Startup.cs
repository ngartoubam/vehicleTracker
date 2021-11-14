﻿using System;

using Domain.Module;

using EventFlow;
using EventFlow.AspNetCore.Extensions;
using EventFlow.DependencyInjection.Extensions;
using EventFlow.EntityFramework;
using EventFlow.Extensions;
using EventFlow.RabbitMQ;
using EventFlow.RabbitMQ.Extensions;

using EventStore.Middleware.Module;

using Infrastructure.Configurations;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Trackers.ReadStore;
using Trackers.ReadStore.Module;

namespace Trackers.Service; 

public class Startup {
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) {
        _configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public IServiceProvider ConfigureServices(IServiceCollection services) {

        var env = EnvironmentConfiguration.Bind(_configuration);

        services
            .AddSingleton(env)
            .AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trackers API", Version = "v1" }))
            .AddMvc(e => e.EnableEndpointRouting = false);

        return EventFlowOptions.New
            .UseServiceCollection(services)
            .AddAspNetCore()
            .UseConsoleLog()
            .RegisterModule<DomainModule>()
            .RegisterModule<TrackingReadStoreModule>()
            .RegisterModule<EventSourcingModule>()
            .PublishToRabbitMq(RabbitMqConfiguration.With(new Uri(env.RabbitMqConnection)))
            .CreateServiceProvider();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {

        // initialize InfoDbContext
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetService<IDbContextProvider<TrackingContext>>();
            dbContext?.CreateContext();
        }

        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trackers API V1"); });
            app.UseMvc();
        }
        else {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseMvc();
    }
}