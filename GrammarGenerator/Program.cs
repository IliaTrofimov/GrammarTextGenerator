using GrammarGenerator.Generation;
using GrammarGenerator.Rules;

namespace GrammarGenerator;

internal class Program
{


    private static void Main(string[] args)
    {
        var phonesGrammar = PhonesGrammar();
        var idsGrammar = AdditionalIdGrammar();

        var phoneGenerator = new TextGenerator(phonesGrammar);
        var idGenerator = new TextGenerator(idsGrammar);

        for (var i = 0; i < 1000; i++)
        {
            var (text, tree) = phoneGenerator.GenerateWithTree("rus_city_phone");
            var nodes = tree.Walk().ToArray();
            Console.WriteLine(text);
        }
    }

    private static Grammar PhonesGrammar()
    {
        var grammar = new Grammar();

        grammar.NewRule("digit", true, Enumerable.Range(0, 10).Select(i => new Alternative(i.ToString())));
        grammar.NewRule("digit_nozero", true, Enumerable.Range(1, 9).Select(i => new Alternative(i.ToString())));

        grammar.Rule(new Rule("phone_7_formatted")
        {
            new Alternative()
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit_2", repeats: 2),
                "-",
                new NonTerminal("digit", repeats: 2),
                "-",
                new NonTerminal("digit", repeats: 2),
            },
            new Alternative()
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit_2", repeats: 2),
                " ",
                new NonTerminal("digit", repeats: 2),
                " ",
                new NonTerminal("digit", repeats: 2),
            },
        });      
        grammar.Rule(new Rule("phone_7")
        {
            new Alternative()
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit", repeats: 6)
            }
        });
        grammar.Rule(new Rule("phone_6")
        {
            new Alternative()
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit", repeats: 5)
            }
        });
        grammar.Rule(new Rule("phone_5")
        {
            new Alternative()
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit", repeats: 4)
            }
        });
        grammar.Rule(new Rule("phone_4")
        {
            new Alternative()
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit", repeats: 3)
            }
        });

        grammar.NewRule("rus_operator_code",
            Dictionaries.RUS_DEF_PREFIXES.Select(s => new Alternative(s))
        );
        grammar.NewRule("rus_city_code",
            Dictionaries.RUS_CITY_PREFIXES.Select(x => new Alternative(x.s, x.w))
        );

        grammar.NewRule("pager",
            new Alternative()
            {
                new NonTerminal("phone_7"),
                grammar.NewRule("pager_for_abonent",
                    new Alternative("для аб."),
                    new Alternative("аб.", 4),
                    new Alternative("аб", 5),
                    new Alternative("для аб. "),
                    new Alternative("аб. ", 4),
                    new Alternative("аб ", 5)
                ),
                grammar.NewRule("pager_abonent_suffix",
                    new Alternative(new NonTerminal("digit", 4)),
                    new Alternative(new NonTerminal("digit", 3)),
                    new Alternative(new NonTerminal("digit", 2))
                ),
            }
        );

        return grammar;
    }

    public static Grammar AdditionalIdGrammar()
    {
        var grammar = new Grammar();
        grammar.NewRule("digit", true, Enumerable.Range(0, 10).Select(i => new Alternative(i.ToString())));

        grammar.NewRule("IMEI",
            new Alternative()
            {
                grammar.NewRule("IMEI_prefix",
                    new Alternative("IMEI: ", 0.01),
                    new Alternative("IMEI ", 0.09),
                    new Alternative("", 0.9)
                ),
                grammar.NewRule("TAC",
                    Dictionaries.TAC_Examples.Select(s => new Alternative(s)) // 8 symbols
                ),
                grammar.NewRule("IMEI_number", 
                    new Alternative(new NonTerminal("digit", repeats: 7)),
                    new Alternative(new NonTerminal("digit", repeats: 8))
                )
            }
        );

        grammar.NewRule("IMSI",
            new Alternative(3.0, "_imsi_rus")
            {
                "250",
                grammar.NewRule("MNC_rus",
                    new Alternative("01", 3.0),
                    new Alternative("02", 3.0),
                    new Alternative("99", 2.5),
                    new Alternative("20", 2.0),
                    new Alternative("11", 1.5),
                    new Alternative("39", 1.0)
                ),
                new NonTerminal("digit", repeats: 10)
            },
            new Alternative(1, "_imsi_world")
            {
                grammar.NewRule("MCC", 
                    Dictionaries.MCC_Examples.Select(x => new Alternative(x.s, x.w))
                ),
                new NonTerminal("digit", repeats : 12)
            }
        );

        grammar.NewRule("ICCID",
            new Alternative()
            {
                "89",
                grammar.NewRule("ICCID_country",
                    new Alternative("7", 3.0),
                    new Alternative("70", 0.2),
                    new Alternative("44", 0.6),
                    new Alternative("380", 1),
                    new Alternative("370", 0.1)
                ),
                new NonTerminal("digit", repeats:  13)
            }
        );

        grammar.NewRule("numeric_ID",
            new Alternative()
            {
                 grammar.NewRule("ID_prefix",
                    new Alternative("ID"),
                    new Alternative("")
                ),
                grammar.NewRule("long_number",
                    new Alternative(new NonTerminal("digit", repeats: 12), 0.7),
                    new Alternative(new NonTerminal("digit", repeats: 11), 0.9),
                    new Alternative(new NonTerminal("digit", repeats: 10), 1.2),
                    new Alternative(new NonTerminal("digit", repeats: 9), 1.8),
                    new Alternative(new NonTerminal("digit", repeats: 8), 1.4),
                    new Alternative(new NonTerminal("digit", repeats: 7), 1.0),
                    new Alternative(new NonTerminal("digit", repeats: 6), 0.7),
                    new Alternative(new NonTerminal("digit", repeats: 5), 0.5),
                    new Alternative(new NonTerminal("digit", repeats: 4), 0.1)
                )
            }
        );

        grammar.NewRule("ID",
            new Alternative(new NonTerminal("IMEI"), 3),
            new Alternative(new NonTerminal("IMSI"), 1),
            new Alternative(new NonTerminal("ICCID"), 1),
            new Alternative(new NonTerminal("numeric_ID"), 0.7)
        );

        return grammar;
    }
}
