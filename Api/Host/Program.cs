using System.Reflection;
using AuthenticationApi.Extensions;
using AuthenticationApi.Middleware;
using Db;
using Domain.Mediator.Commands.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("db")));

builder.Services.RegisterAuthentication(configuration)
    .AddMediatR(typeof(LoginUserCommand));

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterSwagger();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient",
        b =>
        {
            b
                .WithOrigins("http://localhost:4200", "https://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularDevClient");
app.ConfigureExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await SeedManager.Seed(services);
}

app.Run();

