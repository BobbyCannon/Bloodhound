#region References

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bloodhound.PerformanceTests.Annotations;

#endregion

namespace Bloodhound.PerformanceTests
{
	public class WriterViewModel : INotifyPropertyChanged
	{
		#region Fields

		private string _name;
		private int _progress;
		private string _status;

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name)
				{
					return;
				}

				_name = value;
				OnPropertyChanged();
			}
		}

		public int Progress
		{
			get { return _progress; }
			set
			{
				if (value == _progress)
				{
					return;
				}
				_progress = value;
				OnPropertyChanged();
			}
		}

		public string Status
		{
			get { return _status; }
			set
			{
				if (value == _status)
				{
					return;
				}
				_status = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Methods

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}