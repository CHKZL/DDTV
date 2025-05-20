using System.ComponentModel;

namespace Desktop.Models
{
    public class ToolsPageModels : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string FixMessage { set; get; } = string.Empty;
        public string FixTimeMessage { set; get; } = string.Empty;
	}
}

