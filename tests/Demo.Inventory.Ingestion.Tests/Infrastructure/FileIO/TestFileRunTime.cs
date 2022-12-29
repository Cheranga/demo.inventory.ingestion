using System.Text;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.FileIO;

public readonly struct TestFileRunTime : IHaveFileOperations<TestFileRunTime>, HasCancel<TestFileRunTime>
{
    private readonly IDictionary<string, string> _files;
    private readonly RuntimeEnv _env;

    private TestFileRunTime(RuntimeEnv env, IDictionary<string, string> files)
    {
        _env = env;
        _files = files;
    }

    public Aff<TestFileRunTime, IFileOperations> FileOperations =>
        Prelude.Eff<TestFileRunTime, IFileOperations>(static rt => TestFileOperations.New(rt._files));

    public TestFileRunTime LocalCancel => New(new Dictionary<string, string>());
    public CancellationToken CancellationToken => _env.Token;
    public CancellationTokenSource CancellationTokenSource => _env.Source;

    public static TestFileRunTime New(IDictionary<string, string> files) =>
        new(new RuntimeEnv(new CancellationTokenSource(), Encoding.Default), files);
}