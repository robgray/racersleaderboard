using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RacersLeaderboard.Models;

namespace RacersLeaderboard.Repositories
{
	interface IDriverInfoRepository
	{
		List<DriverInfo> GetAll();

		DriverInfo Get(string customerId);

		void AddOrUpdate(DriverInfo driver);

		void PersistAll();

		void Load();
	}

	public class DriverInfoRepository : IDriverInfoRepository
	{
		private readonly Dictionary<string, DriverInfo> cache;
		private static object locker = new object();
		private string filename;

		public DriverInfoRepository(string filename)
		{
			cache = new Dictionary<string, DriverInfo>();
			this.filename = filename;	
			Load();
		}

		public void SetFilename(string filename)
		{
			this.filename = filename;
		}

		public List<DriverInfo> GetAll()
		{
			// Load from disk
			lock (locker)
			{
				return cache.Values.ToList();
			}
		}

		public DriverInfo Get(string customerId)
		{
			lock (locker)
			{
				if (cache.ContainsKey(customerId))
					return cache[customerId];
				return null;
			}
		}

		public void AddOrUpdate(DriverInfo driver)
		{
			lock (locker)
			{
				if (cache.ContainsKey(driver.custId))
				{
					cache[driver.custId] = driver;
				}
				else
				{
					cache.Add(driver.custId, driver);
				}
			}
		}

		public void Load()
		{
			lock (locker)
			{
				cache.Clear();
				// load from disk.
				if (File.Exists(filename))
				{
					using (var reader = new StreamReader(filename))
					{
						var contents = reader.ReadToEnd();
						var items = JsonConvert.DeserializeObject<List<DriverInfo>>(contents);
						foreach (var driver in items)
						{
							cache.Add(driver.custId, driver);
						}
					}
				}
			}
		}

		public void PersistAll()
		{
			lock (locker)
			{
				// write to disk
				using (var writer = new JsonTextWriter(new StreamWriter(filename, append: false)))
				{
					var jsonDriver = JsonConvert.SerializeObject(cache.Values.ToList());
					writer.WriteRaw(jsonDriver);					
				}
			}
		}			
	}
}
