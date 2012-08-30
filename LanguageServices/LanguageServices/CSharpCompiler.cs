﻿using System;
using System.IO;
using System.Text;
using Compilify.Extensions;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.LanguageServices
{
    public class CSharpCompiler : ICodeCompiler
    {
        private const string EntryPoint = 
            @"public static class EntryPoint 
              {
                  public static object Result { get; set; }
                  
                  public static void Main()
                  {
                      Result = Script.Eval();
                  }
              }";

        private static readonly ReadOnlyArray<string> DefaultNamespaces =
            ReadOnlyArray<string>.CreateFrom(new[]
            {
                "System", 
                "System.IO", 
                "System.Net", 
                "System.Linq", 
                "System.Text", 
                "System.Text.RegularExpressions", 
                "System.Collections.Generic"
            });

        public ICodeAssembly Compile(ICodeProgram program)
        {
            var compilation = RoslynCompile(program);
            var assembly = new ProgramAssembly
                           {
                               EntryPointClassName = "EntryPoint",
                               EntryPointMethodName = "Main"
                           };

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    return null;
                }

                assembly.CompiledAssembly = stream.ToArray();
            }

            return assembly;
        }

        public Compilation RoslynCompile(ICodeProgram program)
        {
            if (program == null)
            {
                throw new ArgumentNullException("program");
            }

            var asScript = ParseOptions.Default.WithKind(SourceCodeKind.Script);

            var console =
                SyntaxTree.ParseCompilationUnit(
                    "public static readonly StringWriter __Console = new StringWriter();", options: asScript);

            var entry = SyntaxTree.ParseCompilationUnit(EntryPoint);
            var prompt = SyntaxTree.ParseCompilationUnit(BuildScript(program.Content), path: "Prompt", options: asScript);
            var editor = SyntaxTree.ParseCompilationUnit(program.Classes ?? string.Empty, path: "Editor", options: asScript);

            var compilation = RoslynCompile(program.Name ?? "Untitled", new[] { entry, prompt, editor, console });

            var newPrompt = prompt.RewriteWith(new ConsoleRewriter("__Console", compilation.GetSemanticModel(prompt)));
            var newEditor = editor.RewriteWith(new ConsoleRewriter("__Console", compilation.GetSemanticModel(editor)));

            return compilation.ReplaceSyntaxTree(prompt, newPrompt).ReplaceSyntaxTree(editor, newEditor);
        }

        public Compilation RoslynCompile(string compilationName, params SyntaxTree[] syntaxTrees)
        {
            if (string.IsNullOrEmpty(compilationName))
            {
                throw new ArgumentNullException("compilationName");
            }

            var options =
                new CompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOverflowChecks(true)
                    .WithOptimizations(true)
                    .WithUsings(DefaultNamespaces);

            var metadata = new[]
                           {
                               MetadataReference.Create("mscorlib"),
                               MetadataReference.Create("System"),
                               MetadataReference.Create("System.Core")
                           };

            var compilation = Compilation.Create(compilationName, options, syntaxTrees,  metadata);

            return compilation;
        }

        private static string BuildScript(string content)
        {
            var builder = new StringBuilder();

            builder.AppendLine("public static object Eval() {");
            builder.AppendLine("#line 1");
            builder.Append(content);
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
