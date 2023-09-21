using System.Collections;

namespace RandomizerCore.StringLogic
{
    /// <summary>
    /// The fundamental unit of tokenized logic, used throughout the StringLogic namespace.
    /// </summary>
    public abstract record LogicToken
    {
        public virtual bool Equals(LogicToken other) => ReferenceEquals(this, other) ||
            (other is not null && this.EqualityContract == other.EqualityContract);

        public override int GetHashCode() => EqualityContract.GetHashCode();
    }
    
    /// <summary>
    /// LogicToken representing one of the binary boolean operators, | or +.
    /// </summary>
    public record OperatorToken(OperatorType OperatorType, int Precedence, string Symbol) : LogicToken
    {
        public static readonly OperatorToken AND = new(OperatorType.AND, 1, "+");
        public static readonly OperatorToken OR = new(OperatorType.OR, 0, "|");

        public virtual bool Equals(OperatorToken other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.OperatorType == other.OperatorType && this.Precedence == other.Precedence && this.Symbol == other.Symbol);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), OperatorType.GetHashCode(),
            Precedence.GetHashCode(), Symbol.GetHashCode());
    }
    public enum OperatorType
    {
        OR,
        AND,
    }

    /// <summary>
    /// LogicToken which evaluates to a bool.
    /// </summary>
    public abstract record TermToken : LogicToken
    {
        public abstract string Write();

        public virtual bool Equals(TermToken other) => ReferenceEquals(this, other) || base.Equals(other);

        public override int GetHashCode() => base.GetHashCode();

        public static LogicClause operator|(TermToken t, TermToken u)
        {
            return new(t, u, OperatorToken.OR);
        }
        public static LogicClause operator +(TermToken t, TermToken u)
        {
            return new(t, u, OperatorToken.AND);
        }
    }

    /// <summary>
    /// TermToken which represents a simple named variable.
    /// </summary>
    public record SimpleToken(string Name) : TermToken
    {
        public override string Write() => Name;

        public virtual bool Equals(SimpleToken other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Name == other.Name);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name?.GetHashCode());
    }
    
    /// <summary>
    /// TermToken which represents a simple comparison of two named integer variables.
    /// </summary>
    public record ComparisonToken(ComparisonType ComparisonType, string Left, string Right) : TermToken
    {
        public override string Write()
        {
            char symbol = ComparisonType switch
            {
                ComparisonType.EQ => '=',
                ComparisonType.LT => '<',
                ComparisonType.GT => '>',
                _ => throw new NotImplementedException(),
            };
            return $"{Left}{symbol}{Right}";
        }

        public virtual bool Equals(ComparisonToken other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.ComparisonType == other.ComparisonType && this.Left == other.Left && this.Right == other.Right);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ComparisonType.GetHashCode(),
            Left.GetHashCode(), Right.GetHashCode());
    }
    public enum ComparisonType
    {
        EQ,
        LT,
        GT,
    }

    /// <summary>
    /// TermToken which represents a nested LogicClause, provided through the IMacroSource, usually a LogicProcessor.
    /// </summary>
    public record MacroToken(string Name, IMacroSource Source) : TermToken
    {
        public override string Write() => Name;
        public LogicClause Value => Source.GetMacro(Name);

        public virtual bool Equals(MacroToken other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Name == other.Name && ReferenceEquals(this.Source, other.Source));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name?.GetHashCode(), Source?.GetHashCode());
    }

    /// <summary>
    /// TermToken which represents a nested LogicClause by name.
    /// </summary>
    public record ReferenceToken(string Target) : TermToken
    {
        public override string Write() => $"*{Target}";

        public virtual bool Equals(ReferenceToken other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Target == other.Target);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Target?.GetHashCode());
    }

    /// <summary>
    /// TermToken which is parsed as its left argument if defined, otherwise as its right argument.
    /// </summary>
    public record CoalescingToken(TermToken Left, TermToken Right) : TermToken
    {
        public override string Write() => $"{Left.Write()}?{Right.Write()}";

        public virtual bool Equals(CoalescingToken other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && ReferenceEquals(this.Left, other.Left) && ReferenceEquals(this.Right, other.Right));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Left?.GetHashCode(), Right?.GetHashCode());
    }

    /// <summary>
    /// TermToken which represents a constant bool.
    /// </summary>
    public record ConstToken(bool Value) : TermToken
    {
        public override string Write() => Value.ToString().ToUpper();
        public static readonly ConstToken True = new(true);
        public static readonly ConstToken False = new(false);

        public virtual bool Equals(ConstToken other) => ReferenceEquals(this, other) ||
            (base.Equals(other) && this.Value == other.Value);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Value.GetHashCode());
    }
}
