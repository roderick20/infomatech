using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealWebRTC.Hubs
{
    public class TicketHub: Hub
    {
        private readonly EFContext _context;


        public TicketHub(EFContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string group, string user, string message)
        {
            var meetingUniqueId = new Guid(group).ToString();
            var userUniqueId = new Guid(user).ToString();
            var meetingObj = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == meetingUniqueId);
            var userObj = await _context.Users.FirstOrDefaultAsync(m => m.UniqueId == userUniqueId);
            var message1 = new Message();
            message1.UniqueId = Guid.NewGuid().ToString();
            message1.MeetingId = meetingObj.Id;
            message1.UserId = userObj.Id;
            message1.MessageText = message;
            message1.MessageDate = DateTime.Now;
            _context.Add(message1);
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", group, user, message);
        }
    }
}
