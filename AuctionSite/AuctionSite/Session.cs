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

        public string ConnectionString { get; set; }

        public IAlarmClock AlarmClock { get; set; }
        public Session(string id,DateTime validUntil,IUser user)
        {
            Id = id;
            ValidUntil = validUntil;
            User = user;
        }
        //rinnovo la sessione  se  è valida  (non scaduta)
        public void RenewedSession(ISession toRenew)
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                var query = ctx.Sessions
                            .Where(s => s.SessionId.Equals(Id))
                            .SingleOrDefault();
                if (IsValid())
                {
                    this.ValidUntil = AlarmClock.Now.AddSeconds(query.Site.SessionExpirationInSeconds);
                    query.ValidUntill = this.ValidUntil;
                    ctx.SaveChanges();
                    
                }
                
               
            }
        }
        public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
        {
            if (null != description)
            {
                if (description.Length > 0)
                {
                    if (startingPrice >= 0)
                    { 
                        if (IsValid())
                        {
                            
                            if (endsOn.CompareTo(AlarmClock.Now)>=0)
                            {
                            
                                using (var ctx = new AuctionContext(ConnectionString))
                                {
                                    var query = ctx.Sessions
                                                .Where(s => s.SessionId.Equals(Id))
                                                .SingleOrDefault();

                                    RenewedSession(this);

                                    var auction = new AuctionImpl(description, endsOn, startingPrice, query.SiteName, query.Username, query.SessionId);
                                    ctx.Auctions.Add(auction);
                                    ctx.SaveChanges();
                                    
                                    return new Auction(auction.AuctionId, new User(auction.Username,auction.SiteName) { ConnectionString = ConnectionString }, description, endsOn, auction.SiteName) { ConnectionString = ConnectionString, AlarmClock = AlarmClock };

                                }
                            }
                            else
                                throw new UnavailableTimeMachineException(nameof(endsOn) + " set  to a future time ");

                        }
                        else
                            throw new  InvalidOperationException();

                }
                    else
                        throw new ArgumentOutOfRangeException(nameof(startingPrice) + "must be positive");

                }
                else
                    throw new ArgumentException();
            }
            else
                throw new ArgumentNullException();
        }

        public bool IsValid()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                var query = ctx.Sessions
                            .Where(s => s.SessionId.Equals(Id))
                            .SingleOrDefault();

                return (null != query && query.ValidUntill > AlarmClock.Now);
                   
            }
        }

        public void Logout()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                if (!this.IsValid()) throw new InvalidOperationException();
                var query = ctx.Sessions
                            .Where(s => s.SessionId.Equals(Id))
                            .SingleOrDefault();

                this.ValidUntil = AlarmClock.Now.Subtract(AlarmClock.Now.TimeOfDay);
                query.ValidUntill = this.ValidUntil;
                ctx.SaveChanges();
            }
        }

        public override bool Equals(object obj)
        {
            return (((Session)obj).Id == Id);

        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(Session u1, Session u2)
        {
            return u1.Equals(u2);
        }

        public static bool operator !=(Session u1, Session u2)
        {
            return !u1.Equals(u2);
        }
    }
}
