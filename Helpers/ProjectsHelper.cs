using BugTracker.Models;
using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Helpers
{
    public class ProjectsHelper
    {
       
            private ApplicationDbContext db = new ApplicationDbContext();

            // T/F - determines whether a user is in a specific project
            public bool IsUserInProject(string userId, int projectId)
            {
                var project = db.Projects.Find(projectId);
                var isAssigned = project.Users.Any(u => u.Id == userId);

                return isAssigned;
            }

            // returns a list of all projects assigned to a specific user
            public IList<Project> ListUserProjects(string userId)
            {
                var user = db.Users.Find(userId);
                return user.Projects.ToList();
            }

            // Assign a project (if not already assigned)
            public void AddUserToProject(string userId, int projectId)
            {
                var user = db.Users.Find(userId);
                var project = db.Projects.Find(projectId);

                project.Users.Add(user);
                db.SaveChanges();
                
            }

            //remove from project (if already assigned)
            public void RemoveUserFromProject(string userId, int projectId)
            {
                var user = db.Users.Find(userId);
                var project = db.Projects.Find(projectId);

                project.Users.Remove(user);
                db.SaveChanges();
            }

            //get a list of all users assigned to a project
            public IList<ApplicationUser> UsersInProject(int projectId)
            {
                var resultList = new List<ApplicationUser>();

                foreach (var user in db.Users)
                {
                    if (IsUserInProject(user.Id, projectId))
                    {
                        resultList.Add(user);
                    }
                }
                return resultList;
            }

            //get a list of all users not in a project
            public IList<ApplicationUser> UsersNotInProject(int projectId)
            {
                var resultList = new List<ApplicationUser>();

                foreach (var user in db.Users)
                {
                    if (!IsUserInProject(user.Id, projectId))
                    {
                        resultList.Add(user);
                    }
                }
                return resultList;
            }
        
    }
}