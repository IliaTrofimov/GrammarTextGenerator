using System.Collections;

namespace GrammarGenerator.Rules;


/// <summary>
/// Формальная грамматика для генерации сулчайного текста по правилам.
/// </summary>
public sealed class Grammar
{
    private readonly Dictionary<string, Rule> _rules = [];
    private readonly Dictionary<string, string> _cache = [];
    private int _fresh;

    /// <summary>Правила вывода.</summary>
    public IReadOnlyDictionary<string, Rule> Rules => _rules;

    
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

    /// <summary>Добавить правило вывода.</summary>
    public NonTerminal Rule(Rule rule)
    {
        if (_rules.ContainsKey(rule.Name))
            throw new ArgumentException($"Правило {rule.Name} уже существует");
        _rules[rule.Name] = rule;
        return new NonTerminal(rule.Name);
    }


    private string Gensym(string hint) => $"_{hint}_{++_fresh}";


    public string Describe(bool showHidden = false)
    {
        var parts = _rules.Values
            .Where(r => showHidden || !r.Hidden)
            .Select(r => r.Describe());
        return string.Join("\n\n", parts);
    }
}
