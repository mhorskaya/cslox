using System;
using System.Collections.Generic;
using System.IO;

namespace Lox
{
    public class Lox
    {
        public static Interpreter Interpreter { get; } = new Interpreter();
        public static bool HadError { get; private set; }
        public static bool HadRuntimeError { get; private set; }

        private static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: Lox [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            Run(File.ReadAllText(path));
            if (HadError) System.Environment.Exit(65);
            if (HadRuntimeError) System.Environment.Exit(70);
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                HadError = false;
            }
        }

        private static void Run(string source)
        {
            var tokens = new Scanner(source).ScanTokens();
            var statements = new Parser(tokens).Parse();

            if (HadError) return;

            new Resolver(Interpreter).Resolve(statements);

            if (HadError) return;

            Interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, string.Empty, message);
        }

        public static void Error(Token token, string message)
        {
            Report(token.Line, token.Type == TokenType.EOF ? " at end" : $" at '{token.Lexeme}'", message);
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
            HadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[Line {line}] Error{where}: {message}");
            HadError = true;
        }

        private static void PrintAst(List<Stmt> statements)
        {
            var printer = new AstPrinter();

            foreach (var statement in statements)
                Console.WriteLine(printer.Print(statement));
        }
    }
}