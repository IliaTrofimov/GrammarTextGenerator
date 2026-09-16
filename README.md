# Text generator with formal grammar


## Grammar definition
First of all you need to define your grammar. Create new `Grammar` instance and use its methods to add new `Rule`s. Rules are made of `Alternative`s. Each alternative variant is selected randomly with some given weight.
All symbols listed in specific alternative will be printed sequentially.
```csharp
 var grammar = new Grammar();
 grammar.NewRule("Message",
     new Alternative()
     {
         grammar.NewRule("Greeting",
             new Alternative(new Terminal("Hello"), weight: 1),
             new Alternative(new Terminal("Good morning"), weight: 0.5),
             new Alternative(new Terminal("Good afternoon"), weight: 0.2)
         ),
         new Terminal("! "),
         new NonTerminal("Question"),
         new Terminal("?")
     }
 );
 grammar.NewRule("Question",
    new Alternative(name: "polite question")
    {
         new Terminal("How "),
         grammar.NewRule(
             new Alternative(new Terminal("are you"), weight: 1),
             new Alternative(new Terminal("was your day"), weight: 2),
             new Alternative(new Terminal("do you do"), weight: 0.1)
         ),
    },
    new Alternative(name: "appointment")
    {
         new Terminal("Would you like "),
         grammar.NewRule(
             new Alternative(new Terminal("to go to cinema")),
             new Alternative(new Terminal("to ..."))
         ),
    }
);
```

Now you can validate and print your grammar like this
```csharp
grammar.Validate();
Console.WriteLine(grammar.Describe());
```
```
<Greeting> ::= "Hello" | "Good morning" /*w:0,5*/ | "Good afternoon" /*w:0,2*/

<Message> ::= <Greeting> "! " <Question> "?"

<Question> ::= "How " <_rule_1> /*n:polite question*/ | "Would you like " <_rule_2> /*n:appointment*/
[Grammar]
<Greeting> ::= "Hello" | "Good morning" /*w:0,5*/ | "Good afternoon" /*w:0,2*/

<Message> ::= <Greeting> "! " <Question> "?"

<Question> ::= "How " <_rule_1> /*n:polite question*/ | "Would you like " <_rule_2> /*n:appointment*/
```

## Generation
To generate some text you need to create `TextGenerator` and pass `Grammar` instance to it. Then use `GenerateWithTree` or `Generate` to create text. Simple message grammar from this example will produce such outputs
```csharp
var (text, tree) = generator.GenerateWithTree("Message");
Console.WriteLine($"Generated \"{text}\". Production tree\n{tree.Render(grammar: grammar)}\n");
```

```
Generated "Good morning! How was your day?". Production tree
<Message>  [alt. 0]
  <Greeting>  [alt. 1]
    "Good morning"
  "! "
  <Question>  [alt. 0]
    "How "
    "was your day"
  "?"

Generated "Hello! Would you like to ...?". Production tree
<Message>  [alt. 0]
  <Greeting>  [alt. 0]
    "Hello"
  "! "
  <Question>  [alt. 1]
    "Would you like "
    "to ..."
  "?"
```
