using System;
using System.Collections.Generic;

#nullable disable

namespace SealWebRTC.Models
{
    public partial class Message
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public int? MeetingId { get; set; }
        public int? UserId { get; set; }
        public string MessageText { get; set; }
        public DateTime MessageDate { get; set; }

        public virtual Meeting Meeting { get; set; }
        public virtual User User { get; set; }
    }
}
