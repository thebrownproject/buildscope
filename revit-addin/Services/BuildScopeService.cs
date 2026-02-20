using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BuildScope
{
    public class QueryResponse
    {
        [JsonProperty("answer")]
        public string Answer { get; set; } = "";

        [JsonProperty("references")]
        public List<NccReference> References { get; set; } = new();
    }

    public class BuildScopeService
    {
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        private readonly string _supabaseUrl;
        private readonly string _apiKey;

        public BuildScopeService(string supabaseUrl, string apiKey)
        {
            _supabaseUrl = supabaseUrl.TrimEnd('/');
            _apiKey = apiKey;
        }

        public async Task<QueryResponse> QueryAsync(
            string question,
            ProjectContext project,
            List<ChatMessage>? chatHistory = null)
        {
            var json = BuildRequestJson(question, project, chatHistory);
            var endpoint = $"{_supabaseUrl}/functions/v1/ncc-query";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("apikey", _apiKey);

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = ParseErrorMessage(body);
                throw new HttpRequestException(
                    $"NCC query failed ({(int)response.StatusCode}): {errorMsg}");
            }

            return ParseResponse(body);
        }

        // Exposed for testing -- builds the JSON request body matching Edge Function contract
        public static string BuildRequestJson(
            string question,
            ProjectContext project,
            List<ChatMessage>? chatHistory)
        {
            var obj = new JObject
            {
                ["question"] = question,
                ["context"] = new JObject
                {
                    ["building_class"] = project.BuildingClass,
                    ["state"] = project.State,
                    ["construction_type"] = project.ConstructionType
                }
            };

            if (chatHistory != null)
            {
                var filtered = chatHistory
                    .Where(m => m.Type is MessageType.User or MessageType.Assistant)
                    .Select(m => new JObject
                    {
                        ["role"] = m.Type == MessageType.User ? "user" : "assistant",
                        ["content"] = m.Content
                    });
                obj["chat_history"] = new JArray(filtered);
            }

            return obj.ToString(Formatting.None);
        }

        public static QueryResponse ParseResponse(string responseJson)
        {
            return JsonConvert.DeserializeObject<QueryResponse>(responseJson)
                ?? throw new JsonException("Failed to deserialize response");
        }

        public static string ParseErrorMessage(string body)
        {
            try
            {
                var obj = JObject.Parse(body);
                return obj["error"]?.ToString() ?? body;
            }
            catch
            {
                return body;
            }
        }
    }
}
