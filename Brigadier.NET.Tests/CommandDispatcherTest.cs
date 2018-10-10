using System;
using System.Collections.Generic;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

using static Brigadier.NET.Arguments;

namespace Brigadier.NET.Tests
{
	public class CommandDispatcherTest
	{
		private readonly CommandDispatcher<object> _subject;
		private readonly Command<object> _command;
		private readonly object _source = Substitute.For<object>();

		public CommandDispatcherTest()
		{
			_subject = new CommandDispatcher<object>();
			_command = Substitute.For<Command<object>>();
			_command.Invoke(Arg.Any<CommandContext<object>>()).Returns(42);
		}

		private static StringReader InputWithOffset(string input, int offset)
		{
			var result = new StringReader(input)
			{
				Cursor = offset
			};
			return result;
		}

        private class CommandSourceStack { }

		[Fact]
		public void TestReadMeExample()
		{
var dispatcher = new CommandDispatcher<CommandSourceStack>();

dispatcher.Register(r =>
	r.Literal("foo")
		.Then(c =>
			c.Argument("bar", Integer())
				.Executes(e => {
					Console.WriteLine("Bar is " + GetInteger(e, "bar"));
					return 1;
				})
		)
		.Executes(e => {
			Console.WriteLine("Called foo with no arguments");
			return 1;
		})
);
		}

		[Fact]
		public void TestCreateAndExecuteCommand()
		{
			_subject.Register(r => r.Literal("foo").Executes(_command));

			_subject.Execute("foo", _source).Should().Be(42);
			_command.Received().Invoke(Arg.Any<CommandContext<object>>());
		}


		[Fact]
		public void TestCreateAndExecuteOffsetCommand()
		{
			_subject.Register(r => r.Literal("foo").Executes(_command));

			_subject.Execute(InputWithOffset("/foo", 1), _source).Should().Be(42);
			_command.Received().Invoke(Arg.Any<CommandContext<object>>());
		}


		[Fact]
		public void TestCreateAndMergeCommands()
		{
			_subject.Register(r => r.Literal("base").Then(c => c.Literal("foo").Executes(_command)));
			_subject.Register(r => r.Literal("base").Then(c => c.Literal("bar").Executes(_command)));

			_subject.Execute("base foo", _source).Should().Be(42);
			_subject.Execute("base bar", _source).Should().Be(42);

			_command.Received(2).Invoke(Arg.Any<CommandContext<object>>());
		}

		[Fact]
		public void TestExecuteUnknownCommand()
		{
			_subject.Register(r => r.Literal("bar"));
			_subject.Register(r => r.Literal("baz"));

			_subject.Invoking(s => s.Execute("foo", _source)).Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestExecuteImpermissibleCommand()
		{
			_subject.Register(r => r.Literal("foo").Requires(s => false));

			_subject.Invoking(s => s.Execute("foo", _source)).Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestExecuteEmptyCommand()
		{
			_subject.Register(r => r.Literal(""));

			_subject.Invoking(s => s.Execute("", _source)).Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestExecuteUnknownSubCommand()
		{
			_subject.Register(r => r.Literal("foo").Executes(_command));

			_subject.Invoking(s => s.Execute("foo bar", _source)).Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument())
				.Where(ex => ex.Cursor == 4);
		}

		[Fact]
		public void TestExecuteIncorrectLiteral()
		{
			_subject.Register(r => r.Literal("foo").Executes(_command).Then(c => c.Literal("bar")));
			_subject.Invoking(s => s.Execute("foo baz", _source)).Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument())
				.Where(ex => ex.Cursor == 4);
		}

		[Fact]
		public void TestExecuteAmbiguousIncorrectArgument()
		{
			_subject.Register(r => 
				r.Literal("foo").Executes(_command)
					.Then(c => c.Literal("bar"))
					.Then(c => c.Literal("baz"))
			);
			_subject.Invoking(s => s.Execute("foo unknown", _source)).Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument())
				.Where(ex => ex.Cursor == 4);
		}


