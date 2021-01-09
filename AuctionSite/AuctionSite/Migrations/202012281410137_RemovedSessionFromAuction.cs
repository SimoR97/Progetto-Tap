namespace AuctionSite.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RemovedSessionFromAuction : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Auction", name: "SessionId", newName: "SessionImpl_SessionId");
            RenameIndex(table: "dbo.Auction", name: "IX_SessionId", newName: "IX_SessionImpl_SessionId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Auction", name: "IX_SessionImpl_SessionId", newName: "IX_SessionId");
            RenameColumn(table: "dbo.Auction", name: "SessionImpl_SessionId", newName: "SessionId");
        }
    }
}
