using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.context;

public class ContextChainTest
{
	[Fact]
	public void ExecuteAllForSingleCommand()
	{
		var consumer = Substitute.For<ResultConsumer<object>>();
		var command = Substitute.For<Command<object>>();

		command.Invoke(Arg.Any<CommandContext<object>>()).Returns(4);
		
		var dispatcher = new CommandDispatcher<object>();
		dispatcher.Register(l => l.Literal("foo").Executes(command));
		var source = "compile_source";

		var parse = dispatcher.Parse("foo", source);
		var topContext = parse.Context.Build("foo");
		topContext.TryFlatten(out var chain).Should().BeTrue();

		var runtimeSource = "runtime_source";
		chain!.ExecuteAll(runtimeSource, consumer).Should().Be(4);

		command.Received().Invoke(Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, runtimeSource)));
		consumer.Received().Invoke(
			Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, runtimeSource)),
			true,
			4
		);
		//verifyNoMoreInteractions(consumer);
	}

	[Fact]
	public void ExecuteAllForRedirectedCommand()
	{
		var consumer = Substitute.For<ResultConsumer<object>>();
		var command = Substitute.For<Command<object>>();

		command.Invoke(Arg.Any<CommandContext<object>>()).Returns(4);
		
		var redirectedSource = "redirected_source";

		var dispatcher = new CommandDispatcher<object>();
		dispatcher.Register(l => l.Literal("foo").Executes(command));
		dispatcher.Register(l => l.Literal("bar").Redirect(dispatcher.Root, _ => redirectedSource ));
		var source = "compile_source";

		var parse = dispatcher.Parse("bar foo", source);
		var topContext = parse.Context.Build("bar foo");
		topContext.TryFlatten(out var chain).Should().BeTrue();

		var runtimeSource = "runtime_source";
		chain!.ExecuteAll(runtimeSource, consumer).Should().Be(4);

		command.Received().Invoke(Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, redirectedSource)));
		consumer.Received().Invoke(
			Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, redirectedSource)),
			true,
			4
		);
		//verifyNoMoreInteractions(consumer);
	}

	[Fact]
	public void SingleStageExecution()
	{
		var dispatcher = new CommandDispatcher<object>();
		dispatcher.Register(l => l.Literal("foo").Executes(_ => 1));
		var source = new object();

		var result = dispatcher.Parse("foo", source);
		var topContext = result.Context.Build("foo");
		topContext.TryFlatten(out var stage0).Should().BeTrue();

		stage0!.CurrentStage.Should().Be(ContextChain<object>.Stage.Execute);
		stage0.TopContext.Should().Be(topContext);
		stage0.NextStage().Should().BeNull();
	}

	[Fact]
	public void MultiStageExecution()
	{
		var dispatcher = new CommandDispatcher<object>();
		dispatcher.Register(l => l.Literal("foo").Executes(_ => 1));
		dispatcher.Register(l => l.Literal("bar").Redirect(dispatcher.Root));
		var source = new object();

		var result = dispatcher.Parse("bar bar foo", source);
		var topContext = result.Context.Build("bar bar foo");
		topContext.TryFlatten(out var stage0).Should().BeTrue();

		stage0!.CurrentStage.Should().Be(ContextChain<object>.Stage.Modify);
		stage0.TopContext.Should().Be(topContext);

		var stage1 = stage0.NextStage();
		stage1.Should().NotBeNull();
		stage1!.CurrentStage.Should().Be(ContextChain<object>.Stage.Modify);
		stage1.TopContext.Should().Be(topContext.Child);

		var stage2 = stage1.NextStage();
		stage2.Should().NotBeNull();
		stage2!.CurrentStage.Should().Be(ContextChain<object>.Stage.Execute);
		stage2.TopContext.Should().Be(topContext.Child!.Child);

		stage2.NextStage().Should().BeNull();
	}

	[Fact]
	public void MissingExecute()
	{
		var dispatcher = new CommandDispatcher<object>();
		dispatcher.Register(l => l.Literal("foo").Executes(_ => 1));
		dispatcher.Register(l => l.Literal("bar").Redirect(dispatcher.Root));

		var source = new object();
		var topContext = dispatcher.Parse("bar bar", source).Context.Build("bar bar");
		ContextChain<object>.TryFlatten(topContext, out var flattened).Should().BeFalse();
		flattened.Should().BeNull();
	}
}
