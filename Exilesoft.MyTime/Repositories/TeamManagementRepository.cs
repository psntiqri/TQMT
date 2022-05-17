using System;
using System.Collections.Generic;
using System.Linq;
using Exilesoft.Models;
using System.Data.Entity;
using System.Text;
using Exilesoft.MyTime.ViewModels;


namespace Exilesoft.MyTime.Repositories
{
    public class TeamManagementRepository
    {
        private static Context dbContext = new Context();

        public static IList<TeamModel> getTeamList()
        {
            IList<TeamModel> teamList=new List<TeamModel>();
            try
            {
                var TList = dbContext.UserTeams.ToList();
                foreach (var item in TList)
                {
                    var teamModel = new TeamModel
                    {
                        Id = item.Id,
                        Name = item.TeamName
                    };
                    teamList.Add(teamModel);
                }
                
            }
            catch (Exception)
            {
                
            }
            return teamList;
        }
        public static string CreateTeam(ViewModels.TeamManagementViewModel teamModel, string loggedUsername)
        {
            try
            {
                EmployeeEnrollment loggedUser = dbContext.EmployeeEnrollment.FirstOrDefault(a => a.UserName == loggedUsername);
                //EmployeeData employee = EmployeeRepository.GetEmployee(loggedUser.EmployeeId);
                List<int> teamIdList = teamModel.TeamMembers.Select(a => a.Id).ToList<int>();
                string teamIdListString = string.Join(",", teamIdList);
                string teamName = teamModel.TeamName;

                var isTeamExists = dbContext.UserTeams.Include(a => a.CreatedBy).Where(a => a.CreatedBy.EmployeeId == loggedUser.EmployeeId).Any(a => a.TeamName == teamName);
                //bool isTeamExists = dbContext.UserTeams.Any(a => a.TeamName == teamName);
                if (isTeamExists)
                    return "Team name already exists";
                UserTeam team = new UserTeam()
                {
                    CreatedBy = loggedUser,
                    DateCreated = DateTime.Today.Date,
                    TeamMembersIdString = teamIdListString,
                    TeamSharedEmpIdString = "",
                    TeamName = teamName
                };
                dbContext.UserTeams.Add(team);
                dbContext.SaveChanges();
                return "Team successfully created";
            }
            catch (Exception)
            {
                return "Team creation failed";
            }
        }
        public static string UpdateTeam(int teamId, string memberString)
        {

            try
            {
                UserTeam userTeam = dbContext.UserTeams.Where(p => p.Id == teamId).FirstOrDefault();

                userTeam.TeamMembersIdString = memberString;

                dbContext.SaveChanges();

                return "Team successfully Updated";
            }
            catch (Exception)
            {
                return "Team updating failed";
            }

        }
        public static string TeamDelete(int teamId)
        {

            try
            {
                UserTeam userTeam = dbContext.UserTeams.Where(p => p.Id == teamId).FirstOrDefault();

                dbContext.UserTeams.Remove(userTeam);
                dbContext.SaveChanges();
                return "Team successfully deleted";
            }
            catch (Exception)
            {
                return "Team deleting failed";
            }

        }
        /// <summary>
        /// Create dropdown html for the given user
        /// </summary>
        /// <param name="loggedUser">logged user id</param>
        /// <returns>html for the dropdown.</returns>
        public static string TeamDropDownHtml(string loggedUser)
        {
            try
            {
                EmployeeEnrollment user = dbContext.EmployeeEnrollment.FirstOrDefault(c => c.UserName == loggedUser);

                var userTeams = dbContext.UserTeams.Include(a => a.CreatedBy).AsEnumerable().Where(a => (a.CreatedBy.EmployeeId == user.EmployeeId || (a.TeamSharedEmpIdString != null && a.TeamSharedEmpIdString.Contains(user.EmployeeId.ToString()))));

                StringBuilder sb = new StringBuilder();
                sb.Append("<option value=\"0\" selected>---Select Team---</option>");
                foreach (var item in userTeams)
                {
                    sb.Append(string.Format("<option value=\"{0}\">{1}</option>", item.Id, item.TeamName));
                }
                return sb.ToString();
            }
            catch (Exception)
            { }
            return "";
        }

