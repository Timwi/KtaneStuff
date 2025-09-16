using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RT.Json;
using RT.Modeling;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static RT.Modeling.Md;

namespace KtaneStuff;

internal static class Souvenir
{
    public static void DoModels()
    {
        File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightVeryLong.obj", GenerateObjFile(answerHighlight(.39), "AnswerHighlightVeryLong"));
        File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightLong.obj", GenerateObjFile(answerHighlight(.21), "AnswerHighlightLong"));
        File.WriteAllText(@"D:\c\KTANE\Souvenir\Assets\Models\AnswerHighlightShort.obj", GenerateObjFile(answerHighlight(.15), "AnswerHighlightShort"));
    }

    private static IEnumerable<Pt[]> answerHighlight(double width)
    {
        var height = .08;
        var padding = .02;
        var right = width - height / 2;
        return
            Enumerable.Range(0, 37)
                .Select(i => i * 180 / 36 + 90)
                .Select(angle => new { Inner = pt((height - padding) / 2 * cos(angle), 0, (height - padding) / 2 * sin(angle)), Outer = pt(height / 2 * cos(angle), 0, height / 2 * sin(angle)) })
                .SelectConsecutivePairs(false, (i1, i2) => new[] { i1.Inner, i1.Outer, i2.Outer, i2.Inner })
            .Concat([pt(0, 0, -height / 2), pt(right, 0, -height / 2), pt(right, 0, -(height - padding) / 2), pt(0, 0, -(height - padding) / 2)])
            .Concat([pt(0, 0, (height - padding) / 2), pt(right, 0, (height - padding) / 2), pt(right, 0, height / 2), pt(0, 0, height / 2)])
            .Concat([pt(right, 0, -height / 2), pt(right, 0, height / 2), pt(right - height / 3, 0, 0)]);
    }

