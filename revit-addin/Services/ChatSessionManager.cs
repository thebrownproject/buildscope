namespace BuildScope
{
    public class ChatSessionManager
    {
        private const int MessageCap = 20;
        private const int ApiHistoryLimit = 10;

        private readonly Dictionary<string, List<ChatMessage>> _sessions = new();

        public void SaveChat(string projectName, List<ChatMessage> messages)
        {
            _sessions[projectName] = messages
                .Where(m => m.Type is MessageType.User or MessageType.Assistant)
                .Select(m => new ChatMessage
                {
                    Content = m.Content,
                    Type = m.Type,
                    References = m.References,
                    Timestamp = m.Timestamp
                })
                .ToList();
        }

        public List<ChatMessage> LoadChat(string projectName)
        {
            return _sessions.TryGetValue(projectName, out var messages)
                ? new List<ChatMessage>(messages)
                : new List<ChatMessage>();
        }

        public void ClearChat(string projectName) => _sessions.Remove(projectName);

        public void EnforceMessageCap(List<ChatMessage> messages)
        {
            while (messages.Count > MessageCap)
                messages.RemoveAt(0);
        }

        public List<ChatMessage> GetHistoryForApi(List<ChatMessage> messages)
        {
            return messages
                .Where(m => m.Type is MessageType.User or MessageType.Assistant)
                .TakeLast(ApiHistoryLimit)
                .ToList();
        }
    }
}
