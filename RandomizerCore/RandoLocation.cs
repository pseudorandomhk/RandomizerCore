﻿using RandomizerCore.Logic;
using Newtonsoft.Json;

namespace RandomizerCore
{
    public class RandoLocation : IRandoLocation
    {
        public LogicDef logic;
        public List<LogicCost>? costs;

        [JsonIgnore]
        public string Name => logic.Name;

        public float Priority { get ; set; }
        public TempState Reachable { get; set; }
        public int Sphere { get; set; }

        public bool CanGet(ProgressionManager pm)
        {
            if (costs != null)
            {
                if (costs.Any(l => !l.CanGet(pm))) return false;
            }
            return logic.CanGet(pm);
        }

        public IEnumerable<Term> GetTerms()
        {
            var query = logic.GetTerms();
            if (costs != null)
            {
                query = query.Concat(costs.SelectMany(c => c.GetTerms()));
            }
            return query;
        }

        public void AddCost(LogicCost cost)
        {
            if (costs == null) costs = new List<LogicCost>();
            costs.Add(cost);
        }

        public RandoLocation Clone()
        {
            RandoLocation rl = (RandoLocation)MemberwiseClone();
            rl.costs = rl.costs?.ToList();
            return rl;
        }

        public IEnumerable<RandoLocation> CloneMany(int count)
        {
            for (int i = 0; i < count; i++) yield return Clone();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
