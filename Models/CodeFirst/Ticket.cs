using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst
{
    public class Ticket
    {
        public Ticket()
        {
            this.Attachments = new HashSet<Attachment>();
            this.Comments = new HashSet<Comment>();
            this.TicketLogs = new HashSet<TicketLog>();


        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RepoLocation { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }

        public int CreatedById { get; set; }
        public int ProjectId { get; set; }
        public int AssignedToId { get; set; }
        public int TicketPriorityId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketTypeId { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual Project Project { get; set; }
        public virtual ApplicationUser AssignedTo { get; set; }
        public virtual TicketPriority Priority { get; set; }
        public virtual TicketStatus Status { get; set; }
        public virtual TicketType TicketType { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<TicketLog> TicketLogs { get; set; }


    }
}