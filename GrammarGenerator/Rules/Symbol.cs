namespace GrammarGenerator.Rules;

/// <summary>Символ формальной грамматики.</summary>
public abstract class Symbol
{
    /// <summary>Количество повторений этого символа. </summary>
    public int Repeats { get; }

    public Symbol(int repeats = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(repeats);
        Repeats = repeats;
    }

    public abstract string Describe();
}


/// <summary>Терминальный символ грамматики. Представляет конкретную строку.</summary>
public sealed class Terminal(string text, int repeats = 1) : Symbol(repeats)
{
    /// <summary>Значение символа.</summary>
    public string Text { get; } = text;

    public override string Describe()
    {
        if (Repeats > 1) return $"\"{Text}\"{{{Repeats}}}";
        return $"\"{Text}\"";
    }

    public override string ToString() => $"Term({Text})";
}


/// <summary>Нетерминальный символ грамматики. Представляет ссылку на другое правило.</summary>
public sealed class NonTerminal(string name, int repeats = 1) : Symbol(repeats)
{
    public NonTerminal(NonTerminal other, int repeats = 1) : this(other.Name, repeats) {}

    public NonTerminal(Rule other, int repeats = 1) : this(other.Name, repeats) { }


    /// <summary>Название правила, на которое ссылается символ.</summary>
    public string Name { get; } = name;

    public override string Describe()
    {
        if (Repeats > 1) return $"<{Name}>{{{Repeats}}}";
        return $"<{Name}>";
    }

    public override string ToString() => $"NonTerm({Name})";
}
