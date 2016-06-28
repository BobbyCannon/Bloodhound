#region References

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bloodhound.PerformanceTests.Annotations;

#endregion

namespace Bloodhound.PerformanceTests
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		#region Fields

		private string _bottomStatus;
		private string _readerStatus;

		#endregion

		#region Constructors

		public MainWindowViewModel()
		{
			Writers = new ObservableCollection<WriterViewModel>();
		}

		#endregion

		#region Properties

		public string BottomStatus
		{
			get { return _bottomStatus; }
			set
			{
				if (value == _bottomStatus)
				{
					return;
				}
				_bottomStatus = value;
				OnPropertyChanged();
			}
		}

		public string ReaderStatus
		{
			get { return _readerStatus; }
			set
			{
				if (value == _readerStatus)
				{
					return;
				}
				_readerStatus = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<WriterViewModel> Writers { get; set; }

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