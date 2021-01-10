using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class User : IUser
    {
        public string Username { get; }
        public string ConnectionString { get; set; }
        public IAlarmClock AlarmClock { get; set; }

        public string SiteName { get; set; }
        public User(string username,string siteName)
        {
            Username = username;
            SiteName = siteName;
        }
        
        public void Delete()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                
                var userToDelete =
                    ctx.Users.SingleOrDefault(s => s.Username.Equals(Username) && s.SiteName.Equals(SiteName)) ??
                    throw new InvalidOperationException(nameof(Username) + "doesn't exist anymore");

                var openAuction = userToDelete.Auctions
                    .Where(s => s.EndsOn > AlarmClock.Now)
                    .ToList();

                foreach (var auctionWinStillOpen in openAuction)
                {
                    if ((auctionWinStillOpen.CurrentWinner != null && auctionWinStillOpen.CurrentWinner.Equals(Username)) ||
                         auctionWinStillOpen.Seller.Username.Equals(Username) )
                        throw new InvalidOperationException(nameof(auctionWinStillOpen.AuctionId) +
                                                            "Cannot delete because  the user is a winner or a seller of an open auction");
                    
                }

                var closedAuctionOwnedByTheUser = ctx.Users
                    .Where(s => s.Username.Equals(Username) && s.SiteName.Equals(SiteName))
                    .Select(s => s.Auctions.Where(auctionClosed => auctionClosed.EndsOn <= AlarmClock.Now))
                    .SingleOrDefault();

                if (WonAuctions().Count() != 0)
                {
                    var closedAuctionWonByTheUser = ctx.Auctions
                        .Where(s => s.CurrentWinner.Equals(Username) && s.SiteName.Equals(SiteName) &&
                                    s.EndsOn <= AlarmClock.Now)
                        .ToList();
                    foreach (var removeCurrentWinnerAuction in closedAuctionWonByTheUser)
                        removeCurrentWinnerAuction.CurrentWinner = null;
                    
                }

                var sessionToDelete = ctx.Users
                    .Where(s => s.Username.Equals(Username) && s.SiteName.Equals(SiteName))
                    .Select(s => s.Sessions)
                    .SingleOrDefault();

                //se l'utente non ha mai creato un'Auction passo un array vuoto di auctionImpl come valore di default
                //elimino le Auction create dall'utente
                ctx.Auctions.RemoveRange(closedAuctionOwnedByTheUser ?? Array.Empty<AuctionImpl>());
                ctx.Sessions.RemoveRange(sessionToDelete ?? Array.Empty<SessionImpl>());
                ctx.Users.Remove(userToDelete);
                ctx.SaveChanges();

            }
        }

        public IEnumerable<IAuction> WonAuctions()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
               
                _ = ctx.Users.SingleOrDefault(s => s.Username.Equals(Username) && s.SiteName.Equals(SiteName)) ??
                    throw new InvalidOperationException(nameof(Username) + "doesn't exist anymore");
                
                var auctionEnded = ctx.Auctions
                    .Where(s => s.EndsOn <= AlarmClock.Now && s.SiteName.Equals(SiteName))
                    .Include(s=>s.Seller)
                    .ToList();

                return WonAuctionsSafe();

                IEnumerable<IAuction> WonAuctionsSafe()
                {
                    foreach (var wonAuction in auctionEnded.Where(wonAuction => wonAuction.CurrentWinner == Username))
                    {
                        yield return new Auction(wonAuction.AuctionId,
                            new User(wonAuction.Seller.Username, wonAuction.Seller.SiteName)
                                {ConnectionString = ConnectionString, AlarmClock = AlarmClock},
                            wonAuction.Description, wonAuction.EndsOn, wonAuction.SiteName);
                    }
                }


            }
        }

        public override bool Equals(object obj)
        {
            return (((User)obj).Username == Username);

        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(User u1, User u2)
        {
            return u1.Equals(u2);
        }

        public static bool operator !=(User u1, User u2)
        {
            return !u1.Equals(u2);
        }


    }
}
