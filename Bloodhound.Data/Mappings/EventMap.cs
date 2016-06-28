#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Bloodhound.Models;

#endregion

namespace Bloodhound.Data.Mappings
{
	public class EventMap : EntityTypeConfiguration<Event>
	{
		#region Constructors

		public EventMap()
		{
			// Primary Key
			HasKey(x => x.Id);

			// Table & Column Mappings
			ToTable("Events");
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.CompletedOn).HasColumnType("DateTime2").IsRequired();
			Property(x => x.CreatedOn).HasColumnType("DateTime2").IsRequired();
			Property(x => x.ElapsedTicks).IsRequired();
			Property(x => x.ElapsedTime).IsRequired();
			Property(x => x.Type).IsRequired();
			Property(x => x.Name).HasMaxLength(900).IsRequired().HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Events_Name") { IsUnique = false }));
			Property(x => x.UniqueId).IsRequired().HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Events_UniqueId") { IsUnique = true }));

			// Relationships
			HasOptional(x => x.Parent)
				.WithMany(x => x.Children)
				.HasForeignKey(x => x.ParentId);
		}

		#endregion
	}
}