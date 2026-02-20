using Newtonsoft.Json;

namespace BuildScope
{
    public class ProjectContext
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("building_class")]
        public string BuildingClass { get; set; } = "";

        [JsonProperty("state")]
        public string State { get; set; } = "";

        [JsonProperty("construction_type")]
        public string ConstructionType { get; set; } = "";

        public override string ToString() => $"{Name} | {State} | Class {BuildingClass} | {ConstructionType}";
    }
}
