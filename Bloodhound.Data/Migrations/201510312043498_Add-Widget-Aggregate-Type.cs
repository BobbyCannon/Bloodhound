#region References

using System.Data.Entity.Migrations;

#endregion

namespace Bloodhound.Data.Migrations
{
	public partial class AddWidgetAggregateType : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropColumn("dbo.Widgets", "AggregateType");
		}

		public override void Up()
		{
			AddColumn("dbo.Widgets", "AggregateType", c => c.String(false, 256));
		}

		#endregion
	}
}