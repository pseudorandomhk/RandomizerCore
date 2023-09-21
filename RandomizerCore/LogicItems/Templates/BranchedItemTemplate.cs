using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems.Templates
{
    public record BranchedItemTemplate(string Name, string Logic, ILogicItemTemplate TrueItem, ILogicItemTemplate FalseItem) : LogicItemTemplate<BranchedItem>(Name)
    {
        public override BranchedItem Create(LogicManager lm)
        {
            return new(Name, lm.FromString(new(Name, Logic)), TrueItem.Create(lm), FalseItem.Create(lm));
        }

        public virtual bool Equals(BranchedItemTemplate other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Logic == other.Logic && ReferenceEquals(this.TrueItem, other.TrueItem) && ReferenceEquals(this.FalseItem, other.FalseItem));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Logic?.GetHashCode(),
            TrueItem?.GetHashCode(), FalseItem?.GetHashCode());
    }
}
