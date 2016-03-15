using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst
{
    public class TicketLog
    {
        public int Id { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string WhatChanged { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public string SubmittedById { get; set; }
        public int TicketId { get; set; }

        public virtual ApplicationUser SubmittedBy { get; set; }
        public virtual Ticket Ticket { get; set; }

    }
}