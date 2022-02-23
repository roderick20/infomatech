using System;
using System.Collections.Generic;

#nullable disable

namespace SealWebRTC.Models
{
    public partial class Typeattention
    {
        public Typeattention()
        {
            Meetings = new HashSet<Meeting>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Meeting> Meetings { get; set; }
    }
}
