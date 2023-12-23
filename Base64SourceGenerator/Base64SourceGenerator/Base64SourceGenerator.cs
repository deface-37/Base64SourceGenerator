using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Base64SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class Base64SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(initializationContext => initializationContext.AddSource(
            "Base64Attribute.g.cs", AttributeSourceCode));

        var syntaxProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(AttributeFullname,
                SyntaxProviderPredicate,
                SyntaxProviderTransform)
            .Where(methodInfo => methodInfo is not null)
            .Select((info, _) => info!.Value);

        IncrementalValueProvider<ImmutableArray<(AdditionalText File, MethodInfo Method)>> valuesProvider =
            syntaxProvider.Combine(context.AdditionalTextsProvider.Collect())
                .Select((tuple, _) =>
                {
                    var methodInfo = tuple.Left;
                    var files = tuple.Right;
                    
                    var correspondFile = files.FirstOrDefault(file => file.Path.EndsWith(methodInfo.FileName));
                    return (File: correspondFile, Method: methodInfo);
                })
                .Where(x => x.File is not null)
                .Collect()!;
        
        context.RegisterSourceOutput(valuesProvider, GenerateOutputAction);
    }

    private static void GenerateOutputAction(SourceProductionContext context,
        ImmutableArray<(AdditionalText File, MethodInfo MethodInfo)> methodAndFile)
    {
        
        foreach (var (typeName, source) in GenerateSource(methodAndFile, context.CancellationToken))
        {
            context.AddSource($"{typeName}.g.cs", source);
        }
    }
}