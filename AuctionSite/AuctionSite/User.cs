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
