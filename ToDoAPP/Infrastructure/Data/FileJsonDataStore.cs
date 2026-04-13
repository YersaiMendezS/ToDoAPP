using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ToDoAPP.Application.Interfaces;
using ToDoAPP.Models;

namespace ToDoAPP.Infrastructure.Data
{
    public class GamifiedDataLocal
    {
        public List<Campaign> Campaigns { get; set; } = new();
        public List<PriorityItem> CustomPriorities { get; set; } = new();
        public bool IsDarkMode { get; set; } = false;
    }

    public class FileJsonDataStore : IDataStore
    {
        private const string SaveFilePath = "gamified_tasks.json";

        public (List<Campaign> Campaigns, bool IsDarkMode) LoadData()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var data = JsonSerializer.Deserialize<GamifiedDataLocal>(json);
                    if (data != null)
                    {
                        return (data.Campaigns ?? new List<Campaign>(), data.IsDarkMode);
                    }
                }
                catch { }
            }
            return (new List<Campaign>(), false);
        }

        public void SaveData(List<Campaign> campaigns, bool isDarkMode)
        {
            try
            {
                var data = new GamifiedDataLocal
                {
                    Campaigns = campaigns,
                    IsDarkMode = isDarkMode
                };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SaveFilePath, json);
            }
            catch { }
        }
    }
}
