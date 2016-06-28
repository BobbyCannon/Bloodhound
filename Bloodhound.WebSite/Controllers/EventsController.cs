#region References

using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using Bloodhound.Data;
using Bloodhound.Models;

#endregion

namespace Bloodhound.WebSite.Controllers
{
	public class EventsController : ODataController
	{
		#region Fields

		private readonly DataContext _context;

		#endregion

		#region Constructors

		public EventsController()
		{
			_context = new DataContext();
		}

		#endregion

		#region Methods

		// GET: odata/Events(5)
		[EnableQuery]
		public SingleResult<Event> GetEvent([FromODataUri] int key)
		{
			return SingleResult.Create(_context.Events.Where(session => session.Id == key));
		}

		// GET: odata/Events
		[EnableQuery(PageSize = 1000)]
		public IQueryable<Event> GetEvents()
		{
			return _context.Events;
		}

		// GET: odata/Events(5)/Values(1)
		[EnableQuery]
		public SingleResult<EventValue> GetEventValue([FromODataUri] int key, [FromODataUri] int relatedKey)
		{
			return SingleResult.Create(_context.Events.Where(m => m.Id == key).SelectMany(m => m.Values).Where(x => x.Id == relatedKey));
		}

		// GET: odata/Events(5)/Values
		[EnableQuery(PageSize = 1000)]
		public IQueryable<EventValue> GetValues([FromODataUri] int key)
		{
			return _context.Events.Where(m => m.Id == key).SelectMany(m => m.Values);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_context.Dispose();
			}
			base.Dispose(disposing);
		}

		private bool EventExists(int key)
		{
			return _context.Events.Count(e => e.Id == key) > 0;
		}

		#endregion
	}
}