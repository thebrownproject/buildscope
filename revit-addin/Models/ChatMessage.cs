using System.ComponentModel;
using Newtonsoft.Json;

namespace BuildSpec
{
    public enum MessageType
    {
        User,
        Assistant,
        Welcome,
        Loading
    }

    public class NccReference
    {
        [JsonProperty("section")]
        public string Section { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";
    }

    public class ChatMessage : INotifyPropertyChanged
    {
        private string _content = "";
        private MessageType _type;

        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        public MessageType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public List<NccReference> References { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
