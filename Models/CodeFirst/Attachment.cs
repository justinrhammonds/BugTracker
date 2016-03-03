using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst
{
    public class Attachment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }

        public int TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }
    }
}