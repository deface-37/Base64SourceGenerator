using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Base64SourceGenerator;

public partial class Base64SourceGenerator
{
    private static bool SyntaxProviderPredicate(SyntaxNode node, CancellationToken token)
    {
        return node is MethodDeclarationSyntax
               {
                   AttributeLists.Count: > 0,
                   ParameterList.Parameters.Count: 0
               } method &&
               method.Modifiers.Any(SyntaxKind.PartialKeyword) && 
               !method.Modifiers.Any(SyntaxKind.AbstractKeyword);
    }

    private static MethodInfo? SyntaxProviderTransform(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken token)
    {
        if (syntaxContext.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }
        
        var methodDeclarationSyntax = (MethodDeclarationSyntax)syntaxContext.TargetNode;

        if (methodDeclarationSyntax.Parent is not TypeDeclarationSyntax typeSymbol)
        {
            return null;
        }

        var ns = methodSymbol.ContainingType.ContainingNamespace.ToDisplayString();
        var typeInfo = new TypeInfo(typeSymbol.Keyword.ValueText, typeSymbol.Identifier.ValueText, ns);

        var compilation = syntaxContext.SemanticModel.Compilation;
        var generatedAttributeSymbol = compilation.GetTypeByMetadataName(AttributeFullname);
        if (generatedAttributeSymbol is null)
        {
            return null;
        }
        
        var attributes = methodSymbol.GetAttributes();
        
        foreach (var attributeData in attributes)
        {
            if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, generatedAttributeSymbol))
            {
                continue;
            }

            var fileName = attributeData.ConstructorArguments[0].ToCSharpString().Trim('"');
            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            return new MethodInfo(methodDeclarationSyntax.Identifier.ValueText,
                methodDeclarationSyntax.Modifiers.ToString(), fileName, typeInfo);
        }

        return null;
    }
}