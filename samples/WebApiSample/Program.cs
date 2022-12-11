﻿using System.Reflection;
using Imprise.MediatR.Extensions.Caching;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

// Add MediatR pipeline behaviors which are called in order for each IRequest
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();