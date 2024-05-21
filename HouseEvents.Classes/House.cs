namespace HouseEvents.Classes
{
	public class House
	{
		private readonly string _surmasterFirstName;
		public string HouseName { get; private set; }
		public string SurmasterName
		{
			get { return $"{_surmasterFirstName} {HouseName}"; }
		}
		public string ActivitiesCoordinator { get; set; }

		public House(string houseName, string surmasterFirstName, string activitiesCoordinator)
		{
			_surmasterFirstName = surmasterFirstName;
			HouseName = houseName;
			ActivitiesCoordinator = activitiesCoordinator;
		}
	}
}