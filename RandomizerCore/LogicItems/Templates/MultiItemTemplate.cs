using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems.Templates
{
    public record MultiItemTemplate(string Name, (string Term, int Value)[] Effects) : LogicItemTemplate<MultiItem>(Name)
    {
        public override MultiItem Create(LogicManager lm)
        {
            return new(Name, Effects.Select(p => new TermValue(lm.GetTermStrict(p.Term), p.Value)).ToArray());
        }

        public virtual bool Equals(MultiItemTemplate other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && ReferenceEquals(this.Effects, other.Effects));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Effects?.GetHashCode());
    }
}
