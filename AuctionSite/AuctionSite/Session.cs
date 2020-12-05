using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class Session : ISession
    {
        public string Id { get; }

        public DateTime ValidUntil { get; }

        public IUser User { get; }

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
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }
    }
}
