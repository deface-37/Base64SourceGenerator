namespace Base64SourceGenerator.Example;

public static partial class Base64Source
{
    
    [Base64("NewFile1.txt")]
    public static partial string GetData();
    
    [Base64("Шаблон_штампа_изменений.dwg")]
    public static partial string AutocadData();
}