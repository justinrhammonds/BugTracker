using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.CodeFirst;
using BugTracker.ViewModels;
using BugTracker.Helpers;
using Microsoft.AspNet.Identity;
using AspNetIdentity2.Controllers;

namespace BugTracker.Controllers
{
    // Only Admin/Project Managers will view a project page, where they can view projects,
    // create new projects, edit projects, and assign/unassign users to/from projects
    [RequireHttps]
    [Authorize(Roles = "Admin, Project Manager")]
    public class ProjectsController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        public ActionResult Index()
        {
            //if not Admin (must be PM then) display only the projects 'assigned to the PM'
            var user = db.Users.Find(User.Identity.GetUserId());
            if (!User.IsInRole("Admin"))
            {
                return View(user.Projects.OrderByDescending(p=>p.Id).ToList());
            }
            return View(db.Projects.OrderByDescending(p => p.Id).ToList());
        }

        // GET: Projects/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Project project = db.Projects.Find(id);
        //    if (project == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(project);
        //}

        // GET: Projects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        public ActionResult AssignProjectUsers(int projectId)
        {
            var project = db.Projects.Find(projectId);
            ProjectsViewModel ProjectsModel = new ProjectsViewModel();
            UserRolesHelper helper = new UserRolesHelper();
            //populate a list of available developers, with current assigned users passed in
            var availableUsers = helper.UsersInRole("Developer");
            var currentUsers = project.Users.Select(p => p.Id).ToArray();
            ProjectsModel.Users = new MultiSelectList(availableUsers, "Id", "FullName", currentUsers);
            ProjectsModel.Project = project;

            return View(ProjectsModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignProjectUsers(ProjectsViewModel model, int projectId)
        {
            ProjectsHelper helper = new ProjectsHelper();
            //SelectedUsers is an array of userId's selected to be assigned to a project
            //where SelectedUsers is null, at least set it to an empty array here.
            if (model.SelectedUsers == null)
            {
                model.SelectedUsers = new string[] {""};
            }
            //where SelectedUsers contains the userId, add it to the array, else remove it
            foreach (var user in db.Users.Select(u=>u.Id))
            {
                if (model.SelectedUsers.Contains(user))
                {
                    helper.AddUserToProject(user, projectId);
                }
                else
                {
                    helper.RemoveUserFromProject(user, projectId);
                }
            }

            return RedirectToAction("Index", "Projects", model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
