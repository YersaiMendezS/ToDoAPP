using System.Collections.Generic;
using ToDoAPP.Models;

namespace ToDoAPP.Application.Services
{
    public class QuestManager
    {
        public List<Campaign> Campaigns { get; private set; }
        
        public QuestManager(List<Campaign> campaigns)
        {
            Campaigns = campaigns ?? new List<Campaign>();
        }

        public void AddCampaign(string name)
        {
            Campaigns.Add(new Campaign { Title = name });
        }

        public void AddMission(Campaign campaign, string name)
        {
            if (campaign.Missions == null) campaign.Missions = new List<Mission>();
            campaign.Missions.Add(new Mission { Title = name });
        }

        public void AddObjective(Mission mission, string description, PriorityItem priority, System.DateTime? dueDate)
        {
            if (mission.Objectives == null) mission.Objectives = new List<Objective>();
            mission.Objectives.Add(new Objective
            {
                Description = description,
                PriorityName = priority.Name,
                PriorityColor = priority.Color,
                DueDate = dueDate,
                Status = 0
            });
        }

        public void UpdateObjectiveStatus(Objective obj, int status)
        {
            if (obj != null) obj.Status = status;
        }

        public void DeleteObjective(Mission mission, Objective obj)
        {
            if (mission != null && obj != null)
                mission.Objectives.Remove(obj);
        }
        
        public List<Mission> GetAllMissions()
        {
            var all = new List<Mission>();
            foreach (var c in Campaigns) 
            {
                if (c.Missions != null) all.AddRange(c.Missions);
            }
            return all;
        }
    }
}
