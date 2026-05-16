using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.PropertyBag;
#pragma warning restore IDE0130

public delegate Result<string> DictionaryToStringSerializer(Dictionary<string, string> data);