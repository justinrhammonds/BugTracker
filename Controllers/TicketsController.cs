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
using System.IO;

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
            ViewBag.AttachmentMessage = TempData["aMessage"];
            ViewBag.CommentMessage = TempData["cMessage"];
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
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,ProjectId,AssignedToId,RepoLocation,TicketPriorityId,TicketStatusId,TicketTypeId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
                var userId = User.Identity.GetUserId();
                //UPDATE DB WITH EDIT INFO
                db.Tickets.Attach(ticket);
                //hidden: id, projectId
                //change ModifiedDate
                ticket.ModifiedDate = DateTimeOffset.Now;
                db.Entry(ticket).Property(t => t.ModifiedDate).IsModified = true;
                //passed in - Name, Desc, Repo, tPriority, tType
                db.Entry(ticket).Property(t => t.Name).IsModified = true;
                db.Entry(ticket).Property(t => t.Description).IsModified = true;
                db.Entry(ticket).Property(t => t.AssignedToId).IsModified = true;
                db.Entry(ticket).Property(t => t.RepoLocation).IsModified = true;
                db.Entry(ticket).Property(t => t.TicketPriorityId).IsModified = true;
                db.Entry(ticket).Property(t => t.TicketStatusId).IsModified = true;
                db.Entry(ticket).Property(t => t.TicketTypeId).IsModified = true;
                db.SaveChanges();
                db.Tickets.Include(t => t.AssignedTo).FirstOrDefault(t => t.Id == ticket.Id);

                if (userId != ticket.AssignedToId)
                {
                    if (ticket.AssignedToId != null)
                    {
                        var assignedDeveloper = ticket.AssignedTo.Email;
                        var user = db.Users.Find(userId).FullName;
                        var website = "https://jhammonds-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id;
                        var email = new EmailService();
                        var mail = new IdentityMessage
                        {
                            Subject = "A Ticket Was Changed - CodeVariations",
                            Destination = assignedDeveloper,
                            Body = $"Hi {ticket.AssignedTo.FirstName}! A ticket you are currently assigned ('{ticket.Name}') has been updated by {user}. Click below to view the changes: {website}"
                        };
                        email.SendAsync(mail);
                    }
                }


                var modified = ticket.ModifiedDate;
                //If Name Changes
                if (oldTicket?.Name != ticket.Name)
                {
                    TicketLog tl = new TicketLog
                    {
                        ModifiedDate = modified,
                        WhatChanged = "Name",
                        OldValue = oldTicket?.Name,
                        NewValue = ticket.Name,
                        SubmittedById = userId,
                        TicketId = ticket.Id
                    };
                    db.TicketLogs.Add(tl);
                    db.SaveChanges();
                }

                //If Description Changes
                if (oldTicket?.Description != ticket.Description)
                {
                    TicketLog tl = new TicketLog
                    {
                        ModifiedDate = modified,
                        WhatChanged = "Description",
                        OldValue = oldTicket?.Description,
                        NewValue = ticket.Description,
                        SubmittedById = userId,
                        TicketId = ticket.Id
                    };
                    db.TicketLogs.Add(tl);
                    db.SaveChanges();
                }

                //If Repository Location Changes
                if (oldTicket?.RepoLocation != ticket.RepoLocation)
                {
                    TicketLog tl = new TicketLog
                    {
                        ModifiedDate = modified,
                        WhatChanged = "Repository Location",
                        OldValue = oldTicket?.RepoLocation,
                        NewValue = ticket.RepoLocation,
                        SubmittedById = userId,
                        TicketId = ticket.Id
                    };
                    db.TicketLogs.Add(tl);
                    db.SaveChanges();
                }

                //If TicketPriority Changes
                if (oldTicket?.TicketPriorityId != ticket.TicketPriorityId)
                {
                    TicketLog tl = new TicketLog
                    {
                        ModifiedDate = modified,
                        WhatChanged = "Ticket Priority",
                        OldValue = oldTicket?.Priority.Name,
                        NewValue = db.TicketPriorities.FirstOrDefault(t=> t.Id == ticket.TicketPriorityId).Name,
                        SubmittedById = userId,
                        TicketId = ticket.Id
                    };
                    db.TicketLogs.Add(tl);
                    db.SaveChanges();
                }

                //If TicketStatus Changes
                if (oldTicket?.TicketStatusId != ticket.TicketStatusId)
                {
                    TicketLog tl = new TicketLog
                    {
                        ModifiedDate = modified,
                        WhatChanged = "Ticket Status",
                        OldValue = oldTicket?.Status.Name,
                        NewValue = db.TicketStatuses.FirstOrDefault(t => t.Id == ticket.TicketStatusId).Name,
                        SubmittedById = userId,
                        TicketId = ticket.Id
                    };
                    db.TicketLogs.Add(tl);
                    db.SaveChanges();
                }

                //If TicketType Changes
                if (oldTicket?.TicketTypeId != ticket.TicketTypeId)
                {
                    TicketLog tl = new TicketLog
                    {
                        ModifiedDate = modified,
                        WhatChanged = "Ticket Type",
                        OldValue = oldTicket?.TicketType.Name,
                        NewValue = db.TicketTypes.FirstOrDefault(t => t.Id == ticket.TicketTypeId).Name,
                        SubmittedById = userId,
                        TicketId = ticket.Id
                    };
                    db.TicketLogs.Add(tl);
                    db.SaveChanges();
                }

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
            var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
            var userId = User.Identity.GetUserId();
            //update assignment userId, modifiedDate, and status should reflect 'Assigned/In Progress'
            ticket.AssignedToId = model.SelectedUser;
            ticket.ModifiedDate = DateTimeOffset.Now;
            ticket.TicketStatusId = db.TicketStatuses.FirstOrDefault(s => s.Name == "Assigned/In Progress").Id;
            db.SaveChanges();
            TicketLog tl = new TicketLog
            {
                ModifiedDate = ticket.ModifiedDate,
                WhatChanged = "Assignment",
                //OldValue = oldTicket?.AssignedTo.FullName,
                OldValue = db.Tickets.Find(ticketId).AssignedTo.FullName,
                NewValue = ticket.AssignedTo.FullName,
                SubmittedById = userId,
                TicketId = ticket.Id
            };
            db.TicketLogs.Add(tl);
            db.SaveChanges();

            // Send email notification when a user is assigned to a ticket
            var user = db.Users.Find(userId).FullName;
            var website = "https://jhammonds-bugtracker.azurewebsites.net/Tickets";
            var email = new EmailService();
            var mail = new IdentityMessage
            {
                Subject = "You've Been Assigned a Ticket - CodeVariations",
                Destination = ticket.AssignedTo.Email,
                Body = $"Attention {ticket.AssignedTo.FullName}, you have been assigned a new ticket from {user}. To view click here: {website}"
            };
            email.SendAsync(mail);

            return RedirectToAction("Index", "Tickets", model);
        }

        // POST: Tickets/CreateComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateComment([Bind(Include = "Body,TicketId")] Comment comment)
        {
            //assign createddate
            //assign userid
            //pass in ticketid
            if (ModelState.IsValid)
            {
                if (comment.Body == null)
                {
                    TempData["cMessage"] = "Your comment was empty. Please enter a comment before submitting.";
                    //ModelState.AddModelError(string.Empty, "Your comment was empty. Please enter a comment before submitting.");
                }
                else
                {
                    comment.CreatedDate = DateTimeOffset.Now;
                    comment.UserId = User.Identity.GetUserId();
                    db.Comments.Add(comment);
                    db.SaveChanges();
                }

                //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Name", comment.TicketId);
                return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
            }
            ViewData["cMessage"] = "Your comment was empty. Please enter a comment before submitting.";
            return View(comment);
        }

        //POST Tickets/AttachFile
        [HttpPost]
        public ActionResult AttachFile([Bind(Include = "TicketId")] Attachment attachment, HttpPostedFileBase file)
        {
            Ticket ticket = db.Tickets.Find(attachment.TicketId);
            if (ModelState.IsValid)
            {
                if (file == null)
                {
                    TempData["aMessage"] = "Please Upload Your file before submitting.";
                    //ModelState.AddModelError(string.Empty, "Please Upload Your file");
                }
                else if (file.ContentLength > 0)
                {
                    int MaxContentLength = 1024 * 1024 * 3; //3 MB
                    string[] AllowedFileExtensions = new string[] { ".jpg", ".gif", ".png", ".pdf" };

                    if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                    {
                        ViewData["aMessage"] = "You may only upload a file of type: " + string.Join(", ", AllowedFileExtensions);
                        //ModelState.AddModelError(string.Empty, "Please file of type: " + string.Join(", ", AllowedFileExtensions));
                    }

                    else if (file.ContentLength > MaxContentLength)
                    {
                        ViewData["aMessage"] = "Your file is too large. The maximum allowed size is: " + MaxContentLength + " MB";
                        ModelState.AddModelError(string.Empty, "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB");
                    }
                    else
                    {
                        //TO:DO
                        attachment.Title = Path.GetFileName(file.FileName);
                        var fileName = attachment.Title;
                        attachment.FilePath = Path.Combine(Server.MapPath("~/Content/Upload"), fileName);
                        var path = attachment.FilePath;
                        file.SaveAs(path);
                        ModelState.Clear();
                        db.Attachments.Add(attachment);
                        db.SaveChanges();
                        TempData["aMessage"] = "Your file was uploaded successfully";
                    }
                    return View(ticket);
                }
                return RedirectToAction("Details", "Tickets", new { id = attachment.TicketId });
            }
            return View(ticket);
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
