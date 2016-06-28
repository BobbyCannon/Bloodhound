#region References

using System.Web.Mvc;
using Bloodhound.Data;
using Bloodhound.Services;
using Bloodhound.WebSite.Models.Views;

#endregion

namespace Bloodhound.WebSite.Controllers
{
	public class HomeController : Controller
	{
		#region Fields

		private readonly DataContext _context;

		#endregion

		#region Constructors

		public HomeController()
		{
			_context = new DataContext();
		}

		#endregion

		#region Methods

		public ActionResult About()
		{
			return View();
		}

		public ActionResult Index()
		{
			return View(IndexView.Create(new DataService(_context)));
		}

		#endregion
	}
}