// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Horology;
using Brigadier.NET;
using Brigadier.NET.Builder;
using Brigadier.NET.Tree;

[MarkdownExporterAttribute.GitHub]
public class ParsingBenchmarks {
    private CommandDispatcher<object> subject;

    [GlobalSetup]
    public void setup() {
        subject = new CommandDispatcher<object>();
        subject.Register(r => 
            r.Literal("a")
                .Then(
                    r.Literal("1")
                        .Then(r.Literal("i").Executes(c => 0))
                        .Then(r.Literal("ii").Executes(c => 0))
                )
                .Then(
                    r.Literal("2")
                        .Then(r.Literal("i").Executes(c => 0))
                        .Then(r.Literal("ii").Executes(c => 0))
                )
        );
        subject.Register(r => r.Literal("b").Then(r.Literal("1").Executes(c => 0)));
        subject.Register(r => r.Literal("c").Executes(c => 0));
        subject.Register(r => r.Literal("d").Requires(s => false).Executes(c => 0));
        subject.Register(r => 
            r.Literal("e")
                .Executes(c => 0)
                .Then(
                    r.Literal("1")
                        .Executes(c => 0)
                        .Then(r.Literal("i").Executes(c => 0))
                        .Then(r.Literal("ii").Executes(c => 0))
                )
        );
        subject.Register(r => 
            r.Literal("f")
                .Then(
                    r.Literal("1")
                        .Then(r.Literal("i").Executes(c => 0))
                        .Then(r.Literal("ii").Executes(c => 0).Requires(s => false))
                )
                .Then(
                    r.Literal("2")
                        .Then(r.Literal("i").Executes(c => 0).Requires(s => false))
                        .Then(r.Literal("ii").Executes(c => 0))
                )
        );
        subject.Register(r => 
            r.Literal("g")
                .Executes(c => 0)
                .Then(r.Literal("1").Then(r.Literal("i").Executes(c => 0)))
        );
        LiteralCommandNode<Object> h = subject.Register(r => 
            r.Literal("h")
                .Executes(c => 0)
                .Then(r.Literal("1").Then(r.Literal("i").Executes(c => 0)))
                .Then(r.Literal("2").Then(r.Literal("i").Then(r.Literal("ii").Executes(c => 0))))
                .Then(r.Literal("3").Executes(c => 0))
        );
        subject.Register(r => 
            r.Literal("i")
                .Executes(c => 0)
                .Then(r.Literal("1").Executes(c => 0))
                .Then(r.Literal("2").Executes(c => 0))
        );
        subject.Register(r => 
            r.Literal("j")
                .Redirect(subject.GetRoot())
        );
        subject.Register(r => 
            r.Literal("k")
                .Redirect(h)
        );
    }

    [Benchmark]
    [MemoryDiagnoser]
	public void parse_a1i() {
        subject.Parse("a 1 i", new Object());
    }

    [Benchmark]
    [MemoryDiagnoser]
	public void parse_c() {
        subject.Parse("c", new Object());
    }

    [Benchmark]
    [MemoryDiagnoser]
	public void parse_k1i() {
        subject.Parse("k 1 i", new Object());
    }

    [Benchmark]
    [MemoryDiagnoser]
	public void parse_() {
        subject.Parse("c", new Object());
    }
}
