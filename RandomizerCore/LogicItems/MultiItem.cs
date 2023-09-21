using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems
{
    public sealed record MultiItem(string Name, TermValue[] Effects) : LogicItem(Name), IRemovableItem
    {
        public override void AddTo(ProgressionManager pm)
        {
            for (int i = 0; i < Effects.Length; i++)
            {
                pm.Incr(Effects[i]);
            }
        }
        public void RemoveFrom(ProgressionManager pm)
        {
            for (int i = 0; i < Effects.Length; i++)
            {
                pm.Incr(Effects[i].Term, -Effects[i].Value);
            }
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            return Effects.Select(e => e.Term);
        }

        public bool Equals(MultiItem other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && ReferenceEquals(this.Effects, other.Effects));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Effects?.GetHashCode());
    }
}
