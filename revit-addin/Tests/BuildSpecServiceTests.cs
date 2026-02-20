using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace BuildSpec.Tests;

public class BuildSpecServiceTests
{
    [Fact]
    public void BuildRequestBody_MatchesEdgeFunctionContract()
    {
        var project = new ProjectContext
        {
            Name = "Test Project",
            BuildingClass = "3",
            State = "VIC",
            ConstructionType = "Type A"
        };
        var history = new List<ChatMessage>
        {
            new() { Content = "What are fire requirements?", Type = MessageType.User },
            new() { Content = "Fire requirements include...", Type = MessageType.Assistant },
            new() { Content = "", Type = MessageType.Loading } // should be filtered out
        };

        var json = BuildSpecService.BuildRequestJson("egress requirements?", project, history);
        var obj = JObject.Parse(json);

        Assert.Equal("egress requirements?", obj["question"]?.ToString());

        var ctx = obj["context"];
        Assert.NotNull(ctx);
        Assert.Equal("3", ctx!["building_class"]?.ToString());
        Assert.Equal("VIC", ctx["state"]?.ToString());
        Assert.Equal("Type A", ctx["construction_type"]?.ToString());

        var chatHistory = obj["chat_history"] as JArray;
        Assert.NotNull(chatHistory);
        Assert.Equal(2, chatHistory!.Count); // Loading message filtered out
        Assert.Equal("user", chatHistory[0]!["role"]?.ToString());
        Assert.Equal("assistant", chatHistory[1]!["role"]?.ToString());
    }

    [Fact]
    public void BuildRequestBody_OmitsChatHistoryWhenNull()
    {
        var project = new ProjectContext
        {
            Name = "Test",
            BuildingClass = "2",
            State = "NSW",
            ConstructionType = "Type B"
        };

        var json = BuildSpecService.BuildRequestJson("question?", project, null);
        var obj = JObject.Parse(json);

        Assert.Null(obj["chat_history"]);
    }

    [Fact]
    public void DeserializeResponse_ParsesAnswerAndReferences()
    {
        var responseJson = """
        {
            "answer": "The egress requirements state that...",
            "references": [
                { "section": "D2.6", "title": "Exits from storeys" },
                { "section": "D2.7", "title": "Travel distances" }
            ]
        }
        """;

        var result = BuildSpecService.ParseResponse(responseJson);

        Assert.Equal("The egress requirements state that...", result.Answer);
        Assert.Equal(2, result.References.Count);
        Assert.Equal("D2.6", result.References[0].Section);
        Assert.Equal("Exits from storeys", result.References[0].Title);
        Assert.Equal("D2.7", result.References[1].Section);
        Assert.Equal("Travel distances", result.References[1].Title);
    }

    [Fact]
    public void DeserializeResponse_HandlesEmptyReferences()
    {
        var responseJson = """
        {
            "answer": "No relevant NCC sections found.",
            "references": []
        }
        """;

        var result = BuildSpecService.ParseResponse(responseJson);

        Assert.Equal("No relevant NCC sections found.", result.Answer);
        Assert.Empty(result.References);
    }

    [Fact]
    public void DeserializeResponse_ThrowsOnInvalidJson()
    {
        Assert.Throws<JsonReaderException>(() =>
            BuildSpecService.ParseResponse("not json"));
    }

    [Fact]
    public void ParseErrorResponse_ExtractsErrorMessage()
    {
        var errorJson = """{"error": "Missing required field: question"}""";
        var message = BuildSpecService.ParseErrorMessage(errorJson);
        Assert.Equal("Missing required field: question", message);
    }

    [Fact]
    public void ParseErrorResponse_FallsBackToRawBody()
    {
        var rawBody = "Internal Server Error";
        var message = BuildSpecService.ParseErrorMessage(rawBody);
        Assert.Equal("Internal Server Error", message);
    }
}
