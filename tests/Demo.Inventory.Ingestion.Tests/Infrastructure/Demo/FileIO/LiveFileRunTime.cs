using System.Text;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Demo.FileIO;

public readonly struct LiveFileRunTime : IHaveFileOperations<LiveFileRunTime>, HasCancel<LiveFileRunTime>
{
    private readonly RuntimeEnv _env;

    private LiveFileRunTime(RuntimeEnv env) => _env = env;

    public LiveFileRunTime LocalCancel =>
        new(new RuntimeEnv(new CancellationTokenSource(), Encoding.Default));

    public CancellationToken CancellationToken => _env.Token;

    public CancellationTokenSource CancellationTokenSource => _env.Source;

    public Aff<LiveFileRunTime, IFileOperations> FileOperations =>
        SuccessAff(LiveFileOperations.Default);

    public static LiveFileRunTime New() =>
        new(new RuntimeEnv(new CancellationTokenSource(), Encoding.Default));
}
