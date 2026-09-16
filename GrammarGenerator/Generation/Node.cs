using GrammarGenerator.Rules;
using System.Text;

namespace GrammarGenerator.Generation;

/// <summary>Узел в дереве вывода.</summary>
public sealed class Node
{
    /// <summary>Символ, который был получен на этом этапе вывода.</summary>
    public Symbol Symbol { get; }

    /// <summary>Номер выбранной альтернативы <see cref="Alternative"/>.</summary>
    public int? VariantIndex { get; }

    /// <summary>Дочерние узлы.</summary>
    public List<Node> Children { get; } = new();

    public Node(Symbol symbol, int? variantIndex = null)
    {
        Symbol = symbol;
        VariantIndex = variantIndex;
    }

    public string Text => Symbol is Terminal t
        ? t.Text
        : string.Concat(Children.Select(c => c.Text));

    /// <summary>Обход дерева.</summary>
    public IEnumerable<Node> Walk()
    {
        yield return this;
        foreach (var c in Children)
            foreach (var n in c.Walk())
                yield return n;
    }

    public string Render(int indent = 0,
                         bool hideHidden = true,
                         Grammar? grammar = null)
    {
        if (hideHidden && grammar != null
            && Symbol is NonTerminal nt
            && grammar.Rules.TryGetValue(nt.Name, out var r)
            && r.Hidden)
        {
            return string.Join("\n",
                Children.Select(c => c.Render(indent, hideHidden, grammar)));
        }

        var pad = new string(' ', indent * 2);
        var label = Symbol.Describe();
        if (VariantIndex.HasValue)
            label += $"  [alt. {VariantIndex.Value}]";

        var sb = new StringBuilder();
        sb.Append(pad).Append(label);
        foreach (var c in Children)
            sb.Append('\n').Append(c.Render(indent + 1, hideHidden, grammar));
        return sb.ToString();
    }
}
