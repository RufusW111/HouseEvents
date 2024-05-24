namespace HouseEvents.Data.Dtos
{
    public record EventNoFixturesDto
	{
		public string EventName { get; init; } = string.Empty;

		public int EventDetailId { get; init; }

		public DateOnly EventDate { get; init; }

		public TimeOnly EventStartTime { get; init; }

		public TimeOnly EventEndTime { get; init; }

		public string? Venue { get; init; }

		public string? Notes { get; init; }

		public List<HouseEventDto> Houses { get; init; }

		public EventNoFixturesDto(string eventName, int eventDetailId, DateOnly eventDate, TimeOnly eventStartTime,
			TimeOnly eventEndTime, string? venue, string? notes)
		{
			EventName = eventName;
			EventDetailId = eventDetailId;
			EventDate = eventDate;
			EventStartTime = eventStartTime;
			EventEndTime = eventEndTime;
			Venue = venue;
			Notes = notes;
			Houses = new List<HouseEventDto>();
		}
	}
}
