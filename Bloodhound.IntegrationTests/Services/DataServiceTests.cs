#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Bloodhound.Models;
using Bloodhound.Models.Data;
using Bloodhound.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestR.PowerShell;

#endregion

namespace Bloodhound.IntegrationTests.Services
{
	[TestClass]
	[Cmdlet(VerbsDiagnostic.Test, "DataService")]
	public class DataServiceTests : TestCmdlet
	{
		#region Methods

		[TestMethod]
		public void AddWidget()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				context.SaveChanges();

				var service = new DataService(context);
				var widget = service.AddWidget(new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day"
				});

				var expected = new[]
				{
					new Widget
					{
						AggregateBy = string.Empty,
						AggregateByFormat = string.Empty,
						AggregateType = string.Empty,
						ChartLimit = 5,
						ChartSize = ChartSize.Small,
						ChartType = ChartType.Line,
						Data = new LineChart
						{
							Datasets = new[]
							{
								new LineChartData
								{
									Label = "Data",
									Data = new decimal[] { 0, 1, 0, 1, 0 },
									FillColor = "rgba(0,133,220,0.5)",
									PointColor = "#0085dc",
									PointHighlightFill = "rgba(0,133,220,0.5)",
									PointHighlightStroke = "rgba(0,133,220,0.5)",
									PointStrokeColor = "#0085dc",
									StrokeColor = "#0085dc"
								}
							},
							Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015", "10/04/2015", "10/05/2015" }
						},
						EventType = EventType.Event,
						GroupBy = "Date",
						GroupByFormat = "MM/dd/yyyy",
						Id = widget.Id,
						Name = "Events Per Day",
						Order = 1,
						StartDate = DateTime.Parse("2015/10/01"),
						EndDate = DateTime.Parse("2015/10/05")
					}
				};

				var actual = service.GetWidgets(DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void AddWidgetWithFilter()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				context.SaveChanges();

				var service = new DataService(context);
				var widget = service.AddWidget(new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Filters = new List<WidgetFilter>(new[]
					{
						new WidgetFilter { Name = "Name", Type = WidgetFilter.WidgetFilterType.Equals, Value = "Event2" }
					})
				});

				var expected = new[]
				{
					new Widget
					{
						AggregateBy = string.Empty,
						AggregateByFormat = string.Empty,
						AggregateType = string.Empty,
						ChartLimit = 5,
						ChartSize = ChartSize.Small,
						ChartType = ChartType.Line,
						Data = new LineChart
						{
							Datasets = new[]
							{
								new LineChartData
								{
									Label = "Data",
									Data = new decimal[] { 0, 0, 0, 1, 0 },
									FillColor = "rgba(0,133,220,0.5)",
									PointColor = "#0085dc",
									PointHighlightFill = "rgba(0,133,220,0.5)",
									PointHighlightStroke = "rgba(0,133,220,0.5)",
									PointStrokeColor = "#0085dc",
									StrokeColor = "#0085dc"
								}
							},
							Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015", "10/04/2015", "10/05/2015" }
						},
						EventType = EventType.Event,
						Filters = new List<WidgetFilter>(new[]
						{
							new WidgetFilter { Name = "Name", Type = WidgetFilter.WidgetFilterType.Equals, Value = "Event2", WidgetId = widget.Id, Widget = widget, Id = widget.Filters.First().Id }
						}),
						GroupBy = "Date",
						GroupByFormat = "MM/dd/yyyy",
						Id = widget.Id,
						Name = "Events Per Day",
						Order = 1,
						StartDate = DateTime.Parse("2015/10/01"),
						EndDate = DateTime.Parse("2015/10/05")
					}
				};

