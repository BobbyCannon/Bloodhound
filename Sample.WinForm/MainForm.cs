#region References

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Bloodhound;
using Bloodhound.Models;

#endregion

namespace Sample.WinForm
{
	public partial class MainForm : Form
	{
		#region Fields

		private readonly Tracker _tracker;

		#endregion

		#region Constructors

		public MainForm(Tracker tracker)
		{
			_tracker = tracker;
			InitializeComponent();
			Colors.SelectedIndex = 0;
		}

		#endregion

		#region Methods

		private void exceptionNullReference_Click(object sender, EventArgs e)
		{
			try
			{
				string blah = null;
				Text = blah.Length.ToString();
			}
			catch (Exception ex)
			{
				_tracker.AddException(ex);
			}
		}

		private void fileNotFoundException_Click(object sender, EventArgs e)
		{
			try
			{
				throw new FileNotFoundException("Boom, not found.");
			}
			catch (Exception ex)
			{
				_tracker.AddException(ex);
			}
		}

		private void innerException_Click(object sender, EventArgs e)
		{
			try
			{
				var exception = new OutOfMemoryException("No memory left...");
				throw new OverflowException("Overflowing everywhere...", exception);
			}
			catch (Exception ex)
			{
				_tracker.AddException(ex);
			}
		}

		private void Print_Click(object sender, EventArgs e)
		{
			_tracker.AddEvent("Print", new EventValue("Color", Colors.SelectedItem));
		}

		private void randomTimedEvent_Click(object sender, EventArgs e)
		{
			var random = new Random();
			var delay = TimeSpan.FromMilliseconds(random.Next(5000, 15000));

			var timedEvent = _tracker.StartEvent("RandomTimed", new EventValue("Name", Colors.SelectedItem));
			Thread.Sleep(delay);
			timedEvent.Complete();
		}

		#endregion
	}
}