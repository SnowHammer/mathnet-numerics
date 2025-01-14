﻿using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.MKL;

namespace Benchmark.LinearAlgebra
{
    [Config(typeof(Config))]
    public class DenseMatrixProduct
    {
        class Config : ManualConfig
        {
            public Config()
            {
                AddJob(Job.Default.WithRuntime(ClrRuntime.Net461).WithPlatform(Platform.X64).WithJit(Jit.RyuJit));
                AddJob(Job.Default.WithRuntime(ClrRuntime.Net461).WithPlatform(Platform.X86).WithJit(Jit.LegacyJit));
#if NET7_0_OR_GREATER
                AddJob(Job.Default.WithRuntime(CoreRuntime.Core70).WithPlatform(Platform.X64).WithJit(Jit.RyuJit));
#endif
            }
        }

        public enum ProviderId
        {
            Managed,
            NativeMKLAutoHigh,
            NativeMKLAutoLow,
            NativeMKLAvx2High,
            NativeMKLAvx2Low
        }

        //BenchmarkRunner.Run<LinearAlgebra.DenseMatrixProduct>(config);
        //Benchmark(new DenseMatrixProduct(10, 100), 100, "10 - 100x100 iterations");
        //Benchmark(new DenseMatrixProduct(25, 100), 100, "25 - 100x100 iterations");
        //Benchmark(new DenseMatrixProduct(50, 10), 100, "50 - 100x10 iterations");
        //Benchmark(new DenseMatrixProduct(100, 10), 100, "100 - 100x10 iterations");
        //Benchmark(new DenseMatrixProduct(250, 1), 10, "250 - 10x1 iterations");
        //Benchmark(new DenseMatrixProduct(500,1), 10, "500 - 10x1 iterations");
        //Benchmark(new DenseMatrixProduct(1000,1), 2, "1000 - 2x1 iterations");

        [Params(8, 64, 128)]
        public int M { get; set; }

        [Params(8, 64, 128)]
        public int N { get; set; }

        [Params(ProviderId.Managed, ProviderId.NativeMKLAutoHigh, ProviderId.NativeMKLAvx2High)]
        public ProviderId Provider { get; set; }

        readonly Dictionary<string, Matrix<double>> _data = new Dictionary<string, Matrix<double>>();

        public DenseMatrixProduct()
        {
            foreach (var m in new[] { 8, 64, 128 })
            foreach (var n in new[] { 8, 64, 128 })
            {
                var key = Key(m, n);
                _data[key] = Matrix<double>.Build.Random(m, n);
            }
        }

        static string Key(int m, int n)
        {
            return $"{m}x{n}";
        }

        [GlobalSetup]
        public void Setup()
        {
            switch (Provider)
            {
                case ProviderId.Managed:
                    Control.UseManaged();
                    break;
                case ProviderId.NativeMKLAutoHigh:
                    MklControl.UseNativeMKL(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.High);
                    break;
                case ProviderId.NativeMKLAutoLow:
                    MklControl.UseNativeMKL(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.Low);
                    break;
                case ProviderId.NativeMKLAvx2High:
                    MklControl.UseNativeMKL(MklConsistency.AVX2, MklPrecision.Double, MklAccuracy.High);
                    break;
                case ProviderId.NativeMKLAvx2Low:
                    MklControl.UseNativeMKL(MklConsistency.AVX2, MklPrecision.Double, MklAccuracy.Low);
                    break;
            }
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public Matrix<double> Product()
        {
            if (M != N)
            {
                return _data[Key(M, N)].TransposeAndMultiply(_data[Key(M, N)]);
            }

            return _data[Key(M, N)]*_data[Key(M, N)];
        }
    }
}
