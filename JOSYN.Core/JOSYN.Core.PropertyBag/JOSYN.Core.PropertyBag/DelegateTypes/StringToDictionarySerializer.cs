using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.PropertyBag;
#pragma warning restore IDE0130


public delegate Result<Dictionary<string, string>> StringToDictionarySerializer(string str);