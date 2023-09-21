using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems
{
    public sealed record SingleItem(string Name, TermValue Effect) : LogicItem(Name), IRemovableItem
    {
        public override void AddTo(ProgressionManager pm)
        {
            pm.Incr(Effect);
        }

        public void RemoveFrom(ProgressionManager pm)
        {
            pm.Incr(Effect.Term, -Effect.Value);
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            yield return Effect.Term;
        }

        public bool Equals(SingleItem other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Effect.Equals(other.Effect));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Effect.GetHashCode());
    }
}
