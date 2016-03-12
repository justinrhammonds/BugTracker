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
using AspNetIdentity2.Controllers;
using Microsoft.AspNet.Identity;
using BugTracker.ViewModels;
using BugTracker.Helpers;

namespace BugTracker.Controllers
{
    [RequireHttps]
    public class TicketsController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var userId = User.Identity.GetUserId();
            var tickets = db.Tickets.Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType);
            //populate a list of tickets created by the user
            var ownedTickets = tickets.Where(t => t.CreatedById == userId).AsEnumerable();
            //populate a list of all tickets from projects the user's assigned to
            var projTickets = user.Projects.SelectMany(p => p.Tickets).AsEnumerable();
            //combine these two lists into one list, 'projectTickets'
            var projectTickets = ownedTickets.Union(projTickets);
            //Admin sees all tickets ordered by modified date desc, then created desc
            //PMs and Devs see all tickets created by them or in a project assigned to them, ordered by modified date desc then created desc
            //Submitters see all tickets they created, ordered by created date desc
            if (User.IsInRole("Admin"))
            {
                return View(tickets.ToList().OrderByDescending(t => t.ModifiedDate).ThenByDescending(t => t.CreatedDate));
            } else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
            {
                return View(projectTickets.ToList().OrderByDescending(t => t.ModifiedDate).ThenByDescending(t => t.CreatedDate));
            } else
            {
                return View(tickets.Where(t => t.CreatedById == userId).ToList().OrderByDescending(t => t.CreatedDate));
            }

        }

        //GET: Tickets/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        public ActionResult Create()
        {
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,RepoLocation,ProjectId,AssignedToId,TicketPriorityId,TicketStatusId,TicketTypeId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var tStatus = db.TicketStatuses.FirstOrDefault(s => s.Name == "Unassigned/Open");
                //set date, createdby, and a default status of Unassigned/Open
                ticket.CreatedDate = DateTimeOffset.Now;
                ticket.CreatedById = userId;
                ticket.TicketStatusId = tStatus.Id;

                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize]
        public ActionResult Edit(int? ticketId)
        {
            if (ticketId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(ticketId);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            //gets ticketpriority and tickettype lists, and has the current selected value from db
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,ProjectId,RepoLocation,TicketPriorityId,TicketTypeId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Tickets.Attach(ticket);
                //hidden: id, projectId
                //change ModifiedDate
                ticket.ModifiedDate = DateTimeOffset.Now;
                db.Entry(ticket).Property(t => t.ModifiedDate).IsModified = true; //???
                //passed in - Name, Desc, Repo, tPriority, tType
                db.Entry(ticket).Property(t => t.Name).IsModified = true; //???
                db.Entry(ticket).Property(t => t.Description).IsModified = true; //???
                db.Entry(ticket).Property(t => t.RepoLocation).IsModified = true; //???
                db.Entry(ticket).Property(t => t.TicketPriorityId).IsModified = true; //???
                db.Entry(ticket).Property(t => t.TicketTypeId).IsModified = true; //???
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //gets ticketpriority and tickettype lists, and has the current selected value from db
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }
        //Allow only Admin/PMs to GET/POST Assign Users to a ticket
        //GET: AssignUserToTicket
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignUserToTicket(int ticketId)
        {
            var ticket = db.Tickets.Find(ticketId);
            TicketsViewModel model = new TicketsViewModel();
            UserRolesHelper helper = new UserRolesHelper();
            //list of developers assigned to that ticket's project
            var availableUsers = helper.UsersInRole("Developer").Where(u => u.Projects.Any(p => p.Id == ticket.ProjectId)).ToArray();
            var currentAssignedUser = ticket.AssignedToId;
            //populate a select list of availableUsers, with the currentAssignedUser passed in as selected
            model.Users = new SelectList(availableUsers, "Id", "FullName", currentAssignedUser);
            model.Ticket = ticket;
            return View(model);
        }

        //POST: AssignUserToTicket
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignUserToTicket(TicketsViewModel model, int ticketId)
        {
            var ticket = db.Tickets.Find(ticketId);
            //update assignment userId, modifiedDate, and status should reflect 'Assigned/In Progress'
            ticket.AssignedToId = model.SelectedUser;
            ticket.ModifiedDate = DateTimeOffset.Now;
            ticket.TicketStatusId = db.TicketStatuses.FirstOrDefault(s => s.Name == "Assigned/In Progress").Id;
            db.SaveChanges();
            return RedirectToAction("Index", "Tickets", model);
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
