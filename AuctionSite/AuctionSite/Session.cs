using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using static AuctionSite.BasicControl;

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
            IfNullThrow(toRenew);
            using (var ctx = new AuctionContext(ConnectionString))
            {
                var session = ctx.Sessions
                    .SingleOrDefault(s => s.SessionId.Equals(toRenew.Id))
                    ??throw new InvalidOperationException(nameof(toRenew)+ "The session is no longer available ");
                if (IsValid())
                {
                    ValidUntil = AlarmClock.Now.AddSeconds(session.Site.SessionExpirationInSeconds);
                    session.ValidUntill = ValidUntil;
                    ctx.SaveChanges();
                    
                }
                
               
            }
        }
        public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
        {
            IfNullThrow(description);
            DescriptionEmptyThrow(description);
            StartingPriceLessThanZeroThrow(startingPrice);
                    
            if (IsValid())
            {

                if (endsOn.CompareTo(AlarmClock.Now)>=0)
                {
                
                    using (var ctx = new AuctionContext(ConnectionString))
                    {
                        var session = ctx.Sessions
                            .SingleOrDefault(s => s.SessionId.Equals(Id))
                                      ?? throw new InvalidOperationException("The session is no longer available "); ;

                        RenewedSession(this);

                        var auction = new AuctionImpl(description, endsOn, startingPrice, session.SiteName, session.Username);
                        ctx.Auctions.Add(auction);
                        ctx.SaveChanges();
                        
                        return new Auction(auction.AuctionId, new User(auction.Username,auction.SiteName) { ConnectionString = ConnectionString, AlarmClock=AlarmClock }, description, endsOn, auction.SiteName) { ConnectionString = ConnectionString, AlarmClock = AlarmClock };

                    }
                }

                throw new UnavailableTimeMachineException(nameof(endsOn) + " set  to a future time ");

            }

            throw new  InvalidOperationException();




        }

        public bool IsValid()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                var session = ctx.Sessions
                    .SingleOrDefault(s => s.SessionId.Equals(Id));

                return (null != session && session.ValidUntill > AlarmClock.Now);
                   
            }
        }

        public void Logout()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                if (!IsValid()) throw new InvalidOperationException();
                var session = ctx.Sessions
                    .SingleOrDefault(s => s.SessionId.Equals(Id)) 
                              ?? throw new InvalidOperationException( "The session is no longer available "); ;

                ValidUntil = AlarmClock.Now.Subtract(AlarmClock.Now.TimeOfDay);
                session.ValidUntill = ValidUntil;
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
