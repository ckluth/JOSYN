namespace JOSYN.System.Frontend.JobHost;

public delegate bool ArgumentsComparer<in T>(T a, IEnumerable<T> b) where T : class;