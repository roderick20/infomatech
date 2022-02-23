using System;
using System.Collections.Generic;

#nullable disable

namespace SealWebRTC.Models
{
    public partial class Archive
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int? UserId { get; set; }
        public int? MeetingId { get; set; }
        public DateTime Created { get; set; }
        public string Extension { get; set; }

        public virtual Meeting Meeting { get; set; }
        public virtual User User { get; set; }
    }
}
