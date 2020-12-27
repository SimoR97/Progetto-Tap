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
        public bool IsDeleted()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                var auction = ctx.Auctions.Where(s => s.AuctionId.Equals(Id)).SingleOrDefault();
                if (null == auction) return true;
                return false;
            }
        }
        public bool BidOnAuction(ISession session, double offer)
        {
            if (EndsOn > AlarmClock.Now && !IsDeleted())
            {
                if (offer > 0)
                {
                    if (null != session)
                    {
                        using (var ctx = new AuctionContext(ConnectionString))
                        {
                            var siteNameLoggedU = ctx.Sessions
                                            .Where(s => s.SessionId.Equals(session.Id))
                                            .Select(s => s.User.SiteName)
                                            .SingleOrDefault();
                            var siteNameSellerU = ctx.Users
                                                .Where(s => s.Username.Equals(Seller.Username))
                                                .Select(s => s.SiteName)
                                                .SingleOrDefault();
                            if (session.IsValid() && session.User != Seller && siteNameLoggedU == siteNameSellerU)
                            {
                                
                                var auction = ctx.Auctions
                                            .Where(s => s.AuctionId.Equals(Id))
                                            .SingleOrDefault();

                                var sessionObj = session as Session;

                                sessionObj.RenewedSession(session);

                                if (CurrentWinner() == session.User && offer < auction.HighestBid + auction.Site.MinimunBidIncrement ||
                                    session.User != CurrentWinner() && offer < CurrentPrice() ||
                                    session.User != CurrentWinner() && offer < CurrentPrice() + auction.Site.MinimunBidIncrement && auction.FirstBid == false) return false;

                                if (auction.FirstBid == true )
                                {

                                    auction.HighestBid = offer;
                                    auction.CurrentWinner = session.User.Username;
                                    auction.FirstBid = false;
                                }
                                else if (CurrentWinner() == session.User)
                                {
                                    auction.HighestBid = offer;

                                }
                                else if (auction.FirstBid == false && CurrentWinner()!= session.User && offer > auction.HighestBid)
                                {
                                    auction.CurrentPrice = Math.Min(offer, auction.HighestBid + auction.Site.MinimunBidIncrement);
                                    auction.HighestBid = offer;
                                    auction.CurrentWinner = session.User.Username;
                                }
                                else if (auction.FirstBid == false && CurrentWinner() != session.User && offer <= auction.HighestBid)
                                {
                                    auction.CurrentPrice = Math.Min(offer + auction.Site.MinimunBidIncrement, auction.HighestBid );
                                }

                                ctx.SaveChanges();

                                return true;
                                
                            }
                            else
                                throw new ArgumentException();
                           
                        }
                    }
                    else
                        throw new ArgumentNullException();
                }
                else
                    throw new ArgumentOutOfRangeException();
            }
            else
                throw new InvalidOperationException();
        }

        public double CurrentPrice()
        {
            if (IsDeleted()) throw new InvalidOperationException();
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
                if (IsDeleted()) throw new InvalidOperationException();
                var query = ctx.Auctions
                            .Where(s => s.AuctionId.Equals(Id) && s.SiteName.Equals(SiteName))
                            .SingleOrDefault();
                if (!query.FirstBid && query.CurrentWinner != null)
                    return new User(query.CurrentWinner,query.SiteName) { ConnectionString = ConnectionString };
                else
                    return null;
                            
            }

        }

        public void Delete()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                if (IsDeleted()) throw new InvalidOperationException();
                var query = ctx.Auctions
                            .Where(s => s.AuctionId.Equals(Id) && s.SiteName.Equals(SiteName))
                            .SingleOrDefault();

                ctx.Auctions.Remove(query);
                ctx.SaveChanges();

            }
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