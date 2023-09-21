using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems.Templates
{
    public record BoolItemTemplate(string Name, string Term) : LogicItemTemplate<BoolItem>(Name)
    {
        public override BoolItem Create(LogicManager lm)
        {
            return new(Name, lm.GetTermStrict(Term));
        }

        public virtual bool Equals(BoolItemTemplate other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Term == other.Term);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Term?.GetHashCode());
    }
}
