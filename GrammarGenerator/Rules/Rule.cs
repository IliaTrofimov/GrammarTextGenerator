using System.Collections;

namespace GrammarGenerator.Rules;

/// <summary>
/// Правило вывода формальной грамматики.
/// </summary>
public sealed class Rule : IEnumerable<Alternative>
{
    /// <summary>Название правила.</summary>
    public string Name { get; }

    /// <summary>Варианты раскрытия правила.</summary>
    public List<Alternative> Alternatives { get; }

    /// <summary>Указывает на то, что правило будет скрыто при построении дерева.</summary>
    public bool Hidden { get; }

    /// <summary>Суммарный вес всех альтернатив.</summary>
    public double TotalWeight { get; private set; }


    /// <summary>Создать пустое правило без заполненных вариантов раскрытия.</summary>
    public Rule(string name, bool hidden = false) : this(name, [], hidden) { }

    /// <summary>Создать правило с заданными вариантами раскрытия.</summary>
    public Rule(string name, List<Alternative> alternatives, bool hidden = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        Name = name;
        Alternatives = alternatives;
        Hidden = hidden;
        TotalWeight = alternatives.Sum(a => a.Weight);
    }


    /// <summary>Добавить новый вариант раскрытия.</summary>
    /// <returns>Ссылка на это правило.</returns>
    public Rule Alt(IEnumerable<Symbol> symbols, double weight = 1.0, string name = "")
    {
        return Add(new Alternative(symbols, weight, name));
    }

    /// <summary>Добавить новый вариант раскрытия.</summary>
    /// <returns>Ссылка на это правило.</returns>
    public Rule Add(Alternative alternative)
    {
        Alternatives.Add(alternative);
        TotalWeight += alternative.Weight;
        return this;
    }

    /// <summary>Человеко-читаемое описание правила в виде БНФ.</summary>
    public string Describe()
    {
        var descriptions = Alternatives.Select(a => a.Describe()).ToList();
        if (descriptions.Sum(d => d.Length) > 128)
        {
            return $"<{Name}> ::= {string.Join("\n\t| ", descriptions)}";
        }
        else
        {
            return $"<{Name}> ::= {string.Join(" | ", descriptions)}";
        }
    }

    public IEnumerator<Alternative> GetEnumerator()
    {
        return ((IEnumerable<Alternative>)Alternatives).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Alternatives).GetEnumerator();
    }
}
