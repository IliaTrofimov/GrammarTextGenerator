using GrammarGenerator.Rules;
using System.Text;

namespace GrammarGenerator.Generation;

public sealed class Stats
{
    public Dictionary<(string Rule, int Index), int> VariantCounts { get; }
        = new();

    public static Stats FromTree(Node root)
    {
        var s = new Stats();
        foreach (var n in root.Walk())
        {
            if (n.Symbol is NonTerminal nt && n.VariantIndex.HasValue)
            {
                var key = (nt.Name, n.VariantIndex.Value);
                s.VariantCounts[key] = s.VariantCounts.GetValueOrDefault(key) + 1;
            }
        }
        return s;
    }

    public void Merge(Stats other)
    {
        foreach (var (k, v) in other.VariantCounts)
            VariantCounts[k] = VariantCounts.GetValueOrDefault(k) + v;
    }

    public string Report(Grammar grammar, int? top = null)
    {
        var total = VariantCounts.Values.Sum();
        if (total == 0) total = 1;
        var items = VariantCounts.OrderByDescending(kv => kv.Value);
        if (top.HasValue) items = (IOrderedEnumerable<KeyValuePair<(string Rule, int Index), int>>)items.Take(top.Value);

        var sb = new StringBuilder();
        foreach (var ((ruleName, idx), cnt) in items)
        {
            var rule = grammar.Rules[ruleName];
            var alt = rule.Alternatives[idx];
            var label = alt.Name ?? string.Join(" ",
                alt.Symbols.Select(s => s.Describe()));
            sb.AppendLine(
                $"<{ruleName}>[{idx}] {label,-40} {cnt,6}  p≈{(double)cnt / total:F3}");
        }
        return sb.ToString();
    }
}