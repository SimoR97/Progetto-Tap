using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class User : IUser
    {
        public string Username { get; }
        public string ConnectionString { get; set; }
        public string SiteName { get; set; }
        public User(string username,string siteName)
        {
            Username = username;
            SiteName = siteName;
        }
        public void Delete()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAuction> WonAuctions()
        {
            throw new NotImplementedException();
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
