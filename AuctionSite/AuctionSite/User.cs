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

        public string connectionString { get; set; }
        public  User(string username)
        {
            Username = username;
        }
        public void Delete()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAuction> WonAuctions()
        {
            throw new NotImplementedException();
        }
    }
}
