using HouseEvents.Data;

namespace HouseEvents.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddAuthorization();

			var app = builder.Build();

			// Get connection string
			string connectionString = app.Configuration["ConnectionStrings:HouseEventDb"] ?? string.Empty;
			app.Logger.Log(LogLevel.Information, connectionString);
			
			// Configure the HTTP request pipeline.
			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapGet("/houseInfo", (HttpContext httpContext) =>
			{
				HouseEventsDB db = new (connectionString);
				List<House> houses = db.GetHouseInfo();
				return Results.Ok(houses);
			});

			app.Run();
		}
	}
}