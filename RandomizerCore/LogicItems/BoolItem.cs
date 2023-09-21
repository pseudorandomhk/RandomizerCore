using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems
{
    public sealed record BoolItem(string Name, Term Term) : LogicItem(Name)
    {
        public override void AddTo(ProgressionManager pm)
        {
            pm.Set(Term, 1);
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            yield return Term;
        }

        public bool Equals(BoolItem other) => ReferenceEquals(this, other) || (base.Equals(other) && this.Term == other.Term);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Term?.GetHashCode());
    }
}
