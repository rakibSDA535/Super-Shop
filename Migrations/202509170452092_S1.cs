namespace Khati.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class S1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ProductItems", "Quantity");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProductItems", "Quantity", c => c.Int(nullable: false));
        }
    }
}
