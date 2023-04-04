using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UnlimitSoft.CQRS.SourceGenerator;


/// <summary>
/// Generate dependency injection for all Query, Command and Events equivalent to AddUnlimitSoftCQRS
/// </summary>
[Generator]
public sealed class IServiceConnectionExtensionsGenerator : ISourceGenerator
{
    private const string InterfaceName = "IMyCommandHandler";


    public void Execute(GeneratorExecutionContext context)
    {
        var attr = context.Compilation.Assembly.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name == "CommandHandlerAttribute");
        var inter = attr?.ConstructorArguments.First().Value as INamedTypeSymbol;

        var inRef = context.Compilation.References.ToArray();
        var extRef = context.Compilation.ExternalReferences.ToArray();
        var syntaxTrees = context.Compilation.SyntaxTrees;

        var commandHandlerTypes = new List<INamedTypeSymbol>();
        foreach (var syntaxTree in syntaxTrees)
        {
            var root = syntaxTree.GetCompilationUnitRoot();
            var model = context.Compilation.GetSemanticModel(syntaxTree);

            var classDeclarations = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(c =>
                {
                    var types = c.BaseList?.Types;
                    if (types?.Any() != true)
                        return false;

                    var a = types.Value.Any(b =>
                    {
                        return b.Type?.ToFullString().Contains(inter.Name) == true;
                    }) == true;
                    return a;
                });

            // Add each matching class to the list of command handlers
            foreach (var classDeclaration in classDeclarations)
            {
                var symbol = model.GetDeclaredSymbol(classDeclaration);
                if (symbol is not null)
                    commandHandlerTypes.Add(symbol);
            }
        }
        var sb = new StringBuilder(@"
using Microsoft.Extensions.DependencyInjection;
using System;

namespace UnlimitSoft.CQRS.DependencyInjection
{
    public static class IServiceConnectionExtensions
    {
        public static void AddUnlimitSoftCQRSGen(this IServiceCollection services)
        {
");

        foreach (var type in commandHandlerTypes)
        {
            //var handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(argsTypes);
            //var requestHandlerInterface = typeof(IRequestHandler<,>).MakeGenericType(argsTypes);
            //var currHandlerInterface = commandHandlerType.MakeGenericType(argsTypes);
            
            var commandHandlerInterface = type.AllInterfaces.FirstOrDefault(i => i.ToString().StartsWith("UnlimitSoft.CQRS.Command.ICommandHandler<"));
            var requestHandlerInterface = type.AllInterfaces.FirstOrDefault(i => i.ToString().StartsWith("UnlimitSoft.Mediator.IRequestHandler<"));
            var baseInterface = type.AllInterfaces.FirstOrDefault(i => i.ToString().StartsWith(inter.ToString()));

            sb.Append("            services.AddScoped<").Append(commandHandlerInterface).Append(", ").Append(type.ToString()).AppendLine(">();");
            sb.Append("            services.AddScoped<").Append(requestHandlerInterface).AppendLine($">(provider => provider.GetRequiredService<{commandHandlerInterface}>());");
            //services.AddScoped(handlerInterface, handlerImplementation);
            //if (currHandlerInterface != handlerInterface)
            //    services.AddScoped(currHandlerInterface, provider => provider.GetRequiredService(handlerInterface));
            //if (requestHandlerInterface != handlerInterface)
            //    services.AddScoped(requestHandlerInterface, provider => provider.GetRequiredService(handlerInterface));
        }
        sb.Append(@"
        }
    }
}");
       

        // inject the created source into the users compilation
        context.AddSource(nameof(IServiceConnectionExtensionsGenerator), SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUGs
        if (!Debugger.IsAttached)
            Debugger.Launch();
#endif
    }
}
