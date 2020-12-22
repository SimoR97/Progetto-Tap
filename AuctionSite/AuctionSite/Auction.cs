using System;
using System.Linq;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class Auction : IAuction
    {
        public int Id { get; }

        public IUser Seller { get; }

        public string Description { get; }

        public DateTime EndsOn { get;  }

        public string SiteName { get; }
        public string ConnectionString { get; set; }

        public IAlarmClock AlarmClock { get; set; }

        public Auction(int id,IUser seller,string description,DateTime endsOn,string siteName) 
        {
            Id = id;
            Seller = seller;
            Description = description;
            EndsOn = endsOn;
            SiteName = siteName;
        
        }
        public bool BidOnAuction(ISession session, double offer)
        {
            throw new NotImplementedException();
        }

        public double CurrentPrice()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                var query = ctx.Auctions
                            .Where(s => s.AuctionId.Equals(Id) && s.SiteName.Equals(SiteName))
                            .SingleOrDefault();
                return query.CurrentPrice;
            }
        }

        public IUser CurrentWinner()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                var query = ctx.Auctions
                            .Where(s => s.AuctionId.Equals(Id) && s.SiteName.Equals(SiteName))
                            .SingleOrDefault();
                if (!query.FirstBid && query.CurrentWinner != null)
                    return new User(query.CurrentWinner) { connectionString = ConnectionString };
                else
                    return null;
                            
            }

        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return (((Auction)obj).Id == Id && ((Auction)obj).SiteName == SiteName);

        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(Auction u1, Auction u2)
        {
            return u1.Equals(u2);
        }

        public static bool operator !=(Auction u1, Auction u2)
        {
            return !u1.Equals(u2);
        }

    }
}