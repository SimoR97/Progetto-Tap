using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class Site : ISite
    {
        public string Name => throw new NotImplementedException();

        public int Timezone => throw new NotImplementedException();

        public int SessionExpirationInSeconds => throw new NotImplementedException();

        public double MinimumBidIncrement => throw new NotImplementedException();

        public void CleanupSessions()
        {
            throw new NotImplementedException();
        }

        public void CreateUser(string username, string password)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAuction> GetAuctions(bool onlyNotEnded)
        {
            throw new NotImplementedException();
        }

        public ISession GetSession(string sessionId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISession> GetSessions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IUser> GetUsers()
        {
            throw new NotImplementedException();
        }

        public ISession Login(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
