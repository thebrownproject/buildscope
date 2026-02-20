using Xunit;

namespace BuildScope.Tests;

public class ChatSessionManagerTests
{
    private ChatSessionManager CreateManager() => new();

    private static ChatMessage UserMsg(string content) =>
        new() { Content = content, Type = MessageType.User };

    private static ChatMessage AssistantMsg(string content) =>
        new() { Content = content, Type = MessageType.Assistant };

    [Fact]
    public void SaveAndLoad_RoundTripsMessages()
    {
        var manager = CreateManager();
        var messages = new List<ChatMessage>
        {
            UserMsg("What are fire requirements?"),
            AssistantMsg("Fire requirements include...")
        };

        manager.SaveChat("ProjectA", messages);
        var loaded = manager.LoadChat("ProjectA");

        Assert.Equal(2, loaded.Count);
        Assert.Equal("What are fire requirements?", loaded[0].Content);
        Assert.Equal(MessageType.User, loaded[0].Type);
        Assert.Equal("Fire requirements include...", loaded[1].Content);
        Assert.Equal(MessageType.Assistant, loaded[1].Type);
    }

    [Fact]
    public void LoadChat_ReturnsEmptyForUnknownProject()
    {
        var manager = CreateManager();
        var loaded = manager.LoadChat("NonExistent");
        Assert.Empty(loaded);
    }

    [Fact]
    public void ProjectSwitching_LoadsCorrectChat()
    {
        var manager = CreateManager();

        manager.SaveChat("ProjectA", new List<ChatMessage>
        {
            UserMsg("Question for A"),
            AssistantMsg("Answer for A")
        });

        manager.SaveChat("ProjectB", new List<ChatMessage>
        {
            UserMsg("Question for B"),
            AssistantMsg("Answer for B")
        });

        var chatA = manager.LoadChat("ProjectA");
        Assert.Equal("Question for A", chatA[0].Content);

        var chatB = manager.LoadChat("ProjectB");
        Assert.Equal("Question for B", chatB[0].Content);
    }

    [Fact]
    public void EnforceMessageCap_TrimsOldestWhenOver20()
    {
        var manager = CreateManager();
        var messages = new List<ChatMessage>();

        // Add 22 messages (11 pairs)
        for (int i = 0; i < 11; i++)
        {
            messages.Add(UserMsg($"Q{i}"));
            messages.Add(AssistantMsg($"A{i}"));
        }
        Assert.Equal(22, messages.Count);

        manager.EnforceMessageCap(messages);

        Assert.Equal(20, messages.Count);
        // Oldest pair (Q0/A0) should be trimmed
        Assert.Equal("Q1", messages[0].Content);
        Assert.Equal("A1", messages[1].Content);
        // Newest should still be last
        Assert.Equal("Q10", messages[18].Content);
        Assert.Equal("A10", messages[19].Content);
    }

    [Fact]
    public void EnforceMessageCap_NoOpWhenUnderLimit()
    {
        var manager = CreateManager();
        var messages = new List<ChatMessage>
        {
            UserMsg("Q1"),
            AssistantMsg("A1")
        };

        manager.EnforceMessageCap(messages);

        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public void EnforceMessageCap_TrimsExactlyToLimit()
    {
        var manager = CreateManager();
        var messages = new List<ChatMessage>();

        // Exactly 20 messages
        for (int i = 0; i < 10; i++)
        {
            messages.Add(UserMsg($"Q{i}"));
            messages.Add(AssistantMsg($"A{i}"));
        }

        manager.EnforceMessageCap(messages);
        Assert.Equal(20, messages.Count);
    }

    [Fact]
    public void GetHistoryForApi_ReturnsLast10UserAssistantMessages()
    {
        var manager = CreateManager();
        var messages = new List<ChatMessage>();

        // Add 14 messages (7 pairs)
        for (int i = 0; i < 7; i++)
        {
            messages.Add(UserMsg($"Q{i}"));
            messages.Add(AssistantMsg($"A{i}"));
        }

        var history = manager.GetHistoryForApi(messages);

        Assert.Equal(10, history.Count);
        // Should be the last 10: Q2,A2,Q3,A3,...Q6,A6
        Assert.Equal("Q2", history[0].Content);
        Assert.Equal("A6", history[9].Content);
    }

    [Fact]
    public void GetHistoryForApi_FiltersNonConversationMessages()
    {
        var manager = CreateManager();
        var messages = new List<ChatMessage>
        {
            new() { Type = MessageType.Welcome },
            UserMsg("Q1"),
            new() { Type = MessageType.Loading },
            AssistantMsg("A1"),
            UserMsg("Q2"),
            AssistantMsg("A2")
        };

        var history = manager.GetHistoryForApi(messages);

        Assert.Equal(4, history.Count);
        Assert.All(history, m =>
            Assert.True(m.Type is MessageType.User or MessageType.Assistant));
    }

    [Fact]
    public void GetHistoryForApi_ReturnsAllWhenUnderLimit()
    {
        var manager = CreateManager();
        var messages = new List<ChatMessage>
        {
            UserMsg("Q1"),
            AssistantMsg("A1"),
            UserMsg("Q2"),
            AssistantMsg("A2")
        };

        var history = manager.GetHistoryForApi(messages);

        Assert.Equal(4, history.Count);
    }

    [Fact]
    public void SaveChat_OverwritesPreviousSession()
    {
        var manager = CreateManager();

        manager.SaveChat("ProjectA", new List<ChatMessage> { UserMsg("Old") });
        manager.SaveChat("ProjectA", new List<ChatMessage> { UserMsg("New") });

        var loaded = manager.LoadChat("ProjectA");
        Assert.Single(loaded);
        Assert.Equal("New", loaded[0].Content);
    }

    [Fact]
    public void ClearChat_RemovesProjectSession()
    {
        var manager = CreateManager();
        manager.SaveChat("ProjectA", new List<ChatMessage> { UserMsg("Q1") });

        manager.ClearChat("ProjectA");

        Assert.Empty(manager.LoadChat("ProjectA"));
    }
}
