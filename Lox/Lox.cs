using System;
using System.Collections.Generic;
using System.IO;

namespace Lox
{
    public class Lox
    {
        private static readonly Interpreter Interpreter = new Interpreter();
        private static bool _hadError;
        private static bool _hadRuntimeError;

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

            if (_hadError)
            {
                System.Environment.Exit(65);
            }

            if (_hadRuntimeError)
            {
                System.Environment.Exit(70);
            }
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                _hadError = false;
            }
        }

        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var statements = parser.Parse();

            if (_hadError) return;

            Interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, string.Empty, message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
            _hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[Line {line}] Error{where}: {message}");
            _hadError = true;
        }

        private static void PrintAst(List<Stmt> statements)
        {
            var printer = new AstPrinter();

            foreach (var statement in statements)
                Console.WriteLine(printer.Print(statement));
        }
    }
}