using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class SiteFactory : ISiteFactory
    {
        public void CreateSiteOnDb(string connectionString, string name, int timezone, int sessionExpirationTimeInSeconds, double minimumBidIncrement)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetSiteNames(string connectionString)
        {
            throw new NotImplementedException();
        }

        public int GetTheTimezoneOf(string connectionString, string name)
        {
            throw new NotImplementedException();
        }

        public ISite LoadSite(string connectionString, string name, IAlarmClock alarmClock)
        {
            throw new NotImplementedException();
        }

        public void Setup(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