				var actual = service.GetWidgets(DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsByElapsedTime()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Print", DateTime.Parse("2015/10/02 10:42:16 AM")));
				context.Events.Add(CreateEvent(session.SessionId, "Print", DateTime.Parse("2015/10/02 11:15:24 AM")));
				context.Events.Add(CreateEvent(session.SessionId, "Print", DateTime.Parse("2015/10/04 09:44:31 PM")));
				context.Events.Add(CreateEvent(session.SessionId, "Print", DateTime.Parse("2015/10/06 04:35:21 AM")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Pie,
					ChartLimit = 5,
					Name = "Top Prints",
					GroupBy = "ElapsedTime",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Pie,
					Name = "Top Prints",
					Data = new PieChart
					{
						Datasets = new[]
						{
							new PieChartData
							{
								Label = "00:00:00",
								Value = 3,
								Color = "#0085dc",
								HighlightColor = "rgba(0,133,220,0.5)"
							}
						},
						IsDonut = false
					},
					GroupBy = "ElapsedTime",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsByName()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Print", DateTime.Parse("2015/10/02 10:42:16 AM")));
				context.Events.Add(CreateEvent(session.SessionId, "Print", DateTime.Parse("2015/10/02 11:15:24 AM")));
				context.Events.Add(CreateEvent(session.SessionId, "Print", DateTime.Parse("2015/10/04 09:44:31 PM")));
				context.Events.Add(CreateEvent(session.SessionId, "Print", DateTime.Parse("2015/10/06 04:35:21 AM")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Pie,
					ChartLimit = 5,
					Name = "Top Prints",
					GroupBy = "Name",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Pie,
					Name = "Top Prints",
					Data = new PieChart
					{
						Datasets = new[]
						{
							new PieChartData
							{
								Label = "Print",
								Value = 3,
								Color = "#0085dc",
								HighlightColor = "rgba(0,133,220,0.5)"
							}
						},
						IsDonut = false
					},
					GroupBy = "Name",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsBySession()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session1 = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session1);
				context.Events.Add(CreateEvent(session1.SessionId, "Print", DateTime.Parse("2015/10/02 10:42:16 AM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Print", DateTime.Parse("2015/10/02 11:15:24 AM")));
				var session2 = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session2);
				context.Events.Add(CreateEvent(session2.SessionId, "Print", DateTime.Parse("2015/10/04 09:44:31 PM")));
				context.Events.Add(CreateEvent(session2.SessionId, "Print", DateTime.Parse("2015/10/06 04:35:21 AM")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Pie,
					ChartLimit = 5,
					Name = "Events Per Session",
					GroupBy = "SessionId",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Pie,
					Name = "Events Per Session",
					Data = new PieChart
					{
						Datasets = new[]
						{
							new PieChartData
							{
								Label = session1.SessionId.ToString(),
								Value = 2,
								Color = "#0085dc",
								HighlightColor = "rgba(0,133,220,0.5)"
							},
							new PieChartData
							{
								Label = session2.SessionId.ToString(),
								Value = 1,
								Color = "#35aa47",
								HighlightColor = "rgba(53,170,71,0.5)"
							}
						},
						IsDonut = false
					},
					GroupBy = "SessionId",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsBySessionMachineName()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session1 = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session1);
				context.Events.Add(CreateEvent(session1.SessionId, "Print", DateTime.Parse("2015/10/02 10:42:16 AM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Print", DateTime.Parse("2015/10/02 11:15:24 AM")));
				var session2 = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session2);
				context.Events.Add(CreateEvent(session2.SessionId, "Print", DateTime.Parse("2015/10/04 09:44:31 PM")));
				context.Events.Add(CreateEvent(session2.SessionId, "Print", DateTime.Parse("2015/10/06 04:35:21 AM")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Pie,
					ChartLimit = 5,
					Name = "Events Per Session",
					GroupBy = "Session.Machine Name",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Pie,
					Name = "Events Per Session",
					Data = new PieChart
					{
						Datasets = new[]
						{
							new PieChartData
							{
								Label = Environment.MachineName,
								Value = 3,
								Color = "#0085dc",
								HighlightColor = "rgba(0,133,220,0.5)"
							}
						},
						IsDonut = false
					},
					GroupBy = "Session.Machine Name",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsPerDay()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Line,
					Name = "Events Per Day",
					GroupBy = "Date",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Events Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new decimal[] { 0, 1, 0, 1, 0 },
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015", "10/04/2015", "10/05/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsPerDayWithCustomDates()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Bar,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/04"),
					Name = "Events Per Day",
					GroupBy = "Date",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Bar,
					Name = "Events Per Day",
					Data = new BarChart
					{
						Datasets = new[]
						{
							new BarChartData
							{
								Label = "Data",
								Data = new decimal[] { 0, 1, 0, 1 },
								FillColor = "rgba(0,133,220,0.5)",
								HighlightFill = "rgba(0,133,220,0.5)",
								HighlightStroke = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015", "10/04/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/04")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsPerMonthFullYearFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Bar,
					Name = "Events Per Month",
					GroupBy = "Date",
					GroupByFormat = "MM/yyyy",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Bar,
					Name = "Events Per Month",
					Data = new BarChart
					{
						Datasets = new[]
						{
							new BarChartData
							{
								Label = "Data",
								Data = new decimal[] { 2 },
								FillColor = "rgba(0,133,220,0.5)",
								HighlightFill = "rgba(0,133,220,0.5)",
								HighlightStroke = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/2015" }
					},
					GroupBy = "Date",
					GroupByFormat = "MM/yyyy",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsPerMonthShortYearFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Bar,
					Name = "Events Per Month",
					GroupBy = "Date",
					GroupByFormat = "MM/yy",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Bar,
					Name = "Events Per Month",
					Data = new BarChart
					{
						Datasets = new[]
						{
							new BarChartData
							{
								Label = "Data",
								Data = new decimal[] { 2 },
								FillColor = "rgba(0,133,220,0.5)",
								HighlightFill = "rgba(0,133,220,0.5)",
								HighlightStroke = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/15" }
					},
					GroupBy = "Date",
					GroupByFormat = "MM/yy",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsPerYearFullFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Bar,
					Name = "Events Per Year",
					GroupBy = "Date",
					GroupByFormat = "yyyy",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Bar,
					Name = "Events Per Year",
					Data = new BarChart
					{
						Datasets = new[]
						{
							new BarChartData
							{
								Label = "Data",
								Data = new decimal[] { 2 },
								FillColor = "rgba(0,133,220,0.5)",
								HighlightFill = "rgba(0,133,220,0.5)",
								HighlightStroke = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "2015" }
					},
					GroupBy = "Date",
					GroupByFormat = "yyyy",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataEventsPerYearShortFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Bar,
					Name = "Events Per Year",
					GroupBy = "Date",
					GroupByFormat = "yy",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Bar,
					Name = "Events Per Year",
					Data = new BarChart
					{
						Datasets = new[]
						{
							new BarChartData
							{
								Label = "Data",
								Data = new decimal[] { 2 },
								FillColor = "rgba(0,133,220,0.5)",
								HighlightFill = "rgba(0,133,220,0.5)",
								HighlightStroke = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "15" }
					},
					GroupBy = "Date",
					GroupByFormat = "yy",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataSessionsPerDay()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/01")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/02")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/03")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/04")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/05")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Line,
					Name = "Sessions Per Day",
					GroupBy = "Date",
					EventType = EventType.Session
				};

				var expected = new Widget
				{
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Sessions Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new decimal[] { 1, 1, 1, 1, 1 },
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015", "10/04/2015", "10/05/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataSessionsPerDayWithCustomPeriod()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/01")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/02")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/03")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/04")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/05")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartType = ChartType.Line,
					EventType = EventType.Session,
					GroupBy = "Date",
					Name = "Sessions Per Day (3)",
					TimePeriod = TimeSpan.FromDays(2)
				};

				var expected = new Widget
				{
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new decimal[] { 1, 1, 1 },
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/03/2015", "10/04/2015", "10/05/2015" }
					},
					EventType = EventType.Session,
					GroupBy = "Date",
					Name = "Sessions Per Day (3)",
					StartDate = DateTime.Parse("2015/10/03"),
					EndDate = DateTime.Parse("2015/10/05"),
					TimePeriod = TimeSpan.FromDays(2)
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataSessionTimePerDay()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01 6:45:36 PM"), DateTime.Parse("2015/10/01 8:42:36 PM"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/01 6:50:45 PM"), DateTime.Parse("2015/10/01 7:04:52 PM")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/01 7:52:12 PM"), DateTime.Parse("2015/10/01 7:55:24 PM")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/01 8:42:01 PM"), DateTime.Parse("2015/10/01 8:42:36 PM")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/02 9:01:41 AM"), DateTime.Parse("2015/10/02 9:07:12 AM")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/03 11:11:11 PM"), DateTime.Parse("2015/10/03 11:45:21 PM")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					GroupBy = "Date",
					EventType = EventType.Session
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new decimal[] { 70200000000, 3310000000, 20500000000 },
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/03")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/03"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataSessionTimePerDayDayFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session1 = TestHelper.CreateSession(DateTime.Parse("2015/10/01 6:45:36 PM"), DateTime.Parse("2015/10/01 8:42:36 PM"));
				var session2 = TestHelper.CreateSession(DateTime.Parse("2015/10/02 9:01:41 AM"), DateTime.Parse("2015/10/02 9:07:12 AM"));
				var session3 = TestHelper.CreateSession(DateTime.Parse("2015/10/03 11:11:11 PM"), DateTime.Parse("2015/10/03 11:45:21 PM"));

				context.Events.Add(session1);
				context.Events.Add(CreateEvent(session1.SessionId, "Event1", DateTime.Parse("2015/10/01 6:50:45 PM"), DateTime.Parse("2015/10/01 7:04:52 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event2", DateTime.Parse("2015/10/01 7:52:12 PM"), DateTime.Parse("2015/10/01 7:55:24 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event3", DateTime.Parse("2015/10/01 8:42:01 PM"), DateTime.Parse("2015/10/01 8:42:36 PM")));
				context.Events.Add(session2);
				context.Events.Add(session3);
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "d",
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					GroupBy = "Date",
					EventType = EventType.Session
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "d",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new[]
								{
									Math.Round((decimal) (session1.CompletedOn - session1.CreatedOn).TotalDays, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session2.CompletedOn - session2.CreatedOn).TotalDays, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session3.CompletedOn - session3.CreatedOn).TotalDays, 2, MidpointRounding.AwayFromZero)
								},
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/03")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/03"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataSessionTimePerDayHourFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session1 = TestHelper.CreateSession(DateTime.Parse("2015/10/01 6:45:36 PM"), DateTime.Parse("2015/10/01 8:42:36 PM"));
				var session2 = TestHelper.CreateSession(DateTime.Parse("2015/10/02 9:01:41 AM"), DateTime.Parse("2015/10/02 9:07:12 AM"));
				var session3 = TestHelper.CreateSession(DateTime.Parse("2015/10/03 11:11:11 PM"), DateTime.Parse("2015/10/03 11:45:21 PM"));

				context.Events.Add(session1);
				context.Events.Add(CreateEvent(session1.SessionId, "Event1", DateTime.Parse("2015/10/01 6:50:45 PM"), DateTime.Parse("2015/10/01 7:04:52 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event2", DateTime.Parse("2015/10/01 7:52:12 PM"), DateTime.Parse("2015/10/01 7:55:24 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event3", DateTime.Parse("2015/10/01 8:42:01 PM"), DateTime.Parse("2015/10/01 8:42:36 PM")));
				context.Events.Add(session2);
				context.Events.Add(session3);
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "h",
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					GroupBy = "Date",
					EventType = EventType.Session
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "h",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new[]
								{
									Math.Round((decimal) (session1.CompletedOn - session1.CreatedOn).TotalHours, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session2.CompletedOn - session2.CreatedOn).TotalHours, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session3.CompletedOn - session3.CreatedOn).TotalHours, 2, MidpointRounding.AwayFromZero)
								},
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/03")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/03"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataSessionTimePerDayMillisecondFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session1 = TestHelper.CreateSession(DateTime.Parse("2015/10/01 6:45:36 PM"), DateTime.Parse("2015/10/01 8:42:36 PM"));
				var session2 = TestHelper.CreateSession(DateTime.Parse("2015/10/02 9:01:41 AM"), DateTime.Parse("2015/10/02 9:07:12 AM"));
				var session3 = TestHelper.CreateSession(DateTime.Parse("2015/10/03 11:11:11 PM"), DateTime.Parse("2015/10/03 11:45:21 PM"));

				context.Events.Add(session1);
				context.Events.Add(CreateEvent(session1.SessionId, "Event1", DateTime.Parse("2015/10/01 6:50:45 PM"), DateTime.Parse("2015/10/01 7:04:52 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event2", DateTime.Parse("2015/10/01 7:52:12 PM"), DateTime.Parse("2015/10/01 7:55:24 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event3", DateTime.Parse("2015/10/01 8:42:01 PM"), DateTime.Parse("2015/10/01 8:42:36 PM")));
				context.Events.Add(session2);
				context.Events.Add(session3);
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "ms",
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					GroupBy = "Date",
					EventType = EventType.Session
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "ms",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new[]
								{
									Math.Round((decimal) (session1.CompletedOn - session1.CreatedOn).TotalMilliseconds, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session2.CompletedOn - session2.CreatedOn).TotalMilliseconds, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session3.CompletedOn - session3.CreatedOn).TotalMilliseconds, 2, MidpointRounding.AwayFromZero)
								},
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/03")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/03"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataSessionTimePerDayMinuteFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session1 = TestHelper.CreateSession(DateTime.Parse("2015/10/01 6:45:36 PM"), DateTime.Parse("2015/10/01 8:42:36 PM"));
				var session2 = TestHelper.CreateSession(DateTime.Parse("2015/10/02 9:01:41 AM"), DateTime.Parse("2015/10/02 9:07:12 AM"));
				var session3 = TestHelper.CreateSession(DateTime.Parse("2015/10/03 11:11:11 PM"), DateTime.Parse("2015/10/03 11:45:21 PM"));

				context.Events.Add(session1);
				context.Events.Add(CreateEvent(session1.SessionId, "Event1", DateTime.Parse("2015/10/01 6:50:45 PM"), DateTime.Parse("2015/10/01 7:04:52 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event2", DateTime.Parse("2015/10/01 7:52:12 PM"), DateTime.Parse("2015/10/01 7:55:24 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event3", DateTime.Parse("2015/10/01 8:42:01 PM"), DateTime.Parse("2015/10/01 8:42:36 PM")));
				context.Events.Add(session2);
				context.Events.Add(session3);
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "m",
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					GroupBy = "Date",
					EventType = EventType.Session
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "m",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new[]
								{
									Math.Round((decimal) (session1.CompletedOn - session1.CreatedOn).TotalMinutes, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session2.CompletedOn - session2.CreatedOn).TotalMinutes, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session3.CompletedOn - session3.CreatedOn).TotalMinutes, 2, MidpointRounding.AwayFromZero)
								},
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/03")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/03"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataSessionTimePerDaySecondFormat()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session1 = TestHelper.CreateSession(DateTime.Parse("2015/10/01 6:45:36 PM"), DateTime.Parse("2015/10/01 8:42:36 PM"));
				var session2 = TestHelper.CreateSession(DateTime.Parse("2015/10/02 9:01:41 AM"), DateTime.Parse("2015/10/02 9:07:12 AM"));
				var session3 = TestHelper.CreateSession(DateTime.Parse("2015/10/03 11:11:11 PM"), DateTime.Parse("2015/10/03 11:45:21 PM"));

				context.Events.Add(session1);
				context.Events.Add(CreateEvent(session1.SessionId, "Event1", DateTime.Parse("2015/10/01 6:50:45 PM"), DateTime.Parse("2015/10/01 7:04:52 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event2", DateTime.Parse("2015/10/01 7:52:12 PM"), DateTime.Parse("2015/10/01 7:55:24 PM")));
				context.Events.Add(CreateEvent(session1.SessionId, "Event3", DateTime.Parse("2015/10/01 8:42:01 PM"), DateTime.Parse("2015/10/01 8:42:36 PM")));
				context.Events.Add(session2);
				context.Events.Add(session3);
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "s",
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					GroupBy = "Date",
					EventType = EventType.Session
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "s",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Session Time Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new[]
								{
									Math.Round((decimal) (session1.CompletedOn - session1.CreatedOn).TotalSeconds, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session2.CompletedOn - session2.CreatedOn).TotalSeconds, 2, MidpointRounding.AwayFromZero),
									Math.Round((decimal) (session3.CompletedOn - session3.CreatedOn).TotalSeconds, 2, MidpointRounding.AwayFromZero)
								},
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/03")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/03"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataTopFiveCustomValue()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/01"), null, new EventValue("Operating System", "Windows Pro")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/02"), null, new EventValue("Operating System", "Windows Home")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/02"), null, new EventValue("Operating System", "Windows Pro")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/03"), null, new EventValue("Operating System", "Windows Pro")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/04"), null, new EventValue("Operating System", "Windows Home")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/04"), null, new EventValue("Operating System", "Windows Pro")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Pie,
					Name = "Top Five OS",
					GroupBy = "Operating System",
					EventType = EventType.Session
				};

				var expected = new Widget
				{
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Pie,
					Name = "Top Five OS",
					Data = new PieChart
					{
						Datasets = new[]
						{
							new PieChartData { Color = "#0085dc", HighlightColor = "rgba(0,133,220,0.5)", Label = "Windows Pro", Value = 4 },
							new PieChartData { Color = "#35aa47", HighlightColor = "rgba(53,170,71,0.5)", Label = "Windows Home", Value = 2 }
						},
						IsDonut = false
					},
					GroupBy = "Operating System",
					EventType = EventType.Session,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataTopFiveCustomValueWithFilter()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/01"), null, new EventValue("Operating System", "Windows Pro")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/02"), null, new EventValue("Operating System", "Windows Home")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/02"), null, new EventValue("Operating System", "Windows Pro")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/03"), null, new EventValue("Operating System", "Windows Pro")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/04"), null, new EventValue("Operating System", "Windows Home")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/04"), null, new EventValue("Operating System", "Windows Pro")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.List,
					Name = "Top Five OS",
					GroupBy = "Operating System",
					EventType = EventType.Session,
					Filters = new[] { new WidgetFilter { Name = "Operating System", Value = "Windows Pro", Type = WidgetFilter.WidgetFilterType.Equals } }
				};

				var expected = new Widget
				{
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.List,
					Name = "Top Five OS",
					Data = new ListChart
					{
						Datasets = new[]
						{
							new ListChartData { Label = "Windows Pro", Value = 4 }
						}
					},
					GroupBy = "Operating System",
					EventType = EventType.Session,
					Filters = new[] { new WidgetFilter { Name = "Operating System", Value = "Windows Pro", Type = WidgetFilter.WidgetFilterType.Equals } },
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/05")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataWithEmptyFilter()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/01")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/01")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/02")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/02")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/03")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/12/01")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartLimit = 2,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Donut,
					Name = "Foo",
					GroupBy = "Date",
					EventType = EventType.Session,
					Filters = new[] { new WidgetFilter { Name = "Date", Value = "", Type = WidgetFilter.WidgetFilterType.Equals } }
				};

				var expected = new Widget
				{
					ChartLimit = 2,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Donut,
					Name = "Foo",
					Data = new PieChart
					{
						Datasets = new[]
						{
							new PieChartData { Color = "#0085dc", HighlightColor = "rgba(0,133,220,0.5)", Label = "11/02/2015", Value = 2 },
							new PieChartData { Color = "#35aa47", HighlightColor = "rgba(53,170,71,0.5)", Label = "10/01/2015", Value = 1 }
						},
						IsDonut = false
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					Filters = new[] { new WidgetFilter { Name = "Date", Value = "", Type = WidgetFilter.WidgetFilterType.Equals } },
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/12/01")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/12/01"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataWithFilterOnDate()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/10/01")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/01")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/02")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/02")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/03")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/12/01")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartLimit = 2,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Donut,
					Name = "Foo",
					GroupBy = "Date",
					EventType = EventType.Session,
					Filters = new[] { new WidgetFilter { Name = "Date", Value = "2015-11-02", Type = WidgetFilter.WidgetFilterType.Equals } }
				};

				var expected = new Widget
				{
					ChartLimit = 2,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Donut,
					Name = "Foo",
					Data = new PieChart
					{
						Datasets = new[]
						{
							new PieChartData { Color = "#0085dc", HighlightColor = "rgba(0,133,220,0.5)", Label = "11/02/2015", Value = 2 },
							new PieChartData { Color = "#35aa47", HighlightColor = "rgba(53,170,71,0.5)", Label = "10/01/2015", Value = 0 }
						},
						IsDonut = false
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					Filters = new[] { new WidgetFilter { Name = "Date", Value = "2015-11-02", Type = WidgetFilter.WidgetFilterType.Equals } },
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/12/01")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/12/01"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgetDataWithFilterUniqueId()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/01")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/02")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/02")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/11/03")));
				context.Events.Add(TestHelper.CreateSession(DateTime.Parse("2015/12/01")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					ChartLimit = 2,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Donut,
					Name = "Foo",
					GroupBy = "Date",
					EventType = EventType.Session,
					Filters = new[] { new WidgetFilter { Name = "UniqueId", Value = session.UniqueId.ToString(), Type = WidgetFilter.WidgetFilterType.Equals } }
				};

				var expected = new Widget
				{
					ChartLimit = 2,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Donut,
					Name = "Foo",
					Data = new PieChart
					{
						Datasets = new[]
						{
							new PieChartData { Color = "#0085dc", HighlightColor = "rgba(0,133,220,0.5)", Label = "10/01/2015", Value = 1 },
							new PieChartData { Color = "#35aa47", HighlightColor = "rgba(53,170,71,0.5)", Label = "10/02/2015", Value = 0 }
						},
						IsDonut = false
					},
					GroupBy = "Date",
					EventType = EventType.Session,
					Filters = new[] { new WidgetFilter { Name = "UniqueId", Value = session.UniqueId.ToString(), Type = WidgetFilter.WidgetFilterType.Equals } },
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/12/01")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/12/01"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void GetWidgets()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var session = TestHelper.CreateSession(DateTime.Parse("2015/10/01"));
				context.Events.Add(session);
				context.Events.Add(CreateEvent(session.SessionId, "Event1", DateTime.Parse("2015/10/02")));
				context.Events.Add(CreateEvent(session.SessionId, "Event2", DateTime.Parse("2015/10/04")));
				context.Events.Add(CreateEvent(session.SessionId, "Event3", DateTime.Parse("2015/10/06")));
				var widget = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day"
				};
				context.Widgets.Add(widget);
				context.SaveChanges();

				var service = new DataService(context);

				var expected = new[]
				{
					new Widget
					{
						AggregateBy = string.Empty,
						AggregateByFormat = string.Empty,
						AggregateType = string.Empty,
						ChartLimit = 5,
						ChartSize = ChartSize.Small,
						ChartType = ChartType.Line,
						Data = new LineChart
						{
							Datasets = new[]
							{
								new LineChartData
								{
									Label = "Data",
									Data = new decimal[] { 0, 1, 0, 1, 0 },
									FillColor = "rgba(0,133,220,0.5)",
									PointColor = "#0085dc",
									PointHighlightFill = "rgba(0,133,220,0.5)",
									PointHighlightStroke = "rgba(0,133,220,0.5)",
									PointStrokeColor = "#0085dc",
									StrokeColor = "#0085dc"
								}
							},
							Labels = new[] { "10/01/2015", "10/02/2015", "10/03/2015", "10/04/2015", "10/05/2015" }
						},
						GroupBy = "Date",
						GroupByFormat = "MM/dd/yyyy",
						EventType = EventType.Event,
						Id = widget.Id,
						Name = "Events Per Day",
						StartDate = DateTime.Parse("2015/10/01"),
						EndDate = DateTime.Parse("2015/10/05")
					}
				};

				var actual = service.GetWidgets(DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/05"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void MoveWidgetDown()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var widget1 = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Order = 1
				};

				var widget2 = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Order = 2
				};

				context.Widgets.Add(widget1);
				context.Widgets.Add(widget2);
				context.SaveChanges();

				var service = new DataService(context);
				service.MoveWidget(widget1.Id, false);

				Assert.AreEqual(2, context.Widgets.First(x => x.Id == widget1.Id).Order);
				Assert.AreEqual(1, context.Widgets.First(x => x.Id == widget2.Id).Order);
			}
		}

		[TestMethod]
		public void MoveWidgetDownListWidget()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var widget1 = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Order = 1
				};

				var widget2 = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Order = 2
				};

				context.Widgets.Add(widget1);
				context.Widgets.Add(widget2);
				context.SaveChanges();

				var service = new DataService(context);
				service.MoveWidget(widget2.Id, false);

				Assert.AreEqual(1, context.Widgets.First(x => x.Id == widget1.Id).Order);
				Assert.AreEqual(2, context.Widgets.First(x => x.Id == widget2.Id).Order);
			}
		}

		[TestMethod]
		public void MoveWidgetNotFound()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var service = new DataService(context);
				TestHelper.ExpectedException<ArgumentException>(() => service.MoveWidget(1, false), "The widget could not be found by the provided ID.");
			}
		}

		[TestMethod]
		public void MoveWidgetUp()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var widget1 = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Order = 1
				};

				var widget2 = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Order = 2
				};

				context.Widgets.Add(widget1);
				context.Widgets.Add(widget2);
				context.SaveChanges();

				var service = new DataService(context);
				service.MoveWidget(widget2.Id, true);

				Assert.AreEqual(2, context.Widgets.First(x => x.Id == widget1.Id).Order);
				Assert.AreEqual(1, context.Widgets.First(x => x.Id == widget2.Id).Order);
			}
		}

		[TestMethod]
		public void MoveWidgetUpFirstWidget()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var widget1 = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Order = 1
				};

				var widget2 = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day",
					Order = 2
				};

				context.Widgets.Add(widget1);
				context.Widgets.Add(widget2);
				context.SaveChanges();

				var service = new DataService(context);
				service.MoveWidget(widget1.Id, true);

				Assert.AreEqual(1, context.Widgets.First(x => x.Id == widget1.Id).Order);
				Assert.AreEqual(2, context.Widgets.First(x => x.Id == widget2.Id).Order);
			}
		}

		[TestMethod]
		public void PopulateWidgetDataAverageElapsedTimePerEvent()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(CreateEvent(Guid.NewGuid(), "Event1", DateTime.Parse("2015/10/01 8:00:00 AM"), DateTime.Parse("2015/10/01 8:01:30 AM")));
				context.Events.Add(CreateEvent(Guid.NewGuid(), "Event2", DateTime.Parse("2015/10/01 8:00:00 AM"), DateTime.Parse("2015/10/01 8:02:00 AM")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "s",
					AggregateType = "Average",
					ChartType = ChartType.Line,
					Name = "Events Per Day",
					GroupBy = "Date",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "s",
					AggregateType = "Average",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Events Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new decimal[] { 105 },
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/01")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/01"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void PopulateWidgetDataMaximumElapsedTimePerEvent()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(CreateEvent(Guid.NewGuid(), "Event1", DateTime.Parse("2015/10/01 8:00:00 AM"), DateTime.Parse("2015/10/01 8:01:30 AM")));
				context.Events.Add(CreateEvent(Guid.NewGuid(), "Event2", DateTime.Parse("2015/10/01 8:00:00 AM"), DateTime.Parse("2015/10/01 8:02:00 AM")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "s",
					AggregateType = "Maximum",
					ChartType = ChartType.Line,
					Name = "Events Per Day",
					GroupBy = "Date",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "s",
					AggregateType = "Maximum",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Events Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new decimal[] { 120 },
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/01")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/01"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void PopulateWidgetDataMinimumElapsedTimePerEvent()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				context.Events.Add(CreateEvent(Guid.NewGuid(), "Event1", DateTime.Parse("2015/10/01 8:00:00 AM"), DateTime.Parse("2015/10/01 8:01:30 AM")));
				context.Events.Add(CreateEvent(Guid.NewGuid(), "Event2", DateTime.Parse("2015/10/01 8:00:00 AM"), DateTime.Parse("2015/10/01 8:02:00 AM")));
				context.SaveChanges();

				var service = new DataService(context);
				var request = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "s",
					AggregateType = "Minimum",
					ChartType = ChartType.Line,
					Name = "Events Per Day",
					GroupBy = "Date",
					EventType = EventType.Event
				};

				var expected = new Widget
				{
					AggregateBy = "ElapsedTime",
					AggregateByFormat = "s",
					AggregateType = "Minimum",
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					Name = "Events Per Day",
					Data = new LineChart
					{
						Datasets = new[]
						{
							new LineChartData
							{
								Label = "Data",
								Data = new decimal[] { 90 },
								FillColor = "rgba(0,133,220,0.5)",
								PointColor = "#0085dc",
								PointHighlightFill = "rgba(0,133,220,0.5)",
								PointHighlightStroke = "rgba(0,133,220,0.5)",
								PointStrokeColor = "#0085dc",
								StrokeColor = "#0085dc"
							}
						},
						Labels = new[] { "10/01/2015" }
					},
					GroupBy = "Date",
					EventType = EventType.Event,
					StartDate = DateTime.Parse("2015/10/01"),
					EndDate = DateTime.Parse("2015/10/01")
				};

				var actual = service.PopulateWidgetData(request, DateTime.Parse("2015/10/01"), DateTime.Parse("2015/10/01"));
				TestHelper.AreEqual(expected, actual, true);
			}
		}

		[TestMethod]
		public void RemoveWidget()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var widget = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day"
				};

				context.Widgets.Add(widget);
				context.SaveChanges();

				Assert.AreEqual(1, context.Widgets.Count());

				var service = new DataService(context);
				service.RemoveWidget(widget.Id);

				Assert.AreEqual(0, context.Widgets.Count());
			}
		}

		[TestMethod]
		public void RemoveWidgetInvalidId()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var widget = new Widget
				{
					AggregateBy = string.Empty,
					AggregateByFormat = string.Empty,
					AggregateType = string.Empty,
					ChartLimit = 5,
					ChartSize = ChartSize.Small,
					ChartType = ChartType.Line,
					EventType = EventType.Event,
					GroupBy = "Date",
					GroupByFormat = "MM/dd/yyyy",
					Name = "Events Per Day"
				};

				context.Widgets.Add(widget);
				context.SaveChanges();

				Assert.AreEqual(1, context.Widgets.Count());

				var service = new DataService(context);
				TestHelper.ExpectedException<ArgumentException>(() => service.RemoveWidget(2), "The widget could not be found by the provided ID.");

				Assert.AreEqual(1, context.Widgets.Count());
			}
		}

		[TestMethod]
		public void RemoveWidgetShouldReorder()
		{
			using (var context = TestHelper.CreateDataContext())
			{
				var widget1 = new Widget { AggregateBy = string.Empty, AggregateByFormat = string.Empty, AggregateType = string.Empty, GroupBy = "Date", GroupByFormat = "MM/dd/yyyy", Name = "Chart1", Order = 1 };
				var widget2 = new Widget { AggregateBy = string.Empty, AggregateByFormat = string.Empty, AggregateType = string.Empty, GroupBy = "Date", GroupByFormat = "MM/dd/yyyy", Name = "Chart2", Order = 2 };
				var widget3 = new Widget { AggregateBy = string.Empty, AggregateByFormat = string.Empty, AggregateType = string.Empty, GroupBy = "Date", GroupByFormat = "MM/dd/yyyy", Name = "Chart3", Order = 3 };

				context.Widgets.Add(widget1);
				context.Widgets.Add(widget2);
				context.Widgets.Add(widget3);
				context.SaveChanges();

				Assert.AreEqual(3, context.Widgets.Count());

				var service = new DataService(context);
				service.RemoveWidget(widget2.Id);

				var actual = context.Widgets.ToList();
				Assert.AreEqual(2, actual.Count);
				Assert.AreEqual(1, actual[0].Order);
				Assert.AreEqual(widget1.Id, actual[0].Id);
				Assert.AreEqual(2, actual[1].Order);
				Assert.AreEqual(widget3.Id, actual[1].Id);
			}
		}

		[TestMethod]
		public void SessionShouldBeUpdatedWithLastEvent()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var expectedSession = TestHelper.CreateSession(new DateTime(2015, 10, 15, 12, 0, 0));

				using (var context = provider.GetContext())
				{
					context.Events.Add(expectedSession);
					context.SaveChanges();
				}

				var expectedEvent = new Event { CreatedOn = new DateTime(2015, 10, 15, 12, 14, 57), CompletedOn = new DateTime(2015, 10, 15, 13, 11, 9), Name = "Event1", UniqueId = Guid.NewGuid(), SessionId = expectedSession.SessionId };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expectedEvent });
				}

				using (var context = provider.GetContext())
				{
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Session));
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Event));
					var actualSession = context.Events.First(x => x.UniqueId == expectedSession.UniqueId);
					var actualEvent = context.Events.First(x => x.UniqueId == expectedEvent.UniqueId);

					TestHelper.AreEqual(actualEvent.CompletedOn, actualSession.CompletedOn);
					TestHelper.AreEqual(actualSession.ElapsedTime, actualSession.CompletedOn - actualSession.CreatedOn);
				}
			}
		}

		[TestMethod]
		public void WriteEvents()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.SaveChanges();
				}

				var createdOn = DateTime.UtcNow;
				var expected = new Event { CreatedOn = createdOn, CompletedOn = createdOn, Name = "Event1", UniqueId = Guid.NewGuid() };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expected });
				}

				using (var context = provider.GetContext())
				{
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Session));
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Event));
					var actual = context.Events.FirstOrDefault(x => x.UniqueId == expected.UniqueId);
					TestHelper.AreEqual(expected, actual);
				}
			}
		}

		[TestMethod]
		public void WriteEventsDuplicateIdsInSeparateRequest()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();
				var createdOn = DateTime.UtcNow;
				var uniqueId = Guid.NewGuid();

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.Events.Add(new Event { CreatedOn = createdOn, CompletedOn = createdOn, Name = "Event1", UniqueId = uniqueId });
					context.SaveChanges();
				}

				var expected = new Event { CreatedOn = createdOn, CompletedOn = createdOn, Name = "Event2", UniqueId = uniqueId };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);

					TestHelper.ExpectedException<Exception>(() => { service.WriteEvents(new[] { expected }); }, "IX_Events_UniqueId");
				}
			}
		}

		[TestMethod]
		public void WriteEventsShouldCorrectElapsedTime()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.SaveChanges();
				}

				var elapsed = TimeSpan.FromSeconds(46);
				var expected = new Event { CreatedOn = DateTime.UtcNow.AddTicks(elapsed.Ticks * -1), CompletedOn = DateTime.UtcNow, Name = "Event1", UniqueId = Guid.NewGuid() };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expected });
				}

				using (var context = provider.GetContext())
				{
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Session));
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Event));
					var actual = context.Events.FirstOrDefault(x => x.UniqueId == expected.UniqueId);
					Assert.IsNotNull(actual);
					TestHelper.AreEqual(elapsed.Ticks, actual.ElapsedTicks);
				}
			}
		}

		[TestMethod]
		public void WriteEventsWithChildren()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.SaveChanges();
				}

				var expected = new Event(session.SessionId, "Event1");
				expected.Children.Add(new Event(expected.SessionId, "ChildEvent1"));

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expected });
				}

				using (var context = provider.GetContext())
				{
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Session));
					Assert.AreEqual(2, context.Events.Count(x => x.Type == EventType.Event));
					var actual = context.Events.FirstOrDefault(x => x.UniqueId == expected.UniqueId);
					TestHelper.AreEqual(expected, actual);
				}
			}
		}

		[TestMethod]
		public void WriteEventsWithEmptyUniqueId()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.SaveChanges();
				}

				var createdOn = DateTime.UtcNow;
				var expected = new Event { CreatedOn = createdOn, CompletedOn = createdOn, Name = "Event1", UniqueId = Guid.Empty };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expected });
				}

				using (var context = provider.GetContext())
				{
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Session));
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Event));
					var actual = context.Events.FirstOrDefault(x => x.UniqueId == expected.UniqueId);
					Assert.IsNotNull(actual);
					Assert.AreNotEqual(Guid.Empty, actual.UniqueId);
				}
			}
		}

		[TestMethod]
		public void WriteEventsWithInvalidCompletedOn()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.SaveChanges();
				}

				var createdOn = DateTime.UtcNow;
				var expected = new Event { CreatedOn = createdOn, CompletedOn = createdOn.AddDays(-1), Name = "Event1", UniqueId = Guid.NewGuid() };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expected });
				}

				using (var context = provider.GetContext())
				{
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Session));
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Event));
					var actual = context.Events.FirstOrDefault(x => x.UniqueId == expected.UniqueId);
					Assert.IsNotNull(actual);
					TestHelper.AreEqual(actual.CompletedOn, actual.CreatedOn);
				}
			}
		}

		[TestMethod]
		public void WriteEventsWithNewIpAddress()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();
				session.Values.First(x => x.Name == "IP Address").Value = string.Empty;

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.SaveChanges();
				}

				var createdOn = DateTime.UtcNow;
				var expected = new Event { CreatedOn = createdOn, CompletedOn = createdOn, Name = "Event1", UniqueId = Guid.NewGuid(), SessionId = session.UniqueId };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expected }, "127.0.0.2");
				}

				using (var context = provider.GetContext())
				{
					var actual = context.Events.First(x => x.Type == EventType.Session);
					Assert.AreEqual(14, actual.Values.Count);
					Assert.AreEqual("127.0.0.2", actual.Values.First(x => x.Name == "IP Address").Value);
				}
			}
		}

		[TestMethod]
		public void WriteEventsWithoutSession()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var createdOn = DateTime.UtcNow;
				var expected = new Event { CreatedOn = createdOn, CompletedOn = createdOn, Name = "Event1", UniqueId = Guid.NewGuid() };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expected });
				}

				using (var context = provider.GetContext())
				{
					Assert.AreEqual(1, context.Events.Count());
				}
			}
		}

		[TestMethod]
		public void WriteEventsWithSameUniqueId()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.SaveChanges();
				}

				var createdOn = DateTime.UtcNow;
				var uniqueId = Guid.NewGuid();
				var event1 = new Event { CreatedOn = createdOn, CompletedOn = createdOn, Name = "Event1", UniqueId = uniqueId };
				var event2 = new Event { CreatedOn = createdOn, CompletedOn = createdOn, Name = "Event2", UniqueId = uniqueId };

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					TestHelper.ExpectedException<Exception>(() => { service.WriteEvents(new[] { event1, event2 }); }, "IX_Events_UniqueId");
				}
			}
		}

		[TestMethod]
		public void WriteEventsWithSingleValue()
		{
			foreach (var provider in TestHelper.CreateDataContextProviders())
			{
				var session = TestHelper.CreateSession();

				using (var context = provider.GetContext())
				{
					context.Events.Add(session);
					context.SaveChanges();
				}

				var createdOn = DateTime.UtcNow;
				var expectedValue = new EventValue("Event1Value1", "ValueOne");
				var expected = new Event
				{
					CreatedOn = createdOn,
					CompletedOn = createdOn,
					Name = "Event1",
					UniqueId = Guid.NewGuid(),
					Values = new[] { expectedValue }
				};

				using (var context = provider.GetContext())
				{
					var service = new DataService(context);
					service.WriteEvents(new[] { expected });
				}

				using (var context = provider.GetContext())
				{
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Session));
					Assert.AreEqual(1, context.Events.Count(x => x.Type == EventType.Event));
					var actual = context.Events.FirstOrDefault(x => x.UniqueId == expected.UniqueId);
					Assert.IsNotNull(actual);
					TestHelper.AreEqual(expected, actual);
					TestHelper.AreEqual(expectedValue, actual.Values.First());
				}
			}
		}

		private Event CreateEvent(Guid sessionId, string name, DateTime createdOn, DateTime? completedOn = null, params EventValue[] values)
		{
			var response = new Event
			{
				CreatedOn = createdOn,
				CompletedOn = completedOn ?? createdOn,
				Name = name,
				SessionId = sessionId,
				Type = EventType.Event,
				UniqueId = Guid.NewGuid(),
				Values = values.ToList()
			};

			response.ElapsedTime = response.CompletedOn - response.CreatedOn;

			return response;
		}

		#endregion
	}
}