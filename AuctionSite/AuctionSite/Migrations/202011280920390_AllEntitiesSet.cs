namespace AuctionSite.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AllEntitiesSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Auction",
                c => new
                    {
                        AuctionId = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        EndsOn = c.DateTime(nullable: false),
                        CurrentPrice = c.Double(nullable: false),
                        HighestBid = c.Double(nullable: false),
                        CurrentWinner = c.String(),
                        FirstBid = c.Boolean(nullable: false),
                        SiteName = c.String(maxLength: 128),
                        SessionId = c.String(maxLength: 128),
                        Username = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.AuctionId)
                .ForeignKey("dbo.User", t => t.Username)
                .ForeignKey("dbo.Session", t => t.SessionId)
                .ForeignKey("dbo.Site", t => t.SiteName)
                .Index(t => t.SiteName)
                .Index(t => t.SessionId)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 64),
                        Password = c.String(nullable: false),
                        SiteName = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Username)
                .ForeignKey("dbo.Site", t => t.SiteName)
                .Index(t => t.SiteName);
            
            CreateTable(
                "dbo.Session",
                c => new
                    {
                        SessionId = c.String(nullable: false, maxLength: 128),
                        ValidUntill = c.DateTime(nullable: false),
                        SiteName = c.String(maxLength: 128),
                        Username = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.SessionId)
                .ForeignKey("dbo.Site", t => t.SiteName)
                .ForeignKey("dbo.User", t => t.Username)
                .Index(t => t.SiteName)
                .Index(t => t.Username);
            
            CreateTable(
                "dbo.Site",
                c => new
                    {
                        SiteName = c.String(nullable: false, maxLength: 128),
                        TimeZone = c.Int(nullable: false),
                        MinimunBidIncrement = c.Double(nullable: false),
                        SessionExpirationInSeconds = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SiteName);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Auction", "SiteName", "dbo.Site");
            DropForeignKey("dbo.Auction", "SessionId", "dbo.Session");
            DropForeignKey("dbo.Auction", "Username", "dbo.User");
            DropForeignKey("dbo.User", "SiteName", "dbo.Site");
            DropForeignKey("dbo.Session", "Username", "dbo.User");
            DropForeignKey("dbo.Session", "SiteName", "dbo.Site");
            DropIndex("dbo.Session", new[] { "Username" });
            DropIndex("dbo.Session", new[] { "SiteName" });
            DropIndex("dbo.User", new[] { "SiteName" });
            DropIndex("dbo.Auction", new[] { "Username" });
            DropIndex("dbo.Auction", new[] { "SessionId" });
            DropIndex("dbo.Auction", new[] { "SiteName" });
            DropTable("dbo.Site");
            DropTable("dbo.Session");
            DropTable("dbo.User");
            DropTable("dbo.Auction");
        }
    }
}
