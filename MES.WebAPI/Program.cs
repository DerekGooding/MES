using MES.Data;
using Microsoft.EntityFrameworkCore;

namespace MES.WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddDbContext<DataContext>(options =>
        options.UseSqlite("Data Source=C:\\Users\\Steimel_M1\\source\\repos\\MES\\MES\\bin\\Debug\\net9.0\\partsdata.db"));

        //builder.Services.AddDbContext<DataContext>(options =>
        //    options.UseSqlite());

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseCors(policy =>
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin());


        app.MapControllers();

        app.Run();
    }
}
