using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems
{
    public sealed record CappedItem(string Name, TermValue[] Effects, TermValue Cap) : LogicItem(Name)
    {
        public override void AddTo(ProgressionManager pm)
        {
            if (!pm.Has(Cap))
            {
                for (int i = 0; i < Effects.Length; i++)
                {
                    pm.Incr(Effects[i]);
                }
            }
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            return Effects.Select(e => e.Term);
        }

        public bool Equals(CappedItem other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && ReferenceEquals(this.Effects, other.Effects) && this.Cap.Equals(other.Cap));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Effects?.GetHashCode(), Cap.GetHashCode());
    }
}
