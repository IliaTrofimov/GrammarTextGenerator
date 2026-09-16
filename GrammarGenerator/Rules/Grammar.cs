using System.Text;

namespace GrammarGenerator.Rules;


/// <summary>
/// Формальная грамматика для генерации сулчайного текста по правилам.
/// </summary>
public sealed class Grammar
{
    private readonly Dictionary<string, Rule> _rules = [];
    private int _fresh;

    /// <summary>Правила вывода.</summary>
    public IReadOnlyDictionary<string, Rule> Rules => _rules;


    /// <summary>Добавить новое анонимное правило вывода с заданными вариантами раскрытия.</summary>
    public NonTerminal NewRule(IEnumerable<Alternative> alternatives)
    {
        return NewRule(GetName("rule"), true, alternatives);
    }

    /// <summary>Добавить новое анонимное правило вывода с заданными вариантами раскрытия.</summary>
    public NonTerminal NewRule(params Alternative[] alternatives)
    {
        return NewRule(GetName("rule"), true, alternatives);
    }

    /// <summary>Добавить новое правило вывода с заданными вариантами раскрытия.</summary>
    public NonTerminal NewRule(string name, IEnumerable<Alternative> alternatives)
    {
        return NewRule(name, false, alternatives);
    }

    /// <summary>Добавить новое правило вывода с заданными вариантами раскрытия.</summary>
    public NonTerminal NewRule(string name, bool hidden, IEnumerable<Alternative> alternatives)
    {
        return Rule(new Rule(name, alternatives.ToList(), hidden));
    }

    /// <summary>Добавить новое правило вывода с заданными вариантами раскрытия.</summary>
    public NonTerminal NewRule(string name, params Alternative[] alternatives)
    {
        return NewRule(name, false, alternatives);
    }

    /// <summary>Добавить новое правило вывода с заданными вариантами раскрытия.</summary>
    public NonTerminal NewRule(string name, bool hidden, params Alternative[] alternatives)
    {
        return NewRule(name, hidden, (IEnumerable<Alternative>)alternatives);
    }

    /// <summary>Добавить новое правило, которое выводит заданный символ с некотрым шансом.</summary>
    public NonTerminal NewOptional(Symbol symbol, double wPresent = 1, double wAbscent = 1)
    {
        return NewRule(GetName("optional"), true, new Alternative(symbol, wPresent), new Alternative("", wAbscent));
    }

    /// <summary>Добавить правило вывода.</summary>
    public NonTerminal Rule(Rule rule)
    {
        if (_rules.ContainsKey(rule.Name))
            throw new ArgumentException($"Rule '{rule.Name}' has been already added");
        _rules[rule.Name] = rule;
        return new NonTerminal(rule.Name);
    }

    /// <summary>Человеко-читаемое представление грамматики в виде БНФ.</summary>
    public string Describe(bool showHidden = false)
    {
        var parts = _rules.Values
            .Where(r => showHidden || !r.Hidden)
            .Select(r => r.Describe());
        return string.Join("\n\n", parts);
    }

    /// <summary>Проверить корректность созданной грамматики.</summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Validate()
    {
        var failed = new List<(string rule, string nonTerm)>();

        foreach (var rule in _rules.Values)
        {
            foreach (var alt in rule.Alternatives)
            {
                foreach (var s in alt.Symbols)
                {
                    if (s is NonTerminal nt && !_rules.ContainsKey(nt.Name))
                    {
                        failed.Add((rule.Name, nt.Name));
                    }
                }
            }
        }

        if (failed.Count > 0)
        {
            var sb = new StringBuilder();
            foreach (var group in failed.GroupBy(x => x.rule, x => x.nonTerm))
                sb.Append($"{group.Key} ({string.Join(", ", group)})");
    
            throw new InvalidOperationException($"Invalid NonTerminal symbols in rules {sb}");
        }
    }

    private string GetName(string hint)
    {
        return $"_{hint}_{++_fresh}";
    }
}