		[Fact]
		public void TestExecuteSubCommand()
		{
			var subCommand = Substitute.For<Command<object>>();
			subCommand.Invoke(Arg.Any<CommandContext<object>>()).Returns(100);

			_subject.Register(r => 
				r.Literal("foo")
					.Then(r.Literal("a"))
					.Then(r.Literal("=").Executes(subCommand))
					.Then(r.Literal("c"))
					.Executes(_command));

			_subject.Execute("foo =", _source).Should().Be(100);
			subCommand.Received().Invoke(Arg.Any<CommandContext<object>>());
		}


		[Fact]
		public void TestParseIncompleteLiteral()
		{
			_subject.Register(r => r.Literal("foo").Then(c => c.Literal("bar").Executes(_command)));

			var parse = _subject.Parse("foo ", _source);
			parse.Reader.Remaining.Should().BeEquivalentTo(" ");
			parse.Context.Nodes.Count.Should().Be(1);
		}


		[Fact]
		public void TestParseIncompleteArgument()
		{
			_subject.Register(r => r.Literal("foo").Then(c => c.Argument("bar", Integer()).Executes(_command)));

			var parse = _subject.Parse("foo ", _source);
			parse.Reader.Remaining.Should().BeEquivalentTo(" ");
			parse.Context.Nodes.Count.Should().Be(1);
		}

		[Fact]
		public void TestExecuteAmbiguousParentSubCommand()
		{
			var subCommand = Substitute.For<Command<object>>();
			subCommand.Invoke(Arg.Any<CommandContext<object>>()).Returns(100);

			_subject.Register(r => 
				r.Literal("test")
					.Then(
						c => c.Argument("incorrect", Integer()).Executes(_command)
					)
					.Then(
						c => c.Argument("right", Integer())
							.Then(
								c.Argument("sub", Integer()).Executes(subCommand)
							)
					)
			);

			_subject.Execute("test 1 2", _source).Should().Be(100);
			subCommand.Received().Invoke(Arg.Any<CommandContext<object>>());
			_command.DidNotReceive().Invoke(Arg.Any<CommandContext<object>>());
		}

		[Fact]
		public void TestExecuteAmbiguousParentSubCommandViaRedirect()
		{
			var subCommand = Substitute.For<Command<object>>();
			subCommand.Invoke(Arg.Any<CommandContext<object>>()).Returns(100);

			var real = _subject.Register(r => 
				r.Literal("test")
					.Then(c=>  
						c.Argument("incorrect", Integer())
							.Executes(_command)
					)
					.Then(c =>
						c.Argument("right", Integer())
							.Then(
								c.Argument("sub", Integer())
									.Executes(subCommand)
							)
					)
			);

			_subject.Register(r => r.Literal("redirect").Redirect(real));

			_subject.Execute("redirect 1 2", _source).Should().Be(100);
			subCommand.Received().Invoke(Arg.Any<CommandContext<object>>());
			_command.DidNotReceive().Invoke(Arg.Any<CommandContext<object>>());
		}


		[Fact]
		public void TestExecuteRedirectedMultipleTimes()
		{
			var concreteNode = _subject.Register(r => r.Literal("actual").Executes(_command));
			var redirectNode = _subject.Register(r => r.Literal("redirected").Redirect(_subject.GetRoot()));

			var input = "redirected redirected actual";

			var parse = _subject.Parse(input, _source);
			parse.Context.Range.Get(input).Should().BeEquivalentTo("redirected");
			parse.Context.Nodes.Count.Should().Be(1);
			parse.Context.RootNode.Should().Be(_subject.GetRoot());
			parse.Context.Nodes[0].Range.Should().BeEquivalentTo(parse.Context.Range);
			parse.Context.Nodes[0].Node.Should().Be(redirectNode);

			var child1 = parse.Context.Child;
			child1.Should().NotBeNull();
			child1.Range.Get(input).Should().BeEquivalentTo("redirected");
			child1.Nodes.Count.Should().Be(1);
			child1.RootNode.Should().Be(_subject.GetRoot());
			child1.Nodes[0].Range.Should().BeEquivalentTo(child1.Range);
			child1.Nodes[0].Node.Should().Be(redirectNode);

			var child2 = child1.Child;
			child2.Should().NotBeNull();
			child2.Range.Get(input).Should().BeEquivalentTo("actual");
			child2.Nodes.Count.Should().Be(1);
			child2.RootNode.Should().Be(_subject.GetRoot());
			child2.Nodes[0].Range.Should().BeEquivalentTo(child2.Range);
			child2.Nodes[0].Node.Should().Be(concreteNode);

			_subject.Execute(parse).Should().Be(42);
			_command.Received().Invoke(Arg.Any<CommandContext<object>>());
		}


