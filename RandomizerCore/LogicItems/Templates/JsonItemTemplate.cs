using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems.Templates
{
    /// <summary>
    /// Item template wrapper for a json representation of an item.
    /// </summary>
    public class JsonItemTemplate : ILogicItemTemplate
    {
        public JsonItemTemplate(string json) : this(JToken.Parse(json)) { }

        public JsonItemTemplate(JToken t)
        {
            Name = t.Value<string>("Name");
            JToken = t;
        }

        public string Name { get; }
        public JToken JToken { get; }

        public LogicItem Create(LogicManager lm)
        {
            JsonSerializer js = JsonUtil.GetLogicSerializer(lm);
            return JToken.ToObject<LogicItem>(js);
        }

        public virtual bool Equals(JsonItemTemplate other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Name == other.Name && ReferenceEquals(this.JToken, other.JToken));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name?.GetHashCode(), JToken?.GetHashCode());
    }
}
