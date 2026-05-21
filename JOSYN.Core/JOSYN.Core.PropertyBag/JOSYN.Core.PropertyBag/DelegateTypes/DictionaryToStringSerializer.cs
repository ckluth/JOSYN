using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// 
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
public delegate Result<string> DictionaryToStringSerializer(Dictionary<string, string> data);