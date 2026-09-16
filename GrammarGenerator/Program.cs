using GrammarGenerator.Generation;
using GrammarGenerator.Rules;

namespace GrammarGenerator;

internal class Program
{
    private static readonly IReadOnlyCollection<string> RUS_DEF_PREFIXES =
    [
      "900", "903", "905", "910", "911", "912",
        "919", "920", "921", "922", "923", "925"
    ];

    private static readonly IReadOnlyCollection<string> RUS_CITY_PREFIXES =
    [
        "495", "498", "862", "812", "343",  
        "863", "347", "855", "345", "351",     
        "872", "848", "831", "879", "391"
    ];


    private static void Main()
    {
        var grammar = new Grammar();
        #region Грамматика 

        grammar.NewRule("digit", true, Enumerable.Range(0, 10).Select(i => new Alternative(i.ToString())));
        grammar.NewRule("digit_nozero", true, Enumerable.Range(1, 9).Select(i => new Alternative(i.ToString())));
        grammar.NewRule("digit_2-9", true, Enumerable.Range(2, 8).Select(i => new Alternative(i.ToString())));

        grammar.Rule(new Rule("phone_7_formatted")
        {
            new Alternative(name: "hyphens separators")
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit", repeats: 2),
                "-",
                new NonTerminal("digit", repeats: 2),
                "-",
                new NonTerminal("digit", repeats: 2),
            },
            new Alternative(name: "spaces separators")
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit", repeats: 2),
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
        grammar.Rule(new Rule("phone_3")
        {
            new Alternative()
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit", repeats: 2)
            }
        });

        // У каждого города 1-2 кода. Код всегда стоит в +7(ABC)xxx-xx-xx вместо ABC
        grammar.NewRule("rus_city_code", RUS_CITY_PREFIXES.Select(x => new Alternative(x)));
        grammar.NewRule("rus_city_code_fmt",
            new Alternative(weight: 2, name: "brackets")
            {
                "(", new NonTerminal("rus_city_code"), ")"
            },
            new Alternative(name: "brackets + spaces")
            {
                " (", new NonTerminal("rus_city_code"), ") "
            }
        );

        // Длинный городской номер с кодом города
        grammar.Rule(new Rule("rus_city_phone7_trimmed")
        {
            new Alternative(name: "(ABC)xxx-xx-xx")
            {
                new NonTerminal("rus_city_code"),
                new NonTerminal("phone_7")
            },
        });

        // Городские номера в междугороднем формате (с кодом города), без кода страны и плюса
        grammar.Rule(new Rule("rus_home_phone_trimmed")
        {
            // (10 симв.) без кода страны, код города, 7-значный номер (Москва и тп)
            new Alternative(16000, name: "(ABC)xxx-xx-xx")
            {
                new NonTerminal("rus_city_phone7_trimmed"),
            },
            // (9 симв.) без кода страны, код города, 6-значный номер (Самара)
            new Alternative(3100, name: "(ABC)xx-xx-xx")
            {
                new NonTerminal("rus_city_code"),
                new NonTerminal("phone_6")
            },
            // (8 симв.) без кода страны, код города, 5-значный номер (маленький город)
            new Alternative(1050, name: "(ABC)x-xx-xx")
            {
                new NonTerminal("rus_city_code"),
                new NonTerminal("phone_5")
            },
            // (7 симв.) без кода страны, код города, 4-значный номер (очень маленький город/вброс)
            new Alternative(50, name: "(ABC)xx-xx")
            {
                new NonTerminal("rus_city_code"),
                new NonTerminal("phone_4")
            }
        });
        grammar.Rule(new Rule("rus_work_phone_trimmed")
        {
            // (10 симв.) без кода страны, код города, 7-значный номер (Москва и тп)
            new Alternative(76000, name: "(ABC)xxx-xx-xx")
            {
                new NonTerminal("rus_city_phone7_trimmed"),
            },
            // (9 симв.) без кода страны, код города, 6-значный номер (Самара)
            new Alternative(5100, name: "(ABC)xx-xx-xx")
            {
                new NonTerminal("rus_city_code"),
                new NonTerminal("phone_6")
            },
            // (8 симв.) без кода страны, код города, 5-значный номер (маленький город)
            new Alternative(2500, name: "(ABC)x-xx-xx")
            {
                new NonTerminal("rus_city_code"),
                new NonTerminal("phone_5")
            },
        });

        // Городские номера РФ
        grammar.Rule(new Rule("rus_home_phone")
        {
            // (12 симв.) +, код страны, код города, 7-значный номер
            new Alternative(4017, name: "+7(ABC)xxx-xx-xx")
            {
                new Terminal("+7"),
                new NonTerminal("rus_home_phone_trimmed"),
            },
            // (12 симв.) +, код страны, код города, 7-значный номер (форматирование)
            new Alternative(100, name: "+7 (ABC) xxx-xx-xx formatted rich")
            {
                new Terminal("+7"),
                new NonTerminal("rus_city_code_fmt"),
                new NonTerminal("phone_7")
            },
            // (11 симв.) код страны (7/8), код города, 7-значный номер,
            new Alternative(13000, name: "7/8(ABC)xxx-xx-xx")
            {
                grammar.NewRule(
                    new Alternative("7", weight: 8),
                    new Alternative("8", weight: 1)
                ),
                new NonTerminal("rus_city_phone7_trimmed"),
            },
            // (7-10 симв.) без кода страны, см. выше
            new Alternative(new NonTerminal("rus_home_phone_trimmed"), weight: 20000),
            // (7 симв.) внутренний городской номер (большой город - Москва)
            new Alternative(new NonTerminal("phone_7"), weight: 43000, name: "xxx-xx-xx"),
            // (6 симв.) внутренний городской номер (среднний город - Оренбург)
            new Alternative(new NonTerminal("phone_6"), weight: 7900, name: "xx-xx-xx"),
            // (5 симв.) внутренний городской номер (маленький город)
            new Alternative(new NonTerminal("phone_5"), weight: 2100, name: "x-xx-xx"),
            // (4 симв.) внутренний городской номер (маленький город-село)
            new Alternative(new NonTerminal("phone_4"), weight: 210),
            // (3 симв.) село или вброс
            new Alternative(new NonTerminal("phone_3"), weight: 20)
        });
       
        // У каждого оператора свой код. Код всегда стоит в +7(DEF)xxx-xx-xx вместо DEF и всегда начинается с 9
        grammar.NewRule("rus_oper_code", RUS_DEF_PREFIXES.Select(x => new Alternative(x)));
        grammar.NewRule("rus_oper_code_fmt",
            new Alternative(weight: 2, name: "brackets")
            {
                "(", new NonTerminal("rus_oper_code"), ")"
            },
            new Alternative(name: "brackets + spaces")
            {
                " (", new NonTerminal("rus_oper_code"), ") "
            }
        );

        // Мобильные номера РФ
        grammar.Rule(new Rule("rus_mob_phone")
        {
            new Alternative(name: "+7/7/8(DEF)xxx-xx-xx", weight: 2400)
            {
                grammar.NewRule(
                    new Alternative("+7", weight: 1800),
                    new Alternative("7",  weight: 4000),
                    new Alternative("8",  weight:  500)
                ),
                new NonTerminal("rus_oper_code"),
                new NonTerminal("phone_7")
            },
            // Полностью форматированный
            new Alternative(name: "+7 (DEF) xxx-xx-xx formatted rich", weight: 500)
            {
                new Terminal("+7"),
                new NonTerminal("rus_city_code_fmt"),
                new NonTerminal("phone_7_formatted")
            },
            // (10 симв.) мобильный номер без кода страны и плюса
            new Alternative(name: "(DEF)xxx-xx-xx", weight: 240)
            {
                new NonTerminal("rus_oper_code"),
                new NonTerminal("phone_7")
            },
            // (9 симв.) мобильный номер без кода страны и плюса
            new Alternative(name: "(DEF)xxx-xx-xx", weight: 37)
            {
                new NonTerminal("phone_7")
            },
            // (8 симв.)
            new Alternative(name: "phone8", weight: 4.6)
            {
                new NonTerminal("digit_nozero"),
                new NonTerminal("digit", repeats: 7)
            },
            // (7 симв.) мобильный номер без кода страны, плюса и кода оператора
            new Alternative(name: "xxx-xx-xx", weight: 8.0)
            {
                new NonTerminal("phone_7")
            }
        });

        #endregion

        grammar.Validate();
        var generator = new TextGenerator(grammar);

        Console.WriteLine($"[Grammar]\n{grammar.Describe()}\n\n----------------------------------\n");
        for (var i = 0; i < 5; i++)
        {
            var (text, tree) = generator.GenerateWithTree("rus_home_phone");
            Console.WriteLine($"Generated \"{text}\". Production tree\n{tree.Render(grammar: grammar)}\n");
        }

        Console.WriteLine();

        for (var i = 0; i < 5; i++)
        {
            var (text, tree) = generator.GenerateWithTree("rus_mob_phone");
            Console.WriteLine($"Generated \"{text}\". Production tree\n{tree.Render(grammar: grammar)}\n");
        }
    }
}

