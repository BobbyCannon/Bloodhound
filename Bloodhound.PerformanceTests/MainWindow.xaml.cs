#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Bloodhound.Data;
using Bloodhound.Models;
using Bloodhound.Web;
using Speedy;

#endregion

namespace Bloodhound.PerformanceTests
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Fields

		private readonly BackgroundWorker _reader;
		private Tracker _tracker;
		private readonly Dictionary<BackgroundWorker, WriterViewModel> _writers;

		#endregion

		#region Constructors

		public MainWindow()
		{
			_reader = new BackgroundWorker();
			_reader.WorkerReportsProgress = true;
			_reader.WorkerSupportsCancellation = true;
			_reader.DoWork += ReaderOnDoWork;
			_reader.ProgressChanged += ReaderOnProgressChanged;
			_reader.RunWorkerCompleted += ReaderOnRunWorkerCompleted;

			_writers = new Dictionary<BackgroundWorker, WriterViewModel>(20);

			InitializeComponent();

			ViewModel = new MainWindowViewModel();
			DataContext = ViewModel;
		}

		#endregion

		#region Properties

		public static string Directory { get; set; }

		public MainWindowViewModel ViewModel { get; }

		#endregion

		#region Methods

		private void CreateWriter()
		{
			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += WriterOnDoWork;
			worker.ProgressChanged += WriterOnProgressChanged;
			worker.RunWorkerCompleted += WriterOnRunWorkerCompleted;

			var viewModel = new WriterViewModel();
			viewModel.Name = "Writer " + (_writers.Count + 1);
			_writers.Add(worker, viewModel);
			ViewModel.Writers.Add(viewModel);
			worker.RunWorkerAsync();
		}

		private void MainWindowOnLoaded(object sender, RoutedEventArgs e)
		{
			var server = ConfigurationManager.AppSettings["Server"];
			var client = new WebDataChannel(server);
			Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\BloodHoundPerformance";
			var provider = new RepositoryProvider(Directory, TimeSpan.FromMinutes(1), 1000);
			_tracker = Tracker.Start(client, provider);
		}

		private static string MeasureQuery(Func<int> action, string message)
		{
			var watch = Stopwatch.StartNew();
			var result = action();
			return $"{watch.Elapsed.ToString("mm\\:ss\\:fff")} {result,10} : {message}";
		}

		private static void ReaderOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
		{
			var worker = (BackgroundWorker) sender;
			var builder = new StringBuilder();
			var count = 0;

			while (!worker.CancellationPending)
			{
				builder.Clear();

				using (var context = new DataContext())
				{
					var myContext = context;
					builder.AppendLine();
					builder.AppendLine(MeasureQuery(() => myContext.Events.Count(), "Event Count"));
					builder.AppendLine(MeasureQuery(() => myContext.EventValues.Count(), "Event Value Count"));
					builder.AppendLine(MeasureQuery(() => myContext.Widgets.Count(), "Widget Count"));
					builder.AppendLine(MeasureQuery(() => myContext.WidgetFilters.Count(), "Widget Filter Count"));
				}

				worker.ReportProgress(100, $"Reader {count++}{Environment.NewLine}{builder}{Environment.NewLine}Pausing reader...");
				Thread.Sleep(1000);
			}
		}

		private void ReaderOnProgressChanged(object sender, ProgressChangedEventArgs args)
		{
			if (args.ProgressPercentage == 100)
			{
				ViewModel.ReaderStatus = string.Empty;
			}

			ViewModel.ReaderStatus += args.ProgressPercentage > 0 ? args.UserState + Environment.NewLine : args.UserState;
		}

		private void ReaderOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
		{
			ViewModel.ReaderStatus = "Completed";
		}

		private void ResetButtonClick(object sender, RoutedEventArgs e)
		{
			using (var context = new DataContext())
			{
				context.Database.ExecuteSqlCommand("DELETE FROM Widgets; DELETE FROM Events;");
			}
		}

		private void StartButtonClick(object sender, RoutedEventArgs e)
		{
			var button = (Button) sender;

			if (Equals(button.Content, "Start"))
			{
				_reader.RunWorkerAsync();
				for (var i = 0; i < Environment.ProcessorCount; i++)
				{
					CreateWriter();
				}
				button.Content = "Stop";
			}
			else
			{
				_reader.CancelAsync();
				foreach (var writer in _writers)
				{
					writer.Key.CancelAsync();
				}
				button.Content = "Start";
			}
		}

		private void WindowClosing(object sender, CancelEventArgs e)
		{
			_tracker?.Dispose();
		}

		private static void WriterOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
		{
			var worker = (BackgroundWorker) sender;
			var server = ConfigurationManager.AppSettings["Server"];
			var client = new WebDataChannel(server);
			var provider = new RepositoryProvider(Directory, TimeSpan.FromMinutes(1), 1000);
			var random = new Random(Guid.NewGuid().GetHashCode());
			var count = 0;

			while (!worker.CancellationPending)
			{
				using (var tracker = new Tracker(client, provider))
				{
					worker.ReportProgress(1, $"Starting writer {count++} ...");
					tracker.Start(new EventValue("Count", count));

					decimal eventCount = random.Next(10, 1000);
					for (var i = 1; i <= eventCount; i++)
					{
						var item = tracker.StartEvent($"Event{i}");
						var itemDelay = random.Next(1, 100);
						worker.ReportProgress((int) (i / eventCount * 100), $"{i} of {eventCount}");
						item.Values.Add(new EventValue("Delay", itemDelay));
						Thread.Sleep(itemDelay);
						item.Complete();
					}
				}
			}
		}

		private void WriterOnProgressChanged(object sender, ProgressChangedEventArgs args)
		{
			var worker = (BackgroundWorker) sender;
			var viewModel = _writers[worker];
			viewModel.Progress = args.ProgressPercentage;
			viewModel.Status = args.UserState.ToString();
		}

		private void WriterOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
		{
			var worker = (BackgroundWorker) sender;
			var viewModel = _writers[worker];
			viewModel.Progress = 0;
			viewModel.Status = "Completed";
			ViewModel.Writers.Remove(viewModel);
			_writers.Remove(worker);
		}

		#endregion
	}
}