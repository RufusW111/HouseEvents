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
					object obj = reader.GetValue(2);
					string coordinatorName = string.Empty;
					if (obj is not DBNull)
					{
						coordinatorName = (string)obj;
					}
					House house = new House(reader.GetString(0), reader.GetString(1), coordinatorName);
					result.Add(house);
                }
            }
			return result;
		}

		public async Task UpdateEventsCoordinatorAsync(string houseName, string eventsCoordinator)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				SqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = "UPDATE dbo.Houses SET EventsCoordinator = @EventsCoordinator where HouseName = @HouseName";
				cmd.Parameters.Add(new SqlParameter("@EventsCoordinator", eventsCoordinator));
				cmd.Parameters.Add(new SqlParameter("@HouseName", eventsCoordinator));
				await cmd.ExecuteNonQueryAsync();
			}
		}
	}
}