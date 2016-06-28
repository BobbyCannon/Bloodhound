#region References

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#endregion

namespace Bloodhound.Data
{
	/// <summary>
	/// Represents a repository of entities.
	/// </summary>
	/// <typeparam name="T"> The type of the entity for this repository. </typeparam>
	public interface IRepository<T> : IQueryable<T>
	{
		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		void Add(T entity);

		/// <summary>
		/// Add or update an entity to the repository. The ID of the entity should be a value to update or the default value to
		/// add.
		/// </summary>
		/// <param name="entity"> The entity to be added or updated. </param>
		void AddOrUpdate(T entity);

		/// <summary>
		/// Finds an entity with the given primary key values. If an entity with the given primary key values exists in the
		/// context,
		/// then it is returned immediately without making a request to the store. Otherwise, a request is made to the store for an
		/// entity with the given primary key values and this entity, if found, is attached to the context and returned. If no
		/// entity
		/// is found in the context or the store, then null is returned.
		/// </summary>
		/// <param name="id"> The ID of the entity to locate. </param>
		/// <returns>
		/// The entity if found or null if not found.
		/// </returns>
		T Find(int id);

		/// <summary>
		/// Asynchronously finds an entity with the given primary key values. If an entity with the given primary key values exists
		/// in the context,
		/// then it is returned immediately without making a request to the store. Otherwise, a request is made to the store for an
		/// entity with the given primary key values and this entity, if found, is attached to the context and returned. If no
		/// entity
		/// is found in the context or the store, then null is returned.
		/// </summary>
		/// <param name="id"> The ID of the entity to locate. </param>
		/// <returns>
		/// The entity if found or null if not found.
		/// </returns>
		Task<T> FindAsync(int id);

		/// <summary>
		/// Configures the query to include related entities in the results.
		/// </summary>
		/// <param name="include"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		IQueryable<T> Include(Expression<Func<T, object>> include);

		/// <summary>
		/// Configures the query to include multiple related entities in the results.
		/// </summary>
		/// <param name="includes"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		IQueryable<T> Including(params Expression<Func<T, object>>[] includes);

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="id"> The ID of the entity to remove. </param>
		void Remove(int id);

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="entity"> The entity to remove. </param>
		void Remove(T entity);

		#endregion
	}
}