using System.Collections.Generic;
using ToDoAPP.Models;

namespace ToDoAPP.Application.Interfaces
{
    public interface IDataStore
    {
        (List<Campaign> Campaigns, bool IsDarkMode) LoadData();
        void SaveData(List<Campaign> campaigns, bool isDarkMode);
    }
}
