namespace BugTracker.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using Models.CodeFirst;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(
            new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }

            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }

            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "justinrhammonds@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "justinrhammonds@gmail.com",
                    Email = "justinrhammonds@gmail.com",
                    FirstName = "Justin",
                    LastName = "Hammonds"
                }, "1stPassword!");

                var userId = userManager.FindByEmail("justinrhammonds@gmail.com").Id;
                userManager.AddToRole(userId, "Admin");
            }

            context.TicketPriorities.AddRange(
                new List<TicketPriority>()
                {
                    new TicketPriority { Name = "Nice To Have" },
                    new TicketPriority { Name = "Important" },
                    new TicketPriority { Name = "Urgent" },
                    new TicketPriority { Name = "Critical" }
                });

            context.TicketStatuses.AddRange(
                new List<TicketStatus>()
                {
                                new TicketStatus { Name = "Unassigned/Open" },
                                new TicketStatus { Name = "Assigned/In Progress" },
                                new TicketStatus { Name = "Resolved" }
                });

            context.TicketTypes.AddRange(
                new List<TicketType>()
                {
                                new TicketType { Name = "Change Request" },
                                new TicketType { Name = "Design Change Request" },
                                new TicketType { Name = "Support Request" },
                                new TicketType { Name = "Software Bug" },
                                new TicketType { Name = "Documentation Bug" }
                });

        }
    }
}
