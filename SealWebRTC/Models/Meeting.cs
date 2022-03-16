using System;
using System.Collections.Generic;

#nullable disable

namespace SealWebRTC.Models
{
    public partial class Meeting
    {
        public Meeting()
        {
            Archives = new HashSet<Archive>();
            Messages = new HashSet<Message>();
        }

        public int Id { get; set; }
        public string UniqueId { get; set; }
        public DateTime MeetingDateBegin { get; set; }
        public DateTime? MeetingDateEnd { get; set; }
        public int? TypeAttentionId { get; set; }
        public int? UserClientId { get; set; }
        public int? UserManagerId { get; set; }
        public DateTime Created { get; set; }
        public int? Type { get; set; }
        public int? Number { get; set; }
        public int? Status { get; set; }
        public string PeerClient { get; set; }
        public string PeerManager { get; set; }
        public DateTime? DurationBegin { get; set; }
        public DateTime? DurationEnd { get; set; }
        public string PeerIdManager { get; set; }
        public string PeerIdCliente { get; set; }
        public float? Score { get; set; }
        public bool Paid { get; set; }

        public virtual Typeattention TypeAttention { get; set; }
        public virtual User UserClient { get; set; }
        public virtual User UserManager { get; set; }
        public virtual ICollection<Archive> Archives { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
