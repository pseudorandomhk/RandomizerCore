﻿using RandomizerCore.Logic;

namespace RandomizerCore.LogicItems
{
    public sealed record BranchedItem(string Name, LogicDef Logic, LogicItem TrueItem, LogicItem FalseItem) : LogicItem(Name)
    {
        public override void AddTo(ProgressionManager pm)
        {
            if (Logic.CanGet(pm))
            {
                TrueItem?.AddTo(pm);
            }
            else
            {
                FalseItem?.AddTo(pm);
            }
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            return (TrueItem?.GetAffectedTerms() ?? Enumerable.Empty<Term>())
                .Concat(FalseItem?.GetAffectedTerms() ?? Enumerable.Empty<Term>());
        }
    }
}
