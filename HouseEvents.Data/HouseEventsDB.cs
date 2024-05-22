using Microsoft.Data.SqlClient;

namespace HouseEvents.Data
{
	public class HouseEventsDB
	{
		private readonly string _connectionString;

		public HouseEventsDB(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task<List<House>> GetHouseInfoAsync()
		{
			List<House> result = new List<House>();
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

		public async Task<House?> GetHouseInfoAsync(string houseName)
		{
			House? result = null;
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

		private House GetHouse(SqlDataReader reader)
		{
			object obj = reader.GetValue(2);
			string coordinatorName = string.Empty;
			if (obj is not DBNull)
			{
				coordinatorName = (string)obj;
			}
			return new House(reader.GetString(0), reader.GetString(1), coordinatorName);
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