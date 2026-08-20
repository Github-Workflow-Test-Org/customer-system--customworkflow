using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using CustomerManagement.Services;
using CustomerManagement.Repositories;
using CustomerManagement.Models;

namespace CustomerManagement;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register application services
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IAuditService, AuditService>();
        builder.Services.AddScoped<ISecurityService, SecurityService>();

        // Register repositories
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Configure options
        var connectionString = builder.Configuration.GetConnectionString("CustomerManagementDb")
            ?? throw new InvalidOperationException("Connection string 'CustomerManagementDb' not found.");

        builder.Services.AddSingleton<string>(connectionString);

        var app = builder.Build();

        // Configure middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
