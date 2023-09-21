using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems.Templates
{
    public record SingleItemTemplate(string Name, (string Term, int Value) Effect) : LogicItemTemplate<SingleItem>(Name)
    {
        public override SingleItem Create(LogicManager lm)
        {
            return new(Name, new(lm.GetTermStrict(Effect.Term), Effect.Value));
        }

        public virtual bool Equals(SingleItemTemplate other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Effect.Equals(other.Effect));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Effect.GetHashCode());
    }
}
