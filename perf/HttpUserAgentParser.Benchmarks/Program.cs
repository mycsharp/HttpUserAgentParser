// Copyright © https://myCSharp.de - all rights reserved

using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

// Needed for DeviceDetector.NET
// https://github.com/totpero/DeviceDetector.NET/issues/44
ManualConfig config = ManualConfig.Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.DisableOptimizationsValidator);

// dotnet run -c Release --framework net80 net90 --runtimes net90
BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, config);
