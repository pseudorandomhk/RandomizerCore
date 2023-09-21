using RandomizerCore.Logic;

namespace RandomizerCore.LogicItems
{
    public sealed record EmptyItem(string Name) : LogicItem(Name)
    {
        public override void AddTo(ProgressionManager pm)
        {
            return;
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            return Enumerable.Empty<Term>();
        }

        public bool Equals(EmptyItem other) => base.Equals(other);

        public override int GetHashCode() => base.GetHashCode();
    }
}
