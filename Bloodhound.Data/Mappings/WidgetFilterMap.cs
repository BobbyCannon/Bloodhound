#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Bloodhound.Models;

#endregion

namespace Bloodhound.Data.Mappings
{
	public class WidgetFilterMap : EntityTypeConfiguration<WidgetFilter>
	{
		#region Constructors

		public WidgetFilterMap()
		{
			// Primary Key
			HasKey(t => t.Id);

			// Table & Column Mappings
			ToTable("WidgetFilters");
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.Name).HasMaxLength(900).IsRequired();
			Property(x => x.Type).IsRequired();
			Property(x => x.Value).IsRequired();

			// Relationships
			HasRequired(x => x.Widget)
				.WithMany(x => x.Filters)
				.HasForeignKey(x => x.WidgetId);
		}

		#endregion
	}
}