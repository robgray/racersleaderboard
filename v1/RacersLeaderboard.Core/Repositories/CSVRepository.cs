using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace RacersLeaderboard.Core.Repositories
{
    public interface ICsvRepository
    {
        string CsvFilename { get; }
        List<T> GetAll<T, TMap>() where TMap : ClassMap<T>;
        void SaveCsv(string csvText);
    }

	public class CSVRepository : ICsvRepository
    {
		private static object locker = new object();

		public CSVRepository(string csvFilename)
		{
			CsvFilename = csvFilename;
		}


		public string CsvFilename { get; }

		public List<T> GetAll<T, TMap>()
			where TMap : ClassMap<T>
		{
			List<T> results = new List<T>();

			lock (locker)
			{
				if (File.Exists(CsvFilename))
				{
					using (var csv = new CsvReader(File.OpenText(CsvFilename), CultureInfo.InvariantCulture))
					{
						csv.Configuration.HasHeaderRecord = true;
						csv.Configuration.RegisterClassMap<TMap>();
						results = csv.GetRecords<T>().ToList();
					}
				}
			}

			return results;
		}

		public void SaveCsv(string csvText)
		{
			lock (locker)
			{
				File.WriteAllText(CsvFilename, csvText);
			}
		}
	}
}