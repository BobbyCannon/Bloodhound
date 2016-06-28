#region References

using System.Data.Entity.Migrations;

#endregion

namespace Bloodhound.Data.Migrations
{
	public partial class UpdatedWidgetTimePeriod : DbMigration
	{
		#region Methods

		public override void Down()
		{
			AddColumn("dbo.Widgets", "TimePeriod", c => c.Time(precision: 7));
			DropColumn("dbo.Widgets", "TimePeriodTicks");
		}

		public override void Up()
		{
			AddColumn("dbo.Widgets", "TimePeriodTicks", c => c.Long(false));
			DropColumn("dbo.Widgets", "TimePeriod");
		}

		#endregion
	}
}