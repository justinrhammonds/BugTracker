using BugTracker.Helpers;
using BugTracker.Models;
using BugTracker.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [Authorize]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        //all authorized users

        public ActionResult Index()
        {
            var users = db.Users.ToList().OrderBy(u=>u.LastName);
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditUserRoles(string id)
        {
            var user = db.Users.Find(id);
            AdminUserViewModel AdminModel = new AdminUserViewModel();
            UserRolesHelper helper = new UserRolesHelper();
            var selected = helper.ListUserRoles(id);
            AdminModel.Roles = new MultiSelectList(db.Roles, "Name", "Name", selected);
            AdminModel.User = user;

            return View(AdminModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUserRoles(AdminUserViewModel model)
        {
            var user = User.Identity.GetUserId();
            UserRolesHelper helper = new UserRolesHelper();
            var currentRoles = helper.ListUserRoles(user);

            foreach (var role in db.Roles.Select(r=>r.Name))
            {
                if (model.SelectedRoles.Contains(role))
                {
                    helper.AddUserToRole(user, role);
                } else
                {
                    helper.RemoveUserFromRole(user, role);
                }
            }

            return RedirectToAction("Index", "Users", model);
        }
    }
}