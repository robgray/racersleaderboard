using System;

namespace RacersLeaderboard.Core.Services.iRacing.Models
{
	public class DriverInfo
	{
		public string favTrack { get; set; }

		public string memberSince { get; set; }

		public string clubId { get; set; }

		public License[] licenses { get; set; }

		public string memberSinceMS { get; set; }

		public string favCar { get; set; }

		public string displayName { get; set; }

		public Helmet helmet { get; set; }

		public string custId { get; set; }

		public bool watched { get; set; }

		public bool friend { get; set; }

		// licenses always comes back with two items. 
		// 0 = Oval
		// 1 = Road
		public License RoadLicense => licenses[Constants.Categories.Road];

		public License OvalLicense => licenses[Constants.Categories.Oval];

		
	}

	public class License
	{
		public string srPrime { get; set; }

		public string srSub { get; set; }

		public int subLevel { get; set; }

		public string iRating { get; set; }

		public int mprNumRaces { get; set; }

		public string licLevelDisplayName { get; set; }

		public int catId { get; set; }

		public string licGroupDisplayName { get; set; }

		public string licGroup { get; set; }

		public string mprNumTTs { get; set; }

		public string ttRating { get; set; }

		public string licColor { get; set; }

		public int licLevel { get; set; }

		public string LicenseForegroundColor
		{
			get
			{
				if (licGroup == "5" || licGroup == "6" || licGroup == "1")
					return "FFFFFF";
				return "000000";
			}
		}

		public string GetSignatureTemplate()
		{
			string signatureTemplate = "";
			if (licGroupDisplayName.IndexOf("Rookie", StringComparison.Ordinal) != -1)
			{
				signatureTemplate = "signature-rookie.png";
			}
			else if (licGroupDisplayName.IndexOf("Class D", StringComparison.Ordinal) != -1)
			{
				signatureTemplate = "signature-d.png";
			}
			else if (licGroupDisplayName.IndexOf("Class C", StringComparison.Ordinal) != -1)
			{
				signatureTemplate = "signature-c.png";
			}
			else if (licGroupDisplayName.IndexOf("Class B", StringComparison.Ordinal) != -1)
			{
				signatureTemplate = "signature-b.png";
			}
			else if (licGroupDisplayName.IndexOf("Class A", StringComparison.Ordinal) != -1)
			{
				signatureTemplate = "signature-a.png";
			}
			else
			{
				signatureTemplate = "signature-pro.png";
			}

			return signatureTemplate;			
		}
	}

	public class Helmet
	{
		public string l1 { get; set; }

		public string c3 { get; set; }

		public string hp { get; set; }

		public string c1 { get; set; }

		public string c2 { get; set; }
	}
}