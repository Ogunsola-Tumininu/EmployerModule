namespace EmployerModules.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MemberChange : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Members", "Pin", c => c.String());
            AlterColumn("dbo.Members", "EmployerName", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Members", "EmployerName", c => c.String(nullable: false));
            AlterColumn("dbo.Members", "Pin", c => c.String(nullable: false));
        }
    }
}
