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
			List<EventNoFixturesDto> result = new List<EventNoFixturesDto>();
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				SqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = "select EventName, EventDetailId, EventDate, EventStartTime, EventEndTime, EventVenue, Notes, " +
					"HouseName, Points, EventParticipantId, YearGroup, Reserve, StudentName, NoShow from " +
					"[dbo].[vwEventParticipantsNoFixture] order by EventId, HouseId, Reserve, YearGroup";
				SqlDataReader reader = await cmd.ExecuteReaderAsync();
				while (reader.Read())
				{
					result.Add(GetEventNoFixtures(reader));
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
			object obj = reader.GetValue(2);
			string coordinatorName = string.Empty;
			if (obj is not DBNull)
			{
				coordinatorName = (string)obj;
			}
			return new HouseDto(reader.GetString(0), reader.GetString(1), coordinatorName);
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

		private static EventNoFixturesDto GetEventNoFixtures(SqlDataReader reader)
		{
			EventNoFixturesDto dto = new EventNoFixturesDto(reader.GetString(0), 
				reader.GetInt32(1), reader.GetDateTime(2), reader.GetDateTime(3),
				reader.GetDateTime(4), ;
			
			object obj = reader.GetValue(2);
			string coordinatorName = string.Empty;
			if (obj is not DBNull)
			{
				coordinatorName = (string)obj;
			}
			return new HouseDto(reader.GetString(0), reader.GetString(1), coordinatorName);
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