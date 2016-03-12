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

        public int Id { get; set; } // automatic
        public string Name { get; set; } // user defined
        public string Description { get; set; } // user defined
        public string RepoLocation { get; set; }  // user defined
        public DateTimeOffset CreatedDate { get; set; } // on submit
        public DateTimeOffset? ModifiedDate { get; set; } // on submit

        public string CreatedById { get; set; } // on submit
        public int ProjectId { get; set; } // user defined, required
        public string AssignedToId { get; set; } // unassigned at first, user defined 
        public int TicketPriorityId { get; set; } // user defined (restrict to adm, pm)
        public int TicketStatusId { get; set; } // user defined (restrict to adm, pm)
        public int TicketTypeId { get; set; } // user defined, optional (in case of beta testers?)

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