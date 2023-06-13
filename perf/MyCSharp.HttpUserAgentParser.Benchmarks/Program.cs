// Copyright © myCSharp.de - all rights reserved

using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

// Needed for DeviceDetector.NET
// https://github.com/totpero/DeviceDetector.NET/issues/44
ManualConfig config = ManualConfig.Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.DisableOptimizationsValidator);

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, config);
