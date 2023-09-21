using RandomizerCore.Logic;
using System.Collections;

namespace RandomizerCore.LogicItems
{
    /// <summary>
    /// Interface for building items which cannot be instantiated until the LogicManager is available.
    /// </summary>
    public interface ILogicItemTemplate
    {
        LogicItem Create(LogicManager lm);
        string Name { get; }
    }

    /// <summary>
    /// Base class for convenience with implementing ILogicItemTemplate.
    /// </summary>
    public abstract record LogicItemTemplate<T>(string Name) : ILogicItemTemplate where T : LogicItem
    {
        public abstract T Create(LogicManager lm);

        LogicItem ILogicItemTemplate.Create(LogicManager lm)
        {
            return Create(lm);
        }

        public virtual bool Equals(LogicItemTemplate<T> other) => ReferenceEquals(this, other) ||
            (other is not null && this.EqualityContract == other.EqualityContract && this.Name == other.Name);

        public override int GetHashCode() => HashCode.Combine(EqualityContract.GetHashCode(), Name?.GetHashCode());
    }
}
