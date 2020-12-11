using System;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class Auction : IAuction
    {
        public int Id => throw new NotImplementedException();

        public IUser Seller => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public DateTime EndsOn => throw new NotImplementedException();

        public Auction() { }
        public bool BidOnAuction(ISession session, double offer)
        {
            throw new NotImplementedException();
        }

        public double CurrentPrice()
        {
            throw new NotImplementedException();
        }

        public IUser CurrentWinner()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
    }
}