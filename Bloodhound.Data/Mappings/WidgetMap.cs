#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Bloodhound.Models;

#endregion

namespace Bloodhound.Data.Mappings
{
	public class WidgetMap : EntityTypeConfiguration<Widget>
	{
		#region Constructors

		public WidgetMap()
		{
			// Primary Key
			HasKey(t => t.Id);

			// Table & Column Mappings
			ToTable("Widgets");
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.Name).HasMaxLength(256).IsRequired();
			Property(x => x.AggregateBy).HasMaxLength(256).IsRequired();
			Property(x => x.AggregateByFormat).HasMaxLength(256).IsRequired();
			Property(x => x.AggregateType).HasMaxLength(256).IsRequired();
			Property(x => x.ChartSize).IsRequired();
			Property(x => x.ChartType).IsRequired();
			Property(x => x.EndDate).HasColumnType("DateTime2").IsOptional();
			Property(x => x.EventType).IsRequired();
			Property(x => x.GroupBy).HasMaxLength(256).IsRequired();
			Property(x => x.GroupByFormat).HasMaxLength(256).IsRequired();
			Property(x => x.Order).IsRequired();
			Property(x => x.StartDate).HasColumnType("DateTime2").IsOptional();
			Property(x => x.TimePeriodTicks).IsRequired();
			Ignore(x => x.TimePeriod);
			Ignore(x => x.Data);
		}

		#endregion
	}
}