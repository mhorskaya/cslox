using System;
using System.Collections.Generic;
using System.IO;

namespace GenerateAst
{
    public class GenerateAst
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: GenerateAst <output directory>");
                Environment.Exit(1);
            }

            var outputDir = args[0];
            DefineAst(outputDir, "Expr", new List<string>
            {
                "BinaryExpr   : Expr left, Token @operator, Expr right",
                "GroupingExpr : Expr expression",
                "LiteralExpr  : object value",
                "UnaryExpr    : Token @operator, Expr right"
            });

            DefineAst(outputDir, "Stmt", new List<string>
            {
                "ExpressionStmt : Expr expression",
                "PrintStmt      : Expr expression"
            });
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            var path = $"{outputDir}/{baseName}.cs";
            var writer = new StreamWriter(path);
            var isExpression = baseName == "Expr";

            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine();
            writer.WriteLine("namespace Lox");
            writer.WriteLine("{");
            writer.WriteLine($"    public abstract class {baseName}");
            writer.WriteLine("    {");

            DefineVisitor(writer, baseName, isExpression, types);
            writer.WriteLine();

            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                var className = type.Split(":")[0].Trim();
                var fields = type.Split(":")[1].Trim();
                DefineType(writer, baseName, className, isExpression, fields);
                if (i < types.Count - 1)
                {
                    writer.WriteLine();
                }
            }

            writer.WriteLine();
            writer.WriteLine($"        public abstract {(isExpression ? "T" : "void")} Accept{(isExpression ? "<T>" : "")}(IVisitor{(isExpression ? "<T>" : "")} visitor);");

            writer.WriteLine("    }");
            writer.WriteLine("}");
            writer.Close();
        }

        private static void DefineVisitor(StreamWriter writer, string baseName, bool isExpression, List<string> types)
        {
            writer.WriteLine($"        public interface IVisitor{(isExpression ? "<T>" : "")}");
            writer.WriteLine("        {");

            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                var typeName = type.Split(":")[0].Trim();
                writer.WriteLine($"            {(isExpression ? "T" : "void")} Visit{typeName}({typeName} {baseName.ToLower()});");
                if (i < types.Count - 1)
                {
                    writer.WriteLine();
                }
            }

            writer.WriteLine("        }");
        }

        private static void DefineType(StreamWriter writer, string baseName, string className, bool isExpression, string fieldList)
        {
            writer.WriteLine($"        public class {className} : {baseName}");
            writer.WriteLine("        {");
            writer.WriteLine($"            public {className}({fieldList})");
            writer.WriteLine("            {");

            var fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                var name = field.Split(" ")[1];
                writer.WriteLine($"                {ToUpper(name)} = {name};");
            }
            writer.WriteLine("            }");
            writer.WriteLine();

            writer.WriteLine($"            public override {(isExpression ? "T" : "void")} Accept{(isExpression ? "<T>" : "")}(IVisitor{(isExpression ? "<T>" : "")} visitor)");
            writer.WriteLine("            {");
            writer.WriteLine($"               {(isExpression ? "return" : "")} visitor.Visit{className}(this);");
            writer.WriteLine("            }");
            writer.WriteLine();

            foreach (var field in fields)
            {
                var type = field.Split(" ")[0];
                var name = field.Split(" ")[1];
                writer.WriteLine($"            public {type} {ToUpper(name)} {{ get; }}");
            }
            writer.WriteLine("        }");
        }

        private static string ToUpper(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return string.Empty;
            }

            if (word == "@operator")
            {
                return "Operator";
            }

            return char.ToUpper(word[0]) + word.Substring(1);
        }
    }
}