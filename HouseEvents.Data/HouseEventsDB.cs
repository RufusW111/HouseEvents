using HouseEvents.Data.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using static Azure.Core.HttpHeader;

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

		public async Task InsertEventNoFixturesAsync(NewEventNoFixturesDto dto)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				SqlTransaction sqlTransaction = connection.BeginTransaction();
				try
				{
					int eventId = await InsertEventAsync(connection, dto.EventName, dto.EventDate);
					int eventDetailId = await InsertEventDetailAsync(connection, eventId, dto);
					await InsertHouseEventAsync(connection, eventDetailId);
					
					// Insert participants
				}
				catch (SqlException ex) 
				{ 
					sqlTransaction.Rollback();
					throw ex;				
				}
				finally 
				{ 
					sqlTransaction.Commit(); 
				}
				
			}
		}

		private static async Task InsertHouseEventAsync(SqlConnection connection, int eventDetailId)
		{
			SqlCommand cmd = connection.CreateCommand();
			cmd.CommandText = "INSERT INTO dbo.[HouseEvent](EventDetailID, HouseID, Points) " +
				"select @EventDetailId, houseId, 0 from dbo.House ";
			cmd.Parameters.Add(new SqlParameter("@EventDetailId", eventDetailId));
			await cmd.ExecuteNonQueryAsync();
		}

		private static async Task<int> InsertEventDetailAsync(SqlConnection connection, int eventId, NewEventNoFixturesDto dto)
		{
			SqlCommand cmd = connection.CreateCommand();
			cmd.CommandText = "INSERT INTO dbo.[EventDetail](EventID, EventDate, EventStartTime, EventEndTime, EventVenue, Notes) " +
				"values(@EventID, @EventDate, @EventStartTime, @EventEndTime, @EventVenue, @Notes) " +
				"OUTPUT INSERTED.EventDetailID ";
			cmd.Parameters.Add(new SqlParameter("@EventId", eventId));
			cmd.Parameters.Add(new SqlParameter("@EventDate", dto.EventDate));
			cmd.Parameters.Add(new SqlParameter("@EventStartTime", dto.EventEndTime));
			cmd.Parameters.Add(new SqlParameter("@EventEndTime", dto.EventStartTime));
			cmd.Parameters.Add(new SqlParameter("@EventVenue", dto.Venue));
			cmd.Parameters.Add(new SqlParameter("@Notes", dto.Notes));
			var result = await cmd.ExecuteScalarAsync();
			int eventDetailId = Convert.ToInt32(result);
			return eventDetailId;
		}

		private static async Task<int> InsertEventAsync(SqlConnection connection, string eventName, DateOnly eventDate)
		{
			SqlCommand cmd = connection.CreateCommand();
			cmd.CommandText = "INSERT INTO dbo.[Event](EventName, SchoolYear) values(@EventName, @EventSchoolYear) OUTPUT INSERTED.EventID ";
			cmd.Parameters.Add(new SqlParameter("@EventsName", eventName));
			cmd.Parameters.Add(new SqlParameter("@SchoolYear", GetSchoolYear(eventDate)));
			var result = await cmd.ExecuteScalarAsync();
			int eventId = Convert.ToInt32(result);
			return eventId;
		}

		private static int GetSchoolYear(DateOnly eventDate)
		{
			int ret = eventDate.Year;
			if (eventDate.Month < 9) {

				ret -= 1;
			}
			return ret;
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

		private static bool? GetNullableBoolean(object obj)
		{
			bool? ret = null;
			if (obj is not DBNull)
			{
				ret = (bool)obj;
			}
			return ret;
		}

		private static List<EventNoFixturesDto> GetEventNoFixtures(SqlDataReader reader)
		{
			// Probably best to use Entity framework for this type of thing
			// Illustrating how to do it without the use of frameworks
			List<EventNoFixturesDto> result = new List<EventNoFixturesDto>();
			int currentEntryId = -1;
			int currentHouseId = -1;
			EventNoFixturesDto? eventDto = null;
			HouseEventDto? houseEventDto = null;

			do
			{
				int entryId = reader.GetInt32(0);
				if (currentEntryId != entryId)
				{
					if (eventDto != null)
					{
						result.Add(eventDto);
					}
					currentEntryId = entryId;
					eventDto = new (reader.GetString(1),
						reader.GetInt32(2), reader.GetFieldValue<DateOnly>(3), reader.GetFieldValue<TimeOnly>(4),
						reader.GetFieldValue<TimeOnly>(5), GetNullableString(reader.GetValue(6)),
						GetNullableString(reader.GetValue(7)));
				}

				int houseId = reader.GetInt32(8);
				if (currentHouseId != houseId)
				{
					if (houseEventDto != null && eventDto != null)
					{
						eventDto.Houses.Add(houseEventDto);
					}
					currentHouseId = houseId;
					houseEventDto = new (reader.GetString(9), reader.GetInt32(10));
				}		

				if (houseEventDto != null)
				{
					ParticipantDto participant = new(reader.GetInt32(11), GetNullableString(reader.GetValue(14)), reader.GetBoolean(13), reader.GetString(12), GetNullableBoolean(reader.GetValue(15)));
					houseEventDto.Participants.Add(participant);
				}

			} while (reader.Read());

			if (eventDto != null)
			{
				result.Add(eventDto);
			}
			return result;
		}

		
	}
}