		[Fact]
		public void TestExecuteRedirected()
		{
			var modifier = Substitute.For<RedirectModifier<object>>();
			var source1 = new object();
			var source2 = new object();
			modifier.Invoke(Arg.Is<CommandContext<object>>(s => s.Source == _source)).Returns(new[] {source1, source2});

			var concreteNode = _subject.Register(r => r.Literal("actual").Executes(_command));
			var redirectNode = _subject.Register(r => r.Literal("redirected").Fork(_subject.GetRoot(), modifier));

			var input = "redirected actual";
			var parse = _subject.Parse(input, _source);
			parse.Context.Range.Get(input).Should().BeEquivalentTo("redirected");
			parse.Context.Nodes.Count.Should().Be(1);
			parse.Context.RootNode.Should().BeEquivalentTo(_subject.GetRoot());
			parse.Context.Nodes[0].Range.Should().BeEquivalentTo(parse.Context.Range);
			parse.Context.Nodes[0].Node.Should().Be(redirectNode);
			parse.Context.Source.Should().Be(_source);

			var parent = parse.Context.Child;
			parent.Should().NotBeNull();
			parent.Range.Get(input).Should().BeEquivalentTo("actual");
			parent.Nodes.Count.Should().Be(1);
			parse.Context.RootNode.Should().BeEquivalentTo(_subject.GetRoot());
			parent.Nodes[0].Range.Should().BeEquivalentTo(parent.Range);
			parent.Nodes[0].Node.Should().Be(concreteNode);
			parent.Source.Should().Be(_source);

			_subject.Execute(parse).Should().Be(2);
			_command.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => c.Source == source1));
			_command.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => c.Source == source2));
		}

		[Fact]
		public void TestExecuteOrphanedSubCommand()
		{
			_subject.Register(r => r.Literal("foo").Then(c =>
				c.Argument("bar", Integer())
			).Executes(_command));

			_subject.Invoking(s => s.Execute("foo 5", _source)).Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
				.Where(ex => ex.Cursor == 5);
		}

		[Fact]
		public void testExecute_invalidOther()
		{
			var wrongCommand = Substitute.For<Command<object>>();
			_subject.Register(r => r.Literal("w").Executes(wrongCommand));
			_subject.Register(r => r.Literal("world").Executes(_command));

			_subject.Execute("world", _source).Should().Be(42);
			wrongCommand.DidNotReceive().Invoke(Arg.Any<CommandContext<object>>());
			_command.Received().Invoke(Arg.Any<CommandContext<object>>());
		}

		[Fact]
		public void parse_noSpaceSeparator()
		{
			_subject.Register(r => r.Literal("foo").Then(c => c.Argument("bar", Integer()).Executes(_command)));

			_subject.Invoking(s => s.Execute("foo$", _source))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestExecuteInvalidSubCommand()
		{
			_subject.Register(r => r.Literal("foo").Then(c => c.Argument("bar", Integer())).Executes(_command));

			_subject.Invoking(s => s.Execute("foo bar", _source))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.ReaderExpectedInt())
				.Where(ex => ex.Cursor == 4);
		}

		[Fact]
		public void TestGetPath()
		{
			var bar = LiteralArgumentBuilder<object>.LiteralArgument("bar").Build();
			_subject.Register(r => r.Literal("foo").Then(bar));

			_subject.GetPath(bar).Should().BeEquivalentTo(new List<string> { "foo", "bar" });
		}

		[Fact]
		public void TestFindNodeExists()
		{
			var bar = LiteralArgumentBuilder<object>.LiteralArgument("bar").Build();
			_subject.Register(r => r.Literal("foo").Then(bar));

			_subject.FindNode(new List<string> { "foo", "bar" }).Should().Be(bar);
		}

		[Fact]
		public void TestFindNodeDoesNotExist()
		{
			_subject.FindNode(new List<string> { "foo", "bar" }).Should().BeNull();
		}
	}
}