        /// <summary>
        /// Get the team members details of the team
        /// </summary>
        /// <param name="teamId">team id</param>
        public static TeamManagementViewModel GetTeamMembersDetails(int teamId)
        {
            dbContext = new Context();
            TeamManagementViewModel teamManagementViewModel = new TeamManagementViewModel();

            List<TeamMember> teamMembers = new List<TeamMember>();
            List<SharedEmployee> sharedMembers = new List<SharedEmployee>();
            try
            {
                UserTeam team = dbContext.UserTeams.Find(teamId);
                string[] teamMembersIds = string.IsNullOrEmpty(team.TeamMembersIdString) ? null : team.TeamMembersIdString.Split(',');
                string[] sharedEmpMembersIds = string.IsNullOrEmpty(team.TeamSharedEmpIdString) ? null : team.TeamSharedEmpIdString.Split(',');


                
                

                if (teamId == 1159)
                {
                    List<TeamMember>  listTeamMember =new List<TeamMember> ();

                    var allActiveEmpList = EmployeeRepository.GetAllEnableEmployees();
                    var onSiteEmployeeList = OnSiteRepository.GetOnsiteEmployeesByDateRange(new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1), System.DateTime.Today.AddDays(-1));

                   
                    
                    foreach (var item in onSiteEmployeeList)
                    {
                        allActiveEmpList.RemoveAll(t => t.Id == item.EmployeeId);
                    }

                    foreach (var item in allActiveEmpList)
                    {
                       listTeamMember.Add( new TeamMember{ Id=item.Id,
                            Name=item.Name});
                    }
                    teamManagementViewModel.TeamMembers = listTeamMember.ToArray();
                }
                else
                {
                    for (int i = 0; i < teamMembersIds.Length; i++)
                    {
                        TeamMember user = new TeamMember
                        {
                            Id = int.Parse(teamMembersIds[i]),
                            Name = EmployeeRepository.GetEmployee(Convert.ToInt32(teamMembersIds[i])).Name  //dbContext.Employees.Find(Convert.ToInt32(teamMembersIds[i])).Name
                        };
                        teamMembers.Add(user);
                    }
                    teamManagementViewModel.TeamMembers = teamMembers.ToArray();
                }

                if (sharedEmpMembersIds != null)
                {
                    for (int i = 0; i < sharedEmpMembersIds.Length; i++)
                    {
                        SharedEmployee user = new SharedEmployee
                                                  {
                                                      Id = int.Parse(sharedEmpMembersIds[i]),
                                                      Name = EmployeeRepository.GetEmployee(Convert.ToInt32(teamMembersIds[i])).Name

                                                  };
                        sharedMembers.Add(user);
                    }
                    teamManagementViewModel.SharedEmployeeList = sharedMembers.ToArray();
                }
            }

            catch (Exception)
            { }
            return teamManagementViewModel;
        }


        /// <summary>
        /// Gets the shared member details.
        /// </summary>
        /// <param name="teamId">The team identifier.</param>
        /// <returns></returns>
        public static List<Object> GetSharedMemberDetails(int teamId)
        {
            List<Object> teamMembers = new List<Object>();
            try
            {
                UserTeam team = dbContext.UserTeams.Find(teamId);
                string[] sharedMembersIds = team.TeamSharedEmpIdString.Split(',');
                for (int i = 0; i < sharedMembersIds.Length; i++)
                {
                    Object user = new
                    {
                        Id = sharedMembersIds[i],
                        Name = EmployeeRepository.GetEmployee(Convert.ToInt32(sharedMembersIds[i])).Name
                    };
                    teamMembers.Add(user);
                }
            }
            catch (Exception)
            { }
            return teamMembers;
        }

        /// <summary>
        /// Updates the team shared members.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="sharedTeamEmployeeViewModel">The shared team employee view model.</param>
        /// <returns></returns>
        public static string UpdateTeamSharedMembers(int userTeamId, SharedTeamEmployeeViewModel sharedTeamEmployeeViewModel)
        {
            try
            {
                UserTeam userTeam = dbContext.UserTeams.Single(c => c.Id == userTeamId);
                if (sharedTeamEmployeeViewModel.SharedEmployeeList != null)
                {
                    List<int> sharedEmpIdList = sharedTeamEmployeeViewModel.SharedEmployeeList.Select(a => a.Id).ToList<int>();
                    string sharedEmpIdListString = string.Join(",", sharedEmpIdList);
                    userTeam.TeamSharedEmpIdString = sharedEmpIdListString;
                }
                else
                {
                    userTeam.TeamSharedEmpIdString = null;
                }

                dbContext.SaveChanges();
                return "Team successfully shared";
            }
            catch (Exception)
            {
                return "Team  sharing faild";
            }

        }

        public static string getTeamName(int teamId)
        {
           UserTeam team = dbContext.UserTeams.Find(teamId);

           return team.TeamName;
        }
    
    
    }
}