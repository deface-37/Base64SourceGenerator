namespace Base64SourceGenerator;

public partial class Base64SourceGenerator
{
    private readonly record struct MethodInfo(string Name, string Modifiers, string FileName, TypeInfo Type);

    private readonly record struct TypeInfo(string Keyword, string Name, string? Namespace);
}