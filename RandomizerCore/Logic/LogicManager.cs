﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RandomizerCore.Json;
using RandomizerCore.StringLogic;
using static RandomizerCore.LogHelper;

namespace RandomizerCore.Logic
{
    public class LMConverter : JsonConverter<LogicManager>
    {
        public override LogicManager ReadJson(JsonReader reader, Type objectType, LogicManager existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            LogicManagerBuilder lmb = new();
            JObject lm = JObject.Load(reader);
            lmb.LP = lm[nameof(LogicManager.LP)].ToObject<LogicProcessor>();
            lmb.VariableResolver = lm[nameof(LogicManager.VariableResolver)].ToObject<VariableResolver>();

            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Terms, lm["Terms"]);
            // TODO: variables
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Waypoints, lm["Waypoints"]);
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Transitions, lm["Transitions"]);
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Locations, lm["Logic"]);
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Items, lm["Items"]);

            return new(lmb);
        }

        public override void WriteJson(JsonWriter writer, LogicManager value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            TermConverter tc = new() { LM = value };
            LogicDefConverter ldc = new() { LM = value };
            serializer.Converters.Add(tc);
            serializer.Converters.Add(ldc);

            writer.WritePropertyName("Terms");
            serializer.Serialize(writer, value.Terms);

            writer.WritePropertyName("Variables");
            serializer.Serialize(writer, value.Variables);

            writer.WritePropertyName("Logic");
            serializer.Serialize(writer, value.LogicLookup.Values.Select(l => new RawLogicDef(l.Name, l.ToInfix())));

            writer.WritePropertyName("Items");
            serializer.Serialize(writer, value.ItemLookup.Values);

            writer.WritePropertyName("Transitions");
            serializer.Serialize(writer, value.TransitionLookup.Values.Select(t => new RawLogicTransition(t.sceneName, t.gateName, t.logic.ToInfix(), t.oneWayType)));

            writer.WritePropertyName("Waypoints");
            serializer.Serialize(writer, value.Waypoints.Select(w => new RawLogicDef(w.Name, w.logic.ToInfix())));

            writer.WritePropertyName(nameof(value.LP));
            serializer.Serialize(writer, value.LP);

            writer.WritePropertyName(nameof(value.VariableResolver));
            serializer.Serialize(writer, value.VariableResolver);

            writer.WriteEndObject();
            serializer.Converters.Remove(ldc);
            serializer.Converters.Remove(tc);
        }
    }

    [JsonConverter(typeof(LMConverter))]
    public class LogicManager : ILogicManager
    {
        public readonly int TermCount;
        public readonly ReadOnlyCollection<Term> Terms;
        public readonly ReadOnlyDictionary<string, Term> TermLookup;
        public readonly ReadOnlyDictionary<string, OptimizedLogicDef> LogicLookup;
        public readonly ReadOnlyDictionary<string, LogicItem> ItemLookup;
        public readonly ReadOnlyDictionary<string, LogicTransition> TransitionLookup;
        public readonly ReadOnlyCollection<LogicWaypoint> Waypoints;
        public readonly ReadOnlyCollection<LogicInt> Variables;

        // Data structures dynamically constructed to correspond to logic
        private readonly Dictionary<string, OptimizedLogicDef> _logicDefs;
        private readonly Dictionary<string, Term> _termLookup;
        private readonly Term[] _terms;
        public LogicProcessor LP { get; }
        private readonly List<LogicInt> _variables;
        private readonly Dictionary<string, int> _variableIndices;
        private readonly Dictionary<string, LogicItem> _items;
        private readonly Dictionary<string, LogicTransition> _transitions;
        public VariableResolver VariableResolver { get; }

        public const int intVariableOffset = -100;

        public LogicManager(LogicManagerBuilder source)
        {
            LP = source.LP;
            VariableResolver = source.VariableResolver;

            LP.LogSelf();

            // Terms
            _terms = source.Terms.ToArray();
            TermCount = _terms.Length;
            _termLookup = new Dictionary<string, Term>(TermCount);
            foreach (Term t in _terms) _termLookup.Add(t.Name, t);
            Terms = new(_terms);
            TermLookup = new(_termLookup);

            // Variables
            VariableResolver = source.VariableResolver ?? new VariableResolver();
            _variables = new();
            _variableIndices = new();
            Variables = new(_variables);

            // Logic
            _logicDefs = new(source.LogicLookup.Count);
            foreach (var kvp in source.LogicLookup)
            {
                _logicDefs.Add(kvp.Key, FromTokens(kvp.Key, kvp.Value));
            }
            LogicLookup = new(_logicDefs);

            // Waypoints
            Waypoints = new(source.Waypoints.Select(kvp => new LogicWaypoint(_termLookup[kvp.Key], _logicDefs[kvp.Key])).ToArray());

            // Transitions
            _transitions = new(source.Transitions.Count);
            foreach (var kvp in source.Transitions)
            {
                _transitions.Add(kvp.Key, new LogicTransition(kvp.Value, _termLookup[kvp.Key], _logicDefs[kvp.Key]));
            }
            TransitionLookup = new(_transitions);

            // Items
            _items = new(source.UnparsedItems.Count + source.PrefabItems.Count);
            JsonSerializer js = JsonUtil.GetLogicSerializer(this);
            foreach (var kvp in source.UnparsedItems)
            {
                _items.Add(kvp.Key, kvp.Value.ToObject<LogicItem>(js));
            }
            foreach (var kvp in source.PrefabItems)
            {
                _items[kvp.Key] = kvp.Value;
            }
            ItemLookup = new(_items);
        }

        public OptimizedLogicDef GetLogicDef(string name)
        {
            if (!_logicDefs.TryGetValue(name, out OptimizedLogicDef def))
            {
                Log($"Unable to find logic for {name}.");
                return null;
            }

            return def;
        }

        public Term GetTerm(string item)
        {
            if (!_termLookup.TryGetValue(item, out Term index))
            {
                Log($"Unable to find index of term {item}.");
                return null;
            }

            return index;
        }

        public Term GetTerm(int id)
        {
            return _terms[id];
        }

        public int EvaluateVariable(object sender, ProgressionManager pm, int id)
        {
            return _variables[intVariableOffset - id].GetValue(sender, pm);
        }

        public LogicInt GetVariable(int id)
        {
            return _variables[intVariableOffset - id];
        }

        public LogicItem GetItem(string name)
        {
            if (!_items.TryGetValue(name, out LogicItem item))
            {
                Log($"Unable to find logic item for {name}.");
                return null;
            }

            return item;
        }

        public LogicTransition GetTransition(string name)
        {
            if (!_transitions.TryGetValue(name, out LogicTransition transition))
            {
                Log($"Unable to find logic transition for {name}.");
                return null;
            }

            return transition;
        }

        public OptimizedLogicDef FromString(RawLogicDef def)
        {
            return Process(def);
        }

        private OptimizedLogicDef Process(RawLogicDef def)
        {
            try
            {
                return FromTokens(def.name, LP.ParseInfixToList(def.logic));
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Error in processing logic for {def.name}.", e);
            }
        }

        public OptimizedLogicDef FromTokens(string name, LogicClause c)
        {
            List<int> logic = new();
            try
            {
                for (int i = 0; i < c.Count; i++)
                {
                    ApplyToken(logic, c[i]);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Error in processing logic for {name}.", e);
            }

            return new(name, logic.ToArray(), this);
        }

        private OptimizedLogicDef FromTokens(string name, IEnumerable<LogicToken> lts)
        {
            List<int> logic = new();
            try
            {
                foreach (LogicToken lt in lts)
                {
                    ApplyToken(logic, lt);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Error in processing logic for {name}.", e);
            }

            return new(name, logic.ToArray(), this);
        }

        private void ApplyToken(List<int> logic, LogicToken lt)
        {
            if (lt is OperatorToken ot)
            {
                logic.Add(ot.OperatorType switch
                {
                    OperatorType.AND => (int)LogicOperators.AND,
                    OperatorType.OR => (int)LogicOperators.OR,
                    _ => throw new NotImplementedException()
                });
            }
            else if (lt is ComparisonToken ct)
            {
                logic.Add(ct.ComparisonType switch
                {
                    ComparisonType.EQ => (int)LogicOperators.EQ,
                    ComparisonType.LT => (int)LogicOperators.LT,
                    ComparisonType.GT => (int)LogicOperators.GT,
                    _ => throw new NotImplementedException(),
                });
                ApplyTermOrVariable(logic, ct.Left);
                ApplyTermOrVariable(logic, ct.Right);
            }
            else if (lt is ConstToken bt)
            {
                logic.Add(bt.Value ? (int)LogicOperators.ANY : (int)LogicOperators.NONE);
            }
            else if (lt is MacroToken mt)
            {
                foreach (var tt in mt.Value) ApplyToken(logic, tt);
            }
            else if (lt is SimpleToken st)
            {
                ApplyTermOrVariable(logic, st.Name);
            }
        }

        private void ApplyTermOrVariable(List<int> logic, string name)
        {
            if (_termLookup.TryGetValue(name, out Term t))
            {
                logic.Add(t.Id);
            }
            else if (_variableIndices.TryGetValue(name, out int i))
            {
                logic.Add(i);
            }
            else if (VariableResolver.TryMatch(this, name, out LogicInt variable))
            {
                int index = intVariableOffset - _variables.Count;
                _variableIndices.Add(name, index);
                logic.Add(index);
                _variables.Add(variable);
            }
            else throw new ArgumentException($"Unknown string {name} found as term.");
        }
    }
}