    public static void FindHangingCoroutinesInLogfiles()
    {
        //foreach (var file in new DirectoryInfo(@"F:\KtaneLogfiles").EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        //foreach (var file in new DirectoryInfo(@"D:\temp").EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        foreach (var file in new DirectoryInfo(@"C:\Users\Timwi\AppData\LocalLow\Steel Crate Games\Keep Talking and Nobody Explodes").EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        {
            var contents = File.ReadLines(file.FullName);
            var running = new Dictionary<string, int>();
            foreach (var line in contents)
                if (line.RegexMatch(@"^[<‹]Souvenir #\d+[>›] Module (\w+): (Start processing|Finished processing)\.$", out var m))
                    running.IncSafe(m.Groups[1].Value, m.Groups[2].Value.Equals("Start processing") ? 1 : -1);
            foreach (var kvp in running)
                if (kvp.Value != 0)
                    ConsoleUtil.WriteLine($"{file.Name.Color(ConsoleColor.Red)}: {kvp.Key.Color(ConsoleColor.Cyan)} is {kvp.Value.ToString().Color(ConsoleColor.Magenta)}", null);
        }
    }

    public static void UpdateJs()
    {
        var file = File.ReadAllLines(@"D:\c\KTANE\Public\HTML\js\Modules\Souvenir.js");
        var modules = Ktane.GetLiveJson().Where(md => md["Type"].GetString() != "Appendix").ToDictionary(md => md["Name"].GetString(), md => (id: md["ModuleID"].GetString(), filename: md.Safe["FileName"]?.GetString() ?? md["Name"].GetString()));
        for (var i = 0; i < file.Length; i++)
            if (file[i].RegexMatch(@"name: ""([^""]*)"",\tid: ""\?""", out var m) && modules.Get(m.Groups[1].Value.Replace("’", "'"), null) is { } tup)
            {
                file[i] = file[i].Remove(m.Index, m.Length).Insert(m.Index, $"name: \"{m.Groups[1].Value}\",\tid: \"{tup.id}\"");
                var json = JsonDict.Parse(File.ReadAllText($@"D:\c\KTANE\Public\JSON\{tup.filename}.json"));
                json["Souvenir"] = new JsonDict { ["Status"] = "Supported" };
                File.WriteAllText($@"D:\c\KTANE\Public\JSON\{tup.filename}.json", json.ToStringIndented());
            }
        File.WriteAllLines(@"D:\c\KTANE\Public\HTML\js\Modules\Souvenir.js", file);
    }

    public static void ConvertFromOldCode(bool @override = false)
    {
        if (!@override)
            throw new Exception("Don’t run this anymore");
        var dic = new Dictionary<string, (string moduleType, string author)>();
        var dicRoot = CSharpSyntaxTree.ParseText(File.ReadAllText(@"D:\c\KTANE\Souvenir-old\Lib\Modules_General.cs")).GetRoot();
        var dicCls = dicRoot.ChildNodes().OfType<ClassDeclarationSyntax>().Single(c => c.Identifier.Value.Equals("SouvenirModule"));
        if (dicCls.ChildNodes().OfType<MethodDeclarationSyntax>().SingleOrDefault(m => m.Identifier.Value.Equals("Awake")) is { } mth
                && mth.ExpressionBody is { } eb
                && eb.Expression is AssignmentExpressionSyntax a
                && a.Left is IdentifierNameSyntax ins
                && ins.Identifier.Value.Equals("_moduleProcessors")
                && a.Right is ObjectCreationExpressionSyntax oce)
        {
            foreach (var expr in oce.Initializer.Expressions)
                if (expr is AssignmentExpressionSyntax
                    {
                        Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: { } leftArgs },
                        Right: TupleExpressionSyntax { Arguments: { } tupleArgs }
                    }
                        && leftArgs[0].Expression is LiteralExpressionSyntax { Token.Value: string moduleType }
                        && tupleArgs[1].Expression is LiteralExpressionSyntax { Token.Value: string moduleName }
                        && tupleArgs[2].Expression is LiteralExpressionSyntax { Token.Value: string author })
                    dic.Add(moduleName, (moduleType, author));
        }

        var questionsEnum = CSharpSyntaxTree.ParseText(File.ReadAllText(@"D:\c\KTANE\Souvenir-old\Lib\Question.cs"))
            .GetRoot().ChildNodes().OfType<FileScopedNamespaceDeclarationSyntax>()
            .SelectMany(decl => decl.ChildNodes().OfType<EnumDeclarationSyntax>())
            .Single();

        var questionAttrs = new Dictionary<string, (string name, string moduleName, bool addThe, AttributeSyntax qAttr, AttributeSyntax[] otherAttrs)>();

        foreach (var elem in questionsEnum.ChildNodes().OfType<EnumMemberDeclarationSyntax>())
        {
            var name = elem.Identifier.ValueText;
            var allAttr = elem.AttributeLists.SelectMany(l => l.Attributes).OfType<AttributeSyntax>().ToArray();
            var questionAttr = allAttr.Single(a => a.Name is SimpleNameSyntax sns && sns.Identifier.Value.Equals("SouvenirQuestion"));
            var moduleName = questionAttr.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax { } l ? l.Token.Value as string : null;
            var newArgs = questionAttr.ArgumentList.Arguments.GetWithSeparators().RemoveAt(1).RemoveAt(1);
            var addTheIx = newArgs.IndexOf(x => x.IsNode && x.AsNode() is AttributeArgumentSyntax a && a.NameEquals is { } ne && ne.Name.Identifier.Value.Equals("AddThe") && a.Expression is LiteralExpressionSyntax { } le && le.Token.Value.Equals(true));
            var addThe = addTheIx >= 0;
            if (addTheIx >= 0)
            {
                newArgs = newArgs.RemoveAt(addTheIx - 1);
                newArgs = newArgs.RemoveAt(addTheIx - 1);
            }
            for (var i = 0; i < newArgs.Count; i++)
                if (newArgs[i].IsToken)
                {
                    var tok = newArgs[i].AsToken().WithoutTrivia().WithTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "));
                    newArgs = newArgs.RemoveAt(i);
                    newArgs = newArgs.Insert(i, tok);
                }
                else
                {
                    var node = newArgs[i].AsNode().WithoutTrivia();
                    newArgs = newArgs.RemoveAt(i);
                    newArgs = newArgs.Insert(i, node);
                }
            var v = new arrayConversionVisitor();
            var newQuestionAttr = questionAttr.WithArgumentList(questionAttr.ArgumentList.WithArguments(SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(newArgs)));
            newQuestionAttr = v.Visit(newQuestionAttr) as AttributeSyntax;
            questionAttrs.Add(name, (name, moduleName, addThe, newQuestionAttr, allAttr.Except([questionAttr]).ToArray()));
        }

        var methods = new List<(MethodDeclarationSyntax method, string moduleName, bool addThe, string enumTypeName, (string name, AttributeSyntax qAttr, AttributeSyntax[] otherAttrs)[] questions)>();
        foreach (var file in new DirectoryInfo(@"D:\c\KTANE\Souvenir-old\Lib").EnumerateFiles("Modules*.cs", SearchOption.AllDirectories))
            if (file.Name.RegexMatch(@"^Modules.\.cs$", out _))
            {
                Console.WriteLine(file.FullName);
                var oldCs = File.ReadAllText(file.FullName);
                var tree = CSharpSyntaxTree.ParseText(oldCs);
                foreach (var methodSyntax in tree.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>()
                        .SelectMany(decl => decl.ChildNodes().OfType<MethodDeclarationSyntax>()))
                    if (methodSyntax.Identifier.Value is string s && s.RegexMatch(@"^[Pp]rocess(\w+)$", out var m))
                    {
                        var v = new methodVisitor();
                        v.Visit(methodSyntax);
                        if (v.EnumNames.Any(e => !e.RegexMatch($@"^_?{Regex.Escape(m.Groups[1].Value)}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, out _)))
                            Debugger.Break();
                        var moduleName = questionAttrs[v.EnumNames.First()].moduleName;
                        var addThe = questionAttrs[v.EnumNames.First()].addThe;
                        methods.Add((methodSyntax, moduleName, addThe, m.Groups[1].Value, v.EnumNames
                            .Select(name => questionAttrs[name])
                            .Select(tup => (tup.name.RegexReplace($@"^_?{Regex.Escape(m.Groups[1].Value)}", "", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), tup.qAttr, tup.otherAttrs)).ToArray()));
                    }
            }

        foreach (var (method, moduleName, addThe, enumTypeName, questions) in methods)
        {
            var (moduleType, author) = dic[(addThe ? $"{moduleName}, The" : moduleName).RegexReplace(@"[\uE001\uE002]", "")];
            var enumDeclarations = new List<string>();
            foreach (var (name, qAttr, otherAttrs) in questions)
                enumDeclarations.Add($"""
                        {qAttr.Concat(otherAttrs).Select(attr => $"[{attr}]").JoinString("\r\n")}
                        {sanitizeQuestionName(name)}
                        """);

            File.WriteAllText(Path.Combine(@"D:\c\KTANE\Souvenir\Lib\Handlers", $"{enumTypeName}.cs"), $$"""
                    using System.Collections.Generic;
                    using System.Linq;
                    using Souvenir;
                    using UnityEngine;

                    using static Souvenir.AnswerLayout;

                    public enum S{{enumTypeName}}
                    {
                    {{enumDeclarations.JoinString(",\r\n\r\n").Indent(4)}}
                    }

                    public partial class SouvenirModule
                    {
                        [SouvenirHandler("{{moduleType.CLiteralEscape()}}", "{{moduleName.CLiteralEscape()}}", typeof(S{{enumTypeName}}), "{{author.CLiteralEscape()}}"{{(addThe ? ", AddThe = true" : "")}})]
                        private IEnumerator<SouvenirInstruction> Process{{enumTypeName}}(ModuleData module){{(method.ExpressionBody == null ? "" : $" {method.ExpressionBody};")}}{{(method.Body == null ? "" : $"\r\n    {method.Body}")}}
                    }
                    """);
        }
    }

    class methodVisitor : CSharpSyntaxRewriter
    {
        public HashSet<string> EnumNames = [];
        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.Expression.ToString() == "Question")
            {
                EnumNames.Add(node.Name.ToString());
                return node;
            }
            return base.VisitMemberAccessExpression(node);
        }
    }

    class arrayConversionVisitor : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node) =>
            CollectionExpression(SeparatedList<CollectionElementSyntax>(node.Initializer.Expressions.GetWithSeparators()
                .Select(e => e.IsNode ? ExpressionElement(e.AsNode() as ExpressionSyntax).WithoutTrailingTrivia() : (SyntaxNodeOrToken) e.AsToken())));
    }

    private static string sanitizeQuestionName(string name) => name == "" || char.IsAsciiDigit(name[0]) ? $"Question{name}" : name;

    private static readonly object _lockObject = new();

    public static void ConvertHandlers()
    {
        var files = new DirectoryInfo(@"D:\c\KTANE\Souvenir\Lib\Handlers").EnumerateFiles("*.cs", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            if (file.Name == "General.cs")
                continue;
            var text = File.ReadAllText(file.FullName);
            var cs = CSharpSyntaxTree.ParseText(text);
            var enumType = cs.GetRoot().ChildNodes().OfType<EnumDeclarationSyntax>().Single();
            var partialClass = cs.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().Single();
            var method = partialClass.ChildNodes().OfType<MethodDeclarationSyntax>().Single();
            var enumTypeName = (string) enumType.Identifier.Value;

            var newMethod = method;
            if (method.ExpressionBody is { } exprBody)
                newMethod = method.WithExpressionBody((ArrowExpressionClauseSyntax) new enumValueVisitor(enumTypeName).Visit(exprBody));
            else if (method.Body is { } body)
            {
                body = processWaitForSeconds(body);
                body = processQuestionCreations(body);
                body = (BlockSyntax) new addQuestionVisitor().Visit(body);
                newMethod = method.WithBody(body);
            }

            if (newMethod != method)
            {
                var newRoot = cs.GetRoot().ReplaceNode(method, newMethod);
                File.WriteAllText(file.FullName, newRoot.ToString());
                lock (_lockObject)
                    ConsoleUtil.WriteLineFmt($"{file.FullName:G}");
            }
        }
    }

    public static void ConvertGeneralHandlers()
    {
        const string generalHandlersCs = @"D:\c\KTANE\Souvenir\Lib\Handlers\General.cs";
        var text = File.ReadAllText(generalHandlersCs);
        var cs = CSharpSyntaxTree.ParseText(text);
        var partialClass = cs.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().Single();
        foreach (var method in partialClass.ChildNodes().OfType<MethodDeclarationSyntax>())
        {
            var newMethod = method;
            if (method.Body is { } body)
            {
                body = processWaitForSeconds(body);
                body = processQuestionCreations(body);
                newMethod = method.WithBody(body);
            }

            if (newMethod != method)
                cs = cs.WithRootAndOptions(cs.GetRoot().ReplaceNode(method, newMethod), null);
        }
        File.WriteAllText(generalHandlersCs, cs.ToString());
    }

    private static BlockSyntax processWaitForSeconds(BlockSyntax body)
    {
        var newBody = body;
        for (var i = 0; i < newBody.Statements.Count; i++)
        {
            if (newBody.Statements[i] is WhileStatementSyntax
                {
                    Condition: PrefixUnaryExpressionSyntax
                    {
                        OperatorToken.Value: "!",
                        Operand: IdentifierNameSyntax { Identifier.Value: string condition }
                    },
                    Statement: YieldStatementSyntax
                    {
                        ReturnOrBreakKeyword.Value: "return",
                        Expression: ObjectCreationExpressionSyntax
                        {
                            Type: IdentifierNameSyntax { Identifier.Value: "WaitForSeconds" },
                            ArgumentList.Arguments.Count: 1
                        }
                    }
                })
            {
                if (condition == "finished" && body.Parent is MethodDeclarationSyntax { Identifier.Value: "ProcessMonsplodeFight" })
                    continue;
                newBody = newBody.ReplaceNode(newBody.Statements[i],
                    YieldStatement(SyntaxKind.YieldReturnStatement,
                        Token(SyntaxKind.YieldKeyword).WithTrailingTrivia(Whitespace(" ")),
                        Token(SyntaxKind.ReturnKeyword).WithTrailingTrivia(Whitespace(" ")),
                        IdentifierName(condition switch
                        {
                            "_noUnignoredModulesLeft" => "WaitForUnignoredModules",
                            "_isActivated" => "WaitForActivate",
                            _ => throw new NotImplementedException()
                        }),
                        Token(SyntaxKind.SemicolonToken)).WithTriviaFrom(newBody.Statements[i]));
            }
        }
        return newBody;
    }

    private static BlockSyntax processQuestionCreations(BlockSyntax body)
    {
        // Detect ‘addQuestions()’ using the params form
        for (var i = 0; i < body.Statements.Count; i++)
            if (body.Statements[i] is ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax { Identifier.Value: "addQuestions" },
                        ArgumentList.Arguments: [ArgumentSyntax { Expression: IdentifierNameSyntax { Identifier.Value: "module" } }, ..] argList
                    } expr
                } && argList.Skip(1).All(arg => arg is { Expression: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Value: "makeQuestion" } } }))
            {
                var newStatements = body.Statements.RemoveAt(i);
                for (var j = 1; j < argList.Count; j++)
                {
                    var conv = convertMakeOrAddQuestionExpr(argList[j].Expression);
                    if (conv == null)
                        Debugger.Break();
                    newStatements = newStatements.Insert(i + j - 1, conv.WithLeadingTrivia(body.Statements[i].GetLeadingTrivia()).WithTrailingTrivia(EndOfLine(Environment.NewLine)));
                }
                return body.WithStatements(newStatements);
            }

        // Detect ‘addQuestions()’ with a .Select() iterator with two parameters (variable and index)
        for (var i = 0; i < body.Statements.Count; i++)
            if (body.Statements[i] is ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax { Identifier.Value: "addQuestions" },
                        ArgumentList.Arguments: [{ Expression: IdentifierNameSyntax { Identifier.Value: "module" } },
                        {
                            Expression: InvocationExpressionSyntax
                            {
                                Expression: MemberAccessExpressionSyntax { Expression: var source, Name: IdentifierNameSyntax { Identifier.Value: "Select" } },
                                ArgumentList.Arguments: [
                                    {
                                        Expression: ParenthesizedLambdaExpressionSyntax
                                        {
                                            ParameterList.Parameters: [{ Identifier.Value: string elemVar }, { Identifier.Value: string ixVar }],
                                            ExpressionBody: var lambda
                                        }
                                    }]
                            }
                        }]
                    }
                } expr)
            {
                ExpressionSyntax condition = null;
                if (lambda is ConditionalExpressionSyntax { Condition: var cond, WhenTrue: LiteralExpressionSyntax { Token.Value: null }, WhenFalse: var c })
                {
                    condition = (ExpressionSyntax) new varSubstVisitor(elemVar, source, ixVar).Visit(PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, ParenthesizedExpression(cond)));
                    lambda = c;
                }
                else if (lambda is ConditionalExpressionSyntax { Condition: var cond2, WhenFalse: LiteralExpressionSyntax { Token.Value: null }, WhenTrue: var c2 })
                {
                    condition = (ExpressionSyntax) new varSubstVisitor(elemVar, source, ixVar).Visit(cond2);
                    lambda = c2;
                }

                LocalDeclarationStatementSyntax sourceDecl = null;
                if (source is not IdentifierNameSyntax)
                {
                    sourceDecl = LocalDeclarationStatement(VariableDeclaration(IdentifierName("var").WithTrailingTrivia(Whitespace(" ")), [VariableDeclarator(Identifier("source"), null, EqualsValueClause(source.WithoutTrivia()))]));
                    source = IdentifierName("source");
                }

                var converted = convertMakeOrAddQuestionExpr(lambda);
                if (converted == null)
                    Debugger.Break();
                var statement = ((StatementSyntax) new varSubstVisitor(elemVar, source, ixVar).Visit(converted)).WithLeadingTrivia(EndOfLine(Environment.NewLine));
                if (condition != null)
                    statement = IfStatement(condition, statement).WithLeadingTrivia(EndOfLine(Environment.NewLine));
                var newBody = body.ReplaceNode(expr, ForStatement(
                    VariableDeclaration(IdentifierName("var").WithTrailingTrivia(Whitespace(" ")), [VariableDeclarator(Identifier(ixVar), null, EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))]),
                    [],
                    BinaryExpression(SyntaxKind.LessThanExpression, IdentifierName(ixVar), MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, IdentifierName("Length"))),
                    [PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName(ixVar))],
                    statement).WithTriviaFrom(expr));
                if (sourceDecl != null)
                    newBody = newBody.WithStatements(newBody.Statements.Insert(i, sourceDecl.WithTrailingTrivia(EndOfLine(Environment.NewLine))));
                return newBody;
            }

        // Detect ‘addQuestions()’ with a Enumerable.Range(x, y).Select() iterator with one parameter
        for (var i = 0; i < body.Statements.Count; i++)
            if (body.Statements[i] is ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax { Identifier.Value: "addQuestions" },
                        ArgumentList.Arguments: [{ Expression: IdentifierNameSyntax { Identifier.Value: "module" } },
                        {
                            Expression: InvocationExpressionSyntax
                            {
                                Expression: MemberAccessExpressionSyntax
                                {
                                    Expression: InvocationExpressionSyntax
                                    {
                                        Expression: MemberAccessExpressionSyntax
                                        {
                                            Expression: IdentifierNameSyntax { Identifier.Value: "Enumerable" },
                                            Name: IdentifierNameSyntax { Identifier.Value: "Range" }
                                        },
                                        ArgumentList.Arguments: [{ Expression: LiteralExpressionSyntax { Token.Value: int startValue } }, { Expression: LiteralExpressionSyntax { Token.Value: int count } }]
                                    },
                                    Name: IdentifierNameSyntax { Identifier.Value: "Select" }
                                },
                                ArgumentList.Arguments: [
                                {
                                    Expression: SimpleLambdaExpressionSyntax
                                    {
                                        Parameter.Identifier.Value: string ixVar,
                                        ExpressionBody: var lambda
                                    }
                                }]
                            }
                        }]
                    }
                } expr && convertMakeOrAddQuestionExpr(lambda) is { } converted)
            {
                return body.ReplaceNode(expr, ForStatement(
                    VariableDeclaration(IdentifierName("var").WithTrailingTrivia(Whitespace(" ")), [VariableDeclarator(Identifier(ixVar), null, EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(startValue))))]),
                    [],
                    BinaryExpression(SyntaxKind.LessThanExpression, IdentifierName(ixVar), LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(startValue + count))),
                    [PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName(ixVar))],
                    converted.WithLeadingTrivia(EndOfLine(Environment.NewLine))).WithTriviaFrom(expr));
            }

        // Detect ‘new List<QandA>()’, followed by statements that add to the list, followed by ‘addQuestions()’
        for (var i = 0; i < body.Statements.Count; i++)
            if (body.Statements[i] is LocalDeclarationStatementSyntax
                {
                    Declaration.Variables: [VariableDeclaratorSyntax
                    {
                        Identifier.Value: string listName,
                        Initializer.Value: ObjectCreationExpressionSyntax
                        {
                            Type: GenericNameSyntax
                            {
                                Identifier.Value: "List",
                                TypeArgumentList.Arguments: [IdentifierNameSyntax { Identifier.Value: "QandA" }]
                            },
                            ArgumentList: null or { Arguments: [] },
                            Initializer: var initializerSyntax
                        }
                    }]
                })
                for (var j = i + 1; j < body.Statements.Count; j++)
                    if (body.Statements[j] is ExpressionStatementSyntax
                        {
                            Expression: InvocationExpressionSyntax
                            {
                                Expression: IdentifierNameSyntax { Identifier.Value: "addQuestions" },
                                ArgumentList.Arguments: [
                                    ArgumentSyntax { Expression: IdentifierNameSyntax { Identifier.Value: "module" } },
                                    ArgumentSyntax { Expression: var arg2 }
                                ]
                            }
                        })
                    {
                        if ((arg2 is not IdentifierNameSyntax { Identifier.Value: string listName2 } || listName2 != listName) &&
                            (arg2 is not InvocationExpressionSyntax
                            {
                                Expression: MemberAccessExpressionSyntax
                                {
                                    Expression: IdentifierNameSyntax { Identifier.Value: string listName3 },
                                    Name.Identifier.Value: "ToArray"
                                },
                                ArgumentList.Arguments: []
                            } || listName3 != listName))
                            continue;

                        var newBody = body;
                        for (var k = i + 1; k < j; k++)
                            newBody = newBody.ReplaceNode(newBody.Statements[k], new listAddVisitor(listName).Visit(newBody.Statements[k]));
                        newBody = newBody.WithStatements(newBody.Statements.RemoveAt(j));
                        newBody = newBody.WithStatements(newBody.Statements.RemoveAt(i));
                        if (initializerSyntax != null)
                        {
                            for (var initIx = 0; initIx < initializerSyntax.Expressions.Count; initIx++)
                            {
                                var initializer = initializerSyntax.Expressions[initIx];
                                var converted = convertMakeOrAddQuestionExpr(initializer);
                                var withLeadingTrivia =
                                    initIx == 0 ? converted.WithLeadingTrivia(body.Statements[i].GetLeadingTrivia()) :
                                    converted.WithLeadingTrivia(body.Statements[i].GetLeadingTrivia().Where(tr => !tr.IsKind(SyntaxKind.EndOfLineTrivia)));
                                var withTrailingTrivia =
                                    initIx == initializerSyntax.Expressions.Count - 1 ? withLeadingTrivia.WithTrailingTrivia(body.Statements[i].GetTrailingTrivia()) :
                                    withLeadingTrivia.WithTrailingTrivia(EndOfLine(Environment.NewLine));
                                newBody = newBody.WithStatements(newBody.Statements.Insert(i + initIx, withTrailingTrivia));
                            }
                        }

                        return newBody;
                    }

        return body;
    }

    private class listAddVisitor(string listName) : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node.Expression is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax { Identifier.Value: string invokee },
                        Name: IdentifierNameSyntax { Identifier.Value: "Add" }
                    },
                    ArgumentList.Arguments: [{ } arg]
                }
                    && invokee == listName
                    && convertMakeOrAddQuestionExpr(arg.Expression) is { } converted)
                return converted.WithTriviaFrom(node);

            return base.VisitExpressionStatement(node);
        }
    }

    private static YieldStatementSyntax convertMakeOrAddQuestionExpr(ExpressionSyntax expression)
    {
        if (expression is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Value: string mth and ("makeQuestion" or "addQuestion") }, ArgumentList.Arguments: { } argList }
            && argList[mth == "makeQuestion" ? 0 : 1].Expression is { } q
            && argList[mth == "makeQuestion" ? 1 : 0].Expression is IdentifierNameSyntax { Identifier.Value: "module" })
        {
            if (argList[0].NameColon != null && !argList[0].NameColon.Name.Identifier.Value.Equals("question"))
                Debugger.Break();
            if (argList[1].NameColon != null && !argList[1].NameColon.Name.Identifier.Value.Equals("data"))
                Debugger.Break();

            var maker = new questionMaker { Question = q };
            var skip = 2;
            if (argList[2].NameColon == null)
            {
                if (argList[2].Expression.ToString().Contains("font"))
                    return null;
                maker.QuestionSprite = argList[2].Expression;
                skip = 3;
                lock (_lockObject)
                    ConsoleUtil.WriteLineFmt($"{"Sprite?":Y} {argList[2].Expression.ToString():Y}");
            }
            foreach (var arg in argList.Skip(skip))
            {
                switch (arg.NameColon)
                {
                    case { Name.Identifier.Value: "formatArgs" or "formatArguments" }: maker.Args = arg.Expression; break;
                    case { Name.Identifier.Value: "correctAnswers" }: maker.Correct = arg.Expression; break;
                    case { Name.Identifier.Value: "preferredWrongAnswers" }: maker.PrefWrong = arg.Expression; break;
                    case { Name.Identifier.Value: "allAnswers" }: maker.All = arg.Expression; break;
                    case { Name.Identifier.Value: "questionSprite" } when maker.QuestionSprite == null: maker.QuestionSprite = arg.Expression; break;
                    case { Name.Identifier.Value: "formattedModuleName" }: break;
                    case { Name.Identifier.Value: "font" }: return null;
                    case { Name.Identifier.Value: "fontTexture" }: return null;
                    case null when arg.ToString().Contains("font"): return null;
                    default: throw new NotImplementedException();
                }
            }
            static ArgumentSyntax[] mkIf(ExpressionSyntax expr, string name = null) => expr == null ? [] : [name == null ? Argument(convertArray(expr)) : Argument(NameColon(name), default, convertArray(expr))];
            static ArgumentSyntax[] mkIfSingle(ExpressionSyntax expr, string name = null) => expr == null ? [] : [name == null ? Argument(convertArray(expr, true)) : Argument(NameColon(name), default, convertArray(expr, true))];
            return YieldStatement(SyntaxKind.YieldReturnStatement,
                Token(SyntaxKind.YieldKeyword).WithTrailingTrivia(Whitespace(" ")),
                Token(SyntaxKind.ReturnKeyword).WithTrailingTrivia(Whitespace(" ")),
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        InvocationExpression(
                            IdentifierName("question"),
                            ArgumentList([.. mkIf(maker.Question), .. mkIf(maker.Args, "args"), .. mkIf(maker.QuestionSprite, "questionSprite")])),
                        IdentifierName("Answers")),
                    ArgumentList([.. mkIfSingle(maker.Correct), .. mkIf(maker.All, "all"), .. mkIf(maker.PrefWrong, "preferredWrong")])),
                Token(SyntaxKind.SemicolonToken));
        }
        return null;
    }

    private static ExpressionSyntax convertArray(ExpressionSyntax expr, bool extractIfSingleElement = false)
    {
        if (extractIfSingleElement && expr is ImplicitArrayCreationExpressionSyntax { Initializer.Expressions: [{ } arg] })
            return arg;
        if (expr is ImplicitArrayCreationExpressionSyntax node)
            return CollectionExpression(SeparatedList<CollectionElementSyntax>(node.Initializer.Expressions.GetWithSeparators()
                .Select(e => e.IsNode ? ExpressionElement(e.AsNode() as ExpressionSyntax).WithoutTrailingTrivia() : e)));
        return expr;
    }

    private class questionMaker
    {
        public ExpressionSyntax Question, Args, QuestionSprite, Correct, PrefWrong, All;
    }

    private class varSubstVisitor(string elemVar, ExpressionSyntax source, string ixVar) : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (!node.Identifier.Value.Equals(elemVar))
                return base.VisitIdentifierName(node);
            return ElementAccessExpression(source, BracketedArgumentList([Argument(IdentifierName(ixVar))]));
        }
    }

    private class enumValueVisitor(string enumTypeName) : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.Expression is not IdentifierNameSyntax { Identifier.Value: "Question" } || node.Name is not { Identifier.Value: string enumName })
                return base.VisitMemberAccessExpression(node);

            if (!enumTypeName.StartsWith('S'))
                Debugger.Break();

            if (!enumName.RegexMatch($@"^_?{Regex.Escape(enumTypeName.Substring(1))}(\w*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, out var m))
                Debugger.Break();

            return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(enumTypeName),
                    IdentifierName(sanitizeQuestionName(m.Groups[1].Value))).WithTriviaFrom(node);
        }
    }

    private class addQuestionVisitor : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node is { Expression: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Value: "addQuestion" } } expr } statement)
            {
                var converted = convertMakeOrAddQuestionExpr(expr);
                if (converted == null)
                    Debugger.Break();
                return converted.WithTriviaFrom(statement);
            }
            return base.VisitExpressionStatement(node);
        }
    }
}
