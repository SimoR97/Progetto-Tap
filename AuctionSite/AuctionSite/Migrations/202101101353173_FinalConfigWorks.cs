namespace AuctionSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FinalConfigWorks : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Auction", "Username", "dbo.User");
            DropForeignKey("dbo.Session", "Username", "dbo.User");
            DropForeignKey("dbo.User", "SiteName", "dbo.Site");
            DropIndex("dbo.Auction", new[] { "SiteName" });
            DropIndex("dbo.Auction", new[] { "Username" });
            DropIndex("dbo.User", new[] { "SiteName" });
            DropIndex("dbo.Session", new[] { "Username" });
            DropPrimaryKey("dbo.User");
            AlterColumn("dbo.User", "SiteName", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.User", new[] { "Username", "SiteName" });
            CreateIndex("dbo.Auction", new[] { "Username", "SiteName" });
            CreateIndex("dbo.User", "SiteName");
            CreateIndex("dbo.Session", new[] { "Username", "SiteName" });
            AddForeignKey("dbo.Auction", new[] { "Username", "SiteName" }, "dbo.User", new[] { "Username", "SiteName" });
            AddForeignKey("dbo.Session", new[] { "Username", "SiteName" }, "dbo.User", new[] { "Username", "SiteName" });
            AddForeignKey("dbo.User", "SiteName", "dbo.Site", "SiteName", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.User", "SiteName", "dbo.Site");
            DropForeignKey("dbo.Session", new[] { "Username", "SiteName" }, "dbo.User");
            DropForeignKey("dbo.Auction", new[] { "Username", "SiteName" }, "dbo.User");
            DropIndex("dbo.Session", new[] { "Username", "SiteName" });
            DropIndex("dbo.User", new[] { "SiteName" });
            DropIndex("dbo.Auction", new[] { "Username", "SiteName" });
            DropPrimaryKey("dbo.User");
            AlterColumn("dbo.User", "SiteName", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.User", "Username");
            CreateIndex("dbo.Session", "Username");
            CreateIndex("dbo.User", "SiteName");
            CreateIndex("dbo.Auction", "Username");
            CreateIndex("dbo.Auction", "SiteName");
            AddForeignKey("dbo.User", "SiteName", "dbo.Site", "SiteName");
            AddForeignKey("dbo.Session", "Username", "dbo.User", "Username");
            AddForeignKey("dbo.Auction", "Username", "dbo.User", "Username");
        }
    }
}
