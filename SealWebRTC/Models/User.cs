using System;
using System.Collections.Generic;

#nullable disable

namespace SealWebRTC.Models
{
    public partial class User
    {
        public User()
        {
            Archives = new HashSet<Archive>();
            MeetingUserClients = new HashSet<Meeting>();
            MeetingUserManagers = new HashSet<Meeting>();
            Messages = new HashSet<Message>();
        }

        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string TypeDoc { get; set; }
        public string NumberDoc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CellPhone { get; set; }
        public string Password { get; set; }
        public bool Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime LastAccess { get; set; }
        public bool ConfirmationEmail { get; set; }
        public bool? Recovery { get; set; }
        public int Rol { get; set; }
        public bool Enabled { get; set; }
        public string Phone { get; set; }
        public string Suministro { get; set; }
        public string AccessKeyId { get; set; }
        public string ChannelName { get; set; }
        public string Region { get; set; }
        public string SecretAccessKey { get; set; }

        public virtual ICollection<Archive> Archives { get; set; }
        public virtual ICollection<Meeting> MeetingUserClients { get; set; }
        public virtual ICollection<Meeting> MeetingUserManagers { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
