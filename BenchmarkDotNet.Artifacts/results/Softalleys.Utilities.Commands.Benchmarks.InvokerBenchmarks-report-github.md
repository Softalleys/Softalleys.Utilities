```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.4946)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.304
  [Host]     : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2


```
| Method                               | Mean     | Error   | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |---------:|--------:|---------:|------:|--------:|-------:|----------:|------------:|
| Reflection_Invoke_DefaultHandler     | 158.1 ns | 3.14 ns |  4.29 ns |  1.00 |    0.00 | 0.0563 |     472 B |        1.00 |
| CachedDelegate_Invoke_DefaultHandler | 117.2 ns | 3.54 ns | 10.26 ns |  0.81 |    0.06 | 0.0601 |     504 B |        1.07 |
