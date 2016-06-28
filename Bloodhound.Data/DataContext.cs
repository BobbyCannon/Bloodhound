#region References

using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;
using Bloodhound.Data.Mappings;
using Bloodhound.Models;

#endregion

namespace Bloodhound.Data
{
	[DbConfigurationType(typeof (ModelConfiguration))]
	public class DataContext : DbContext, IDataContext
	{
		#region Constructors

		public DataContext()
			: this("DefaultConnection")
		{
		}

		public DataContext(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
		}

		#endregion

		#region Properties

		public IRepository<Event> Events => new Repository<Event>(Set<Event>());

		public IRepository<EventValue> EventValues => new Repository<EventValue>(Set<EventValue>());

		public IRepository<WidgetFilter> WidgetFilters => new Repository<WidgetFilter>(Set<WidgetFilter>());

		public IRepository<Widget> Widgets => new Repository<Widget>(Set<Widget>());

		#endregion

		#region Methods

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new EventMap());
			modelBuilder.Configurations.Add(new EventValueMap());
			modelBuilder.Configurations.Add(new WidgetFilterMap());
			modelBuilder.Configurations.Add(new WidgetMap());
		}

		#endregion

		#region Classes

		private class DbHistoryContext : HistoryContext
		{
			#region Constructors

			public DbHistoryContext(DbConnection dbConnection, string defaultSchema)
				: base(dbConnection, defaultSchema)
			{
			}

			#endregion

			#region Methods

			protected override void OnModelCreating(DbModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);
				modelBuilder.Entity<HistoryRow>().ToTable("MigrationHistory", "system");
			}

			#endregion
		}

		private class ModelConfiguration : DbConfiguration
		{
			#region Constructors

			public ModelConfiguration()
			{
				SetHistoryContext("System.Data.SqlClient", (connection, defaultSchema) => new DbHistoryContext(connection, defaultSchema));
			}

			#endregion
		}

		#endregion
	}
}