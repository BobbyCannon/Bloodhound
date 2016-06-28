#region References

using System.Data.Entity.Migrations;

#endregion

namespace Bloodhound.Data.Migrations
{
	public partial class InitialDatabase : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropForeignKey("dbo.WidgetFilters", "WidgetId", "dbo.Widgets");
			DropForeignKey("dbo.EventValues", "EventId", "dbo.Events");
			DropForeignKey("dbo.Events", "ParentId", "dbo.Events");
			DropIndex("dbo.WidgetFilters", new[] { "WidgetId" });
			DropIndex("dbo.EventValues", "IX_EventValues_EventId_Name");
			DropIndex("dbo.Events", "IX_Events_UniqueId");
			DropIndex("dbo.Events", new[] { "ParentId" });
			DropIndex("dbo.Events", "IX_Events_Name");
			DropTable("dbo.Widgets");
			DropTable("dbo.WidgetFilters");
			DropTable("dbo.EventValues");
			DropTable("dbo.Events");
		}

		public override void Up()
		{
			CreateTable(
				"dbo.Events",
				c => new
				{
					Id = c.Int(false, true),
					CompletedOn = c.DateTime(false, 7, storeType: "datetime2"),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2"),
					ElapsedTicks = c.Long(false),
					ElapsedTime = c.Time(false, 7),
					Name = c.String(false, 900),
					ParentId = c.Int(),
					SessionId = c.Guid(false),
					Type = c.Int(false),
					UniqueId = c.Guid(false)
				})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Events", t => t.ParentId)
				.Index(t => t.Name, "IX_Events_Name")
				.Index(t => t.ParentId)
				.Index(t => t.UniqueId, unique: true, name: "IX_Events_UniqueId");

			CreateTable(
				"dbo.EventValues",
				c => new
				{
					Id = c.Int(false, true),
					EventId = c.Int(false),
					Name = c.String(false, 900),
					Value = c.String(false)
				})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Events", t => t.EventId, true)
				.Index(t => new { t.EventId, t.Name }, unique: true, name: "IX_EventValues_EventId_Name");

			CreateTable(
				"dbo.WidgetFilters",
				c => new
				{
					Id = c.Int(false, true),
					Name = c.String(false, 900),
					Type = c.Int(false),
					Value = c.String(false),
					WidgetId = c.Int(false)
				})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Widgets", t => t.WidgetId, true)
				.Index(t => t.WidgetId);

			CreateTable(
				"dbo.Widgets",
				c => new
				{
					Id = c.Int(false, true),
					AggregateBy = c.String(false, 256),
					AggregateByFormat = c.String(false, 256),
					ChartLimit = c.Int(false),
					ChartSize = c.Int(false),
					ChartType = c.Int(false),
					EndDate = c.DateTime(precision: 7, storeType: "datetime2"),
					EventType = c.Int(false),
					GroupBy = c.String(false, 256),
					GroupByFormat = c.String(false, 256),
					Name = c.String(false, 256),
					Order = c.Int(false),
					StartDate = c.DateTime(precision: 7, storeType: "datetime2"),
					TimePeriod = c.Time(precision: 7)
				})
				.PrimaryKey(t => t.Id);

			Sql("CREATE INDEX [IX_Events_SessionId_Type] ON [dbo].[Events] ([SessionId], [Type]) INCLUDE ([Id], [CompletedOn], [CreatedOn], [ElapsedTicks], [ElapsedTime], [Name], [ParentId], [UniqueId])");
			Sql("CREATE INDEX [IX_Events_CreatedOn] ON [Bloodhound].[dbo].[Events] ([CreatedOn]) INCLUDE ([Id], [CompletedOn], [ElapsedTicks], [ElapsedTime], [Name], [ParentId], [SessionId], [Type], [UniqueId])");
			Sql("CREATE INDEX [IX_Events_Type_CreatedOn] ON [dbo].[Events] ([Type], [CreatedOn]) INCLUDE ([Id], [CompletedOn], [ElapsedTicks], [ElapsedTime], [Name], [ParentId], [SessionId], [UniqueId])");
		}

		#endregion
	}
}