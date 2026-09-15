using System.Collections;

namespace GrammarGenerator.Rules;

/// <summary> Один из вариантов раскрытия правила грамматики.</summary>
public sealed class Alternative : IEnumerable<Symbol>
{
    /// <summary>Последовательность символов, которые будут применены при выборе этого варианта.</summary>
    public List<Symbol> Symbols { get; }

    /// <summary> Вес данного варианта. Чем выше вес, тем чаще будет выбираться конкретная альтернатива.</summary>
    public double Weight { get; }

    /// <summary>Название.</summary>
    public string Name { get; }


    /// <summary>Создать пустой вариант раскрытия.</summary>
    public Alternative(double weight = 1.0, string name = "")
    {
        Symbols = [];
        Weight = weight;
        Name = name;
    }

    /// <summary>Создать вариант раскрытия с заданной последовательностью символов.</summary>
    public Alternative(IEnumerable<Symbol> symbols, double weight = 1.0, string name = "")
    {
        Symbols = symbols.ToList();
        Weight = weight;
        Name = name;
    }

    /// <summary>Создать вариант раскрытия с одним символом.</summary>
    public Alternative(Symbol symbol, double weight = 1.0, string name = "")
    {
        Symbols = [symbol];
        Weight = weight;
        Name = name;
    }

    /// <summary>Создать вариант раскрытия с одним терминальным символом.</summary>
    public Alternative(string terminalSymbol, double weight = 1.0, string name = "")
        : this(new Terminal(terminalSymbol), weight, name)
    { }

    /// <summary>Добавить символ.</summary>
    public Alternative Add(Symbol symbol)
    {
        Symbols.Add(symbol);
        return this;
    }

    /// <summary>Добавит терминальный символ.</summary>
    public Alternative Add(string terminalSymbol)
    {
        Symbols.Add(new Terminal(terminalSymbol));
        return this;
    }

    /// <summary>Человеко-читаемое описание.</summary>
    public string Describe()
    {
        var body = Symbols.Count == 0
            ? "\"\""
            : string.Join(" ", Symbols.Select(s => s.Describe()));

        var hasWeight = Math.Abs(Weight - 1) > 1e-10;
        var hasName = !string.IsNullOrWhiteSpace(Name);
        
        if (!hasName && !hasWeight)
        {
            return body;
        }
        else if (hasName && hasWeight)
        {
            return $"{body} /w:{Weight} n:{Name}*/";
        }
        else if (hasWeight)
        {
            return $"{body} /*w:{Weight}*/";
        }
        else
        {
            return $"{body} /*n:{Name}*/";
        }
    }


    public IEnumerator<Symbol> GetEnumerator()
    {
        return ((IEnumerable<Symbol>)Symbols).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Symbols).GetEnumerator();
    }
}
