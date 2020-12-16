using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class Session : ISession
    {
        public string Id { get; }

        public DateTime ValidUntil { get; set; }

        public IUser User { get; }

        public string connectionString { get; set; }

        public IAlarmClock alarmClock { get; set; }
        public Session(string id,DateTime validUntil,User user)
        {
            Id = id;
            ValidUntil = validUntil;
            User = user;
        }
        public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            using (var ctx = new AuctionContext(connectionString))
            {
                var query = ctx.Sessions
                            .Where(s => s.SessionId.Equals(Id))
                            .SingleOrDefault();

                return (null != query && query.ValidUntill > alarmClock.Now);
                   
            }
        }

        public void Logout()
        {
            using (var ctx = new AuctionContext(connectionString))
            {
                var query = ctx.Sessions
                            .Where(s => s.SessionId.Equals(Id))
                            .SingleOrDefault();

                this.ValidUntil = alarmClock.Now;
                query.ValidUntill = this.ValidUntil;
                ctx.SaveChanges();
            }
        }
    }
}
