using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems.Templates
{
    public record CappedItemTemplate(string Name, (string Term, int Value)[] Effects, (string Term, int Value) Cap) : LogicItemTemplate<CappedItem>(Name)
    {
        public override CappedItem Create(LogicManager lm)
        {
            return new(Name, Effects.Select(p => new TermValue(lm.GetTermStrict(p.Term), p.Value)).ToArray(), new(lm.GetTermStrict(Cap.Term), Cap.Value));
        }

        public virtual bool Equals(CappedItemTemplate other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && ReferenceEquals(this.Effects, other.Effects) && this.Cap.Equals(other.Cap));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Effects?.GetHashCode(), Cap.GetHashCode());
    }
}
