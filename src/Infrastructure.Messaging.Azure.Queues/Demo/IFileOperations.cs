using System.Text;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

// public interface IFileOperations
// {
//      string ReadAllText(string fileName);
//     Unit WriteAllText(string fileName, string content);
// }
//
// public struct LiveFileOperations : IFileOperations
// {
//     public static readonly IFileOperations Default = new LiveFileOperations();
//
//     public string ReadAllText(string fileName) => File.ReadAllText(fileName);
//
//     public Unit WriteAllText(string fileName, string content)
//     {
//         File.WriteAllText(fileName, content);
//         return unit;
//     }
// }
//
// public interface IHaveFileOperations<RT> where RT : struct, IHaveFileOperations<RT>
// {
//     Eff<RT, IFileOperations> FileOperations { get; }
// }
//
// public struct LiveRunTime : IHaveFileOperations<LiveRunTime>
// {
//     public Eff<LiveRunTime, IFileOperations> FileOperations =>
//         SuccessEff(LiveFileOperations.Default);
// }
//
// public static class FileOperationsSchema<RT> where RT : struct, IHaveFileOperations<RT>
// {
//     public static Eff<RT, string> ReadAllText(string path) =>
//         default(RT).FileOperations.Map(rt => rt.ReadAllText(path));
//
//     public static Eff<RT, Unit> WriteAllText(string path, string text) =>
//         default(RT).FileOperations.Map(rt => rt.WriteAllText(path, text));
// }
//
// public static class SomeClass
// {
//     public static Eff<RT, Unit> CopyFile<RT>(string source, string destination)
//         where RT : struct, IHaveFileOperations<RT> =>
//         from text in FileOperationsSchema<RT>.ReadAllText(source)
//         from op in FileOperationsSchema<RT>.WriteAllText(destination, text)
//         select op;
// }

public interface IFileOperations
{
    Aff<string> ReadAllText(string filePath);
    // Unit ExecuteAsync(string command);
}

public struct LiveFileOperations : IFileOperations
{
    public static readonly IFileOperations Default = new LiveFileOperations();

    public Aff<string> ReadAllText(string filePath) =>
        AffMaybe<string>(async () => await File.ReadAllTextAsync(filePath));
}

public class RuntimeEnv
{
    public readonly CancellationTokenSource Source;
    public readonly CancellationToken Token;
    public readonly Encoding Encoding;

    public RuntimeEnv(CancellationTokenSource source, CancellationToken token, Encoding encoding)
    {
        Source = source;
        Token = token;
        Encoding = encoding;
    }

    public RuntimeEnv(CancellationTokenSource source, Encoding encoding)
        : this(source, source.Token, encoding) { }
}

public interface IHaveFileOperations<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, IFileOperations> FileOperations { get; }
}

public readonly struct LiveRunTime : IHaveFileOperations<LiveRunTime>, HasCancel<LiveRunTime>
{
    private readonly RuntimeEnv _env;

    LiveRunTime(RuntimeEnv env) => _env = env;

    public LiveRunTime LocalCancel =>
        new(new RuntimeEnv(new CancellationTokenSource(), Encoding.Default));
    public CancellationToken CancellationToken => _env.Token;
    public CancellationTokenSource CancellationTokenSource => _env.Source;

    public Aff<LiveRunTime, IFileOperations> FileOperations =>
        SuccessAff(LiveFileOperations.Default);
}

public static class FileOperationsSchema<RT>
    where RT : struct, IHaveFileOperations<RT>, HasCancel<RT>
{
    public static Aff<RT, Aff<string>> ReadAllText(string path) =>
        default(RT).FileOperations.Map(operations => operations.ReadAllText(path));
}
