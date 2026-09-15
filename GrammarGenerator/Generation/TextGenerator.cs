using GrammarGenerator.Rules;
using System.Diagnostics.CodeAnalysis;

namespace GrammarGenerator.Generation;

/// <summary>
/// Генератор текста из фрмальной грамматики <see cref="Grammar"/>.
/// </summary>
public sealed class TextGenerator(Grammar grammar, int? seed = null, int maxDepth = 40)
{
    private readonly Grammar _g = grammar;
    private readonly Random _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    private readonly int _maxDepth = maxDepth;

    /// <summary>Сгенерировать новый текст для указанного стартового правила.</summary>
    public string Generate(string start) => Expand(new NonTerminal(start), 0).Text;

    /// <summary>Сгенерировать новый текст для указанного стартового правила.</summary>
    /// <returns>Текст и корневой узел дерева вывода.</returns>
    public (string Text, Node Tree) GenerateWithTree(string start)
    {
        var node = Expand(new NonTerminal(start), 0);
        return (node.Text, node);
    }

    private Node Expand(Symbol symbol, int depth)
    {
        if (depth > _maxDepth)
            throw new InvalidOperationException($"Превышена глубина на {symbol.Describe()}");

        var singleRepeat = WithSingleRepeat(symbol);
        var node = new Node(symbol);
        for (var i = 0; i < symbol.Repeats; i++)
            node.Children.Add(ExpandSingle(singleRepeat, depth));

        return node;
    }

    private static Symbol WithSingleRepeat(Symbol symbol) => symbol switch
    {
        Terminal t => new Terminal(t.Text, 1),
        NonTerminal nt => new NonTerminal(nt.Name, 1),
        _ => throw new NotSupportedException()
    };

    private Node ExpandSingle(Symbol symbol, int depth)
    {
        if (symbol is Terminal t)
            return new Node(t);

        var nt = (NonTerminal)symbol;
        if (SelectAlternative(_g.Rules[nt.Name], out var idx, out var alt))
        {
            var node = new Node(nt, idx);
            foreach (var s in alt.Symbols)
                node.Children.Add(Expand(s, depth + 1));
            return node;
        }

        return new Node(nt);
    }

    private bool SelectAlternative(Rule rule, out int index, [NotNullWhen(true)] out Alternative? alternative)
    {
        if (rule.Alternatives.Count == 1)
        {
            index = 0;
            alternative = rule.Alternatives[0];
            return true;
        }

        var r = _rng.NextDouble() * rule.TotalWeight;
        var acc = 0.0;
        for (var i = 0; i < rule.Alternatives.Count; i++)
        {
            acc += rule.Alternatives[i].Weight;
            if (r < acc)
            {
                index = i;
                alternative = rule.Alternatives[i];
                return true;
            }
        }

        index = -1;
        alternative = null;
        return false;
    }
}
