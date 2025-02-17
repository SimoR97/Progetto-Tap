﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    [Table("User")]
    public class UserImpl
    {
        [Key, Column(Order = 0)]
        [MinLength(DomainConstraints.MinUserName, ErrorMessage = "Minimum characters are 3"), MaxLength(DomainConstraints.MaxUserName, ErrorMessage = "Minimum characters are 64")]
        public string Username { get; set; }
        [MinLength(DomainConstraints.MinUserPassword, ErrorMessage = "Minimum characters are 4")]
        [Required]
        public string Password { get; set; }
        [Key, Column(Order = 1), ForeignKey("Site")]
        public string SiteName { get; set; }
        public virtual SiteImpl Site { get; set; }
        public virtual ICollection<SessionImpl> Sessions { get; set; }
        public virtual ICollection<AuctionImpl> Auctions { get; set; }
        public UserImpl() { }
        public UserImpl(string username, string password, string siteName)
        {
            Username = username;
            Password = password;
            SiteName = siteName;
        }


    }
    [Table("Auction")]
    public class AuctionImpl
    {
        [Key]
        public int AuctionId { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime EndsOn { get; set; }
        public double CurrentPrice { get; set; }
        public double HighestBid { get; set; }
        public string CurrentWinner { get; set; }
        public bool FirstBid { get; set; }
        [ForeignKey("Site")]
        public string SiteName { get; set; }
        public virtual SiteImpl Site { get; set; }
        public string Username { get; set; }
        [ForeignKey("Username,SiteName")]
        public virtual UserImpl Seller { get; set; }
        private readonly int _idValue;
        public AuctionImpl() { }
        public AuctionImpl(string description, DateTime endsOn, double startingPrice, string siteName, string seller)
        {
            _idValue += 1;
            AuctionId = _idValue;
            Description = description;
            EndsOn = endsOn;
            CurrentPrice = startingPrice;
            HighestBid = startingPrice;
            FirstBid = true;
            CurrentWinner = null;
            SiteName = siteName;
            Username = seller;
           
            
        }

    }
    [Table("Session")]
    public class SessionImpl
    {
        [Key]
        public string SessionId { get; set; }
        [Required]
        public DateTime ValidUntil { get; set; }
        [ForeignKey("Site")]
        public string SiteName { get; set; }
        public virtual SiteImpl Site { get; set; }
        public string Username { get; set; }
        [ForeignKey("Username , SiteName")]
        public virtual UserImpl User { get; set; }
        public virtual ICollection<AuctionImpl> Auctions { get; set; }
        public SessionImpl() { }
        public SessionImpl(DateTime validUntil, string username, string siteName)
        {
            var random = new Random();
            SessionId = random.Next(10000) + username + siteName;
            ValidUntil = validUntil;
            Username = username;
            SiteName = siteName;
        }
    }
    [Table("Site")]
    public class SiteImpl
    {
        [Key]
        [MinLength(DomainConstraints.MinSiteName, ErrorMessage = "Min characters allowed is 1"), MaxLength(DomainConstraints.MaxSiteName, ErrorMessage = "Max characters allowed are 128")]
        public string SiteName { get; set; }
        [Required]
        [Range(DomainConstraints.MinTimeZone, DomainConstraints.MaxTimeZone, ErrorMessage = "Insert a value between -12 and 12")]
        public int TimeZone { get; set; }
        public double MinimumBidIncrement { get; set; }
        public int SessionExpirationInSeconds { get; set; }

        public virtual ICollection<UserImpl> Users { get; set; }
        public virtual ICollection<SessionImpl> Sessions { get; set; }
        public virtual ICollection<AuctionImpl> Auctions { get; set; }
        public SiteImpl() { }
        public SiteImpl(string name, int timeZone, double minimumBidIncrement, int sessionExpirationInSeconds)
        {
            SiteName = name;
            TimeZone = timeZone;
            MinimumBidIncrement = minimumBidIncrement;
            SessionExpirationInSeconds = sessionExpirationInSeconds;
        }

    }

    public class AuctionContext : DbContext
    {
        public DbSet<SiteImpl> Sites { get; set; }
        public DbSet<UserImpl> Users { get; set; }
        public DbSet<SessionImpl> Sessions { get; set; }
        public DbSet<AuctionImpl> Auctions { get; set; }

        public AuctionContext() : base() { }
        public AuctionContext(string connectionString) : base(connectionString) { }

    }
}
