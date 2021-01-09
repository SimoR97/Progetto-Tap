namespace AuctionSite.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class FinalConfiguration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Session", "ValidUntil", c => c.DateTime(nullable: false));
            AddColumn("dbo.Site", "MinimumBidIncrement", c => c.Double(nullable: false));
            DropColumn("dbo.Session", "ValidUntill");
            DropColumn("dbo.Site", "MinimunBidIncrement");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Site", "MinimunBidIncrement", c => c.Double(nullable: false));
            AddColumn("dbo.Session", "ValidUntill", c => c.DateTime(nullable: false));
            DropColumn("dbo.Site", "MinimumBidIncrement");
            DropColumn("dbo.Session", "ValidUntil");
        }
    }
}
