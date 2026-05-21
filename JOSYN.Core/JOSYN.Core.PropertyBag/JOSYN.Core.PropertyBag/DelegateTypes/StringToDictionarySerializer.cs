using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.PropertyBag;
#pragma warning restore IDE0130


/// <summary>
/// 
/// </summary>
/// <param name="str"></param>
/// <returns></returns>
public delegate Result<Dictionary<string, string>> StringToDictionarySerializer(string str);