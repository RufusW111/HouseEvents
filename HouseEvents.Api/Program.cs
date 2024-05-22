using HouseEvents.Data;
using Microsoft.AspNetCore.Mvc;

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

			HouseEventsDB db = new(connectionString);

			app.MapGet("/", GetHouses);
			app.MapGet("/house", GetHouses);
			async Task<IResult> GetHouses() 
			{
				List<House> houses = await db.GetHouseInfoAsync();
				return Results.Ok(houses);
			};	

			app.MapGet("/house/{houseName}", async (string houseName) =>
			{
				House? house = await db.GetHouseInfoAsync(houseName);
				IResult result = house == null ? Results.NotFound() : Results.Ok(house);
				return result;
			});

			app.MapPut("/house/{houseName}/coordinator", async (string houseName, [FromQuery(Name = "coord")] string coordinator) =>
			{
				await db.UpdateEventsCoordinatorAsync(houseName, coordinator);
			});

			app.Run();
		}
	}
}