using HouseEvents.Data.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace HouseEvents.Data
{
	public class HouseEventsDB
	{
		private readonly string _connectionString;

		public HouseEventsDB(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task<List<HouseDto>> GetHouseInfoAsync()
		{
			List<HouseDto> result = new List<HouseDto>();
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				SqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = "SELECT HouseName, UndermasterFirstName, EventsCoordinator from dbo.House";
				SqlDataReader reader = await cmd.ExecuteReaderAsync();
				while (reader.Read())
				{
					result.Add(GetHouse(reader));
				}
			}
			return result;
		}

		public async Task<List<EventNoFixturesDto>> GetEventNoFixturesAsync()
		{
			List<EventNoFixturesDto> result = new();
			using (SqlConnection connection = new(_connectionString))
			{
				connection.Open();
				SqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = "select EventId, EventName, EventDetailId, EventDate, EventStartTime, EventEndTime, EventVenue, Notes, " +
					"HouseId, HouseName, Points, EventParticipantId, YearGroup, Reserve, StudentName, NoShow from " +
					"[dbo].[vwEventParticipantsNoFixture] order by EventId, HouseId, Reserve, YearGroup";
				SqlDataReader reader = await cmd.ExecuteReaderAsync();
				if (reader.Read())
				{
					result = GetEventNoFixtures(reader);
				}
			}
			return result;
		}

		public async Task<HouseDto?> GetHouseInfoAsync(string houseName)
		{
			HouseDto? result = null;
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				SqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = "SELECT HouseName, UndermasterFirstName, EventsCoordinator from dbo.House where HouseName = @houseName";
				cmd.Parameters.Add(new SqlParameter("@HouseName", houseName));
				SqlDataReader reader = await cmd.ExecuteReaderAsync();
				if (reader.Read())
				{
					result = GetHouse(reader);
				}
			}
			return result;
		}

		private static HouseDto GetHouse(SqlDataReader reader)
		{
			return new HouseDto(reader.GetString(0), reader.GetString(1), GetNullableString(reader.GetValue(2)));
		}

		private static string? GetNullableString(object obj)
		{
			string? ret = null;
			if (obj is not DBNull)
			{
				ret = (string)obj;
			}
			return ret;
		}

		private static List<EventNoFixturesDto> GetEventNoFixtures(SqlDataReader reader)
		{
			// Probably best to use Entity framework for this type of thing
			// Illustrating how to do it without the use of frameworks
			List<EventNoFixturesDto> result = new List<EventNoFixturesDto>();
			int currentEntryId = -1;
			//int currentHouseId = reader.GetInt32(8);	

			do
			{
				int entryId = reader.GetInt32(0);
				if (currentEntryId != entryId)
				{
					currentEntryId = entryId;
					EventNoFixturesDto dto = new EventNoFixturesDto(reader.GetString(1),
						reader.GetInt32(2), reader.GetFieldValue<DateOnly>(3), reader.GetFieldValue<TimeOnly>(4),
						reader.GetFieldValue<TimeOnly>(5), GetNullableString(reader.GetValue(6)),
						GetNullableString(reader.GetValue(7)));
					result.Add(dto);

				}
			} while (reader.Read());


			return result;
		}

		public async Task UpdateEventsCoordinatorAsync(string houseName, string eventsCoordinator)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				SqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = "UPDATE dbo.House SET EventsCoordinator = @EventsCoordinator where HouseName = @HouseName";
				cmd.Parameters.Add(new SqlParameter("@EventsCoordinator", eventsCoordinator));
				cmd.Parameters.Add(new SqlParameter("@HouseName", houseName));
				await cmd.ExecuteNonQueryAsync();
			}
		}
	}
}