using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly ResultConsumer<object> _consumer = Substitute.For<ResultConsumer<object>>();

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
			_subject.Register(r => r.Literal("base").Then(r.Literal("foo").Executes(_command)));
			_subject.Register(r => r.Literal("base").Then(r.Literal("bar").Executes(_command)));

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
			_subject.Register(r => r.Literal("foo").Executes(_command).Then(r.Literal("bar")));
			_subject.Invoking(s => s.Execute("foo baz", _source)).Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument())
				.Where(ex => ex.Cursor == 4);
		}

		[Fact]
		public void TestExecuteAmbiguousIncorrectArgument()
		{
			_subject.Register(r => 
				r.Literal("foo").Executes(_command)
					.Then(r.Literal("bar"))
					.Then(r.Literal("baz"))
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
			_subject.Register(r => r.Literal("foo").Then(r.Literal("bar").Executes(_command)));

			var parse = _subject.Parse("foo ", _source);
			parse.Reader.Remaining.Should().BeEquivalentTo(" ");
			parse.Context.Nodes.Count.Should().Be(1);
		}


		[Fact]
		public void TestParseIncompleteArgument()
		{
			_subject.Register(r => r.Literal("foo").Then(r.Argument("bar", Integer()).Executes(_command)));

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
						r.Argument("incorrect", Integer()).Executes(_command)
					)
					.Then(
						r.Argument("right", Integer())
							.Then(
								r.Argument("sub", Integer()).Executes(subCommand)
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
					.Then(
						r.Argument("incorrect", Integer())
							.Executes(_command)
					)
					.Then(
						r.Argument("right", Integer())
							.Then(
								r.Argument("sub", Integer())
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
			var redirectNode = _subject.Register(r => r.Literal("redirected").Redirect(_subject.Root));

			var input = "redirected redirected actual";

			var parse = _subject.Parse(input, _source);
			parse.Context.Range.Get(input).Should().BeEquivalentTo("redirected");
			parse.Context.Nodes.Count.Should().Be(1);
			parse.Context.RootNode.Should().Be(_subject.Root);
			parse.Context.Nodes[0].Range.Should().BeEquivalentTo(parse.Context.Range);
			parse.Context.Nodes[0].Node.Should().Be(redirectNode);

			var child1 = parse.Context.Child;
			child1.Should().NotBeNull();
			child1.Range.Get(input).Should().BeEquivalentTo("redirected");
			child1.Nodes.Count.Should().Be(1);
			child1.RootNode.Should().Be(_subject.Root);
			child1.Nodes[0].Range.Should().BeEquivalentTo(child1.Range);
			child1.Nodes[0].Node.Should().Be(redirectNode);

			var child2 = child1.Child;
			child2.Should().NotBeNull();
			child2.Range.Get(input).Should().BeEquivalentTo("actual");
			child2.Nodes.Count.Should().Be(1);
			child2.RootNode.Should().Be(_subject.Root);
			child2.Nodes[0].Range.Should().BeEquivalentTo(child2.Range);
			child2.Nodes[0].Node.Should().Be(concreteNode);

			_subject.Execute(parse).Should().Be(42);
			_command.Received().Invoke(Arg.Any<CommandContext<object>>());
		}

		[Fact]
		public void TestCorrectExecuteContextAfterRedirect()
		{
			var subject = new CommandDispatcher<int>();

			var root = subject.Root;

			var add = LiteralArgumentBuilder<int>.LiteralArgument("add");
			var blank = LiteralArgumentBuilder<int>.LiteralArgument("blank");
			var addArg = RequiredArgumentBuilder<int, int>.RequiredArgument("value", Integer());
			var run = LiteralArgumentBuilder<int>.LiteralArgument("run");


			subject.Register(add.Then(addArg.Redirect(root, c => c.Source + c.GetArgument<int>("value"))));
			subject.Register(blank.Redirect(root));
			subject.Register(run.Executes(c => c.Source));

			subject.Execute("run", 0).Should().Be(0);
			subject.Execute("run", 1).Should().Be(1);

			subject.Execute("add 5 run", 1).Should().Be(1 + 5);
			subject.Execute("add 5 add 6 run", 2).Should().Be(2 + 5 + 6);
			subject.Execute("add 5 blank run", 1).Should().Be(1 + 5);
			subject.Execute("blank add 5 run", 1).Should().Be(1 + 5);
			subject.Execute("add 5 blank add 6 run", 2).Should().Be(2 + 5 + 6);
			subject.Execute("add 5 blank blank add 6 run", 2).Should().Be(2 + 5 + 6);
		}

		[Fact]
		public void TestSharedRedirectAndExecuteNodes()
		{
			var subject = new CommandDispatcher<int>();
			
			var root = subject.Root;
			var add = LiteralArgumentBuilder<int>.LiteralArgument("add");
			var addArg = RequiredArgumentBuilder<int, int>.RequiredArgument("value", Integer());

			subject.Register(add.Then(
				addArg
					.Redirect(root, c => c.Source + c.GetArgument<int>("value"))
					.Executes(c => c.Source)
			));

			subject.Execute("add 5", 1).Should().Be(1); // executes node itself, no redirect
			subject.Execute("add 5 add 6", 1).Should().Be(1 + 5); // first redirect only
		}

		[Fact]
		public void TestExecuteRedirected()
		{
			var modifier = Substitute.For<RedirectModifier<object>>();
			var source1 = new object();
			var source2 = new object();
			modifier.Invoke(Arg.Is<CommandContext<object>>(s => s.Source == _source)).Returns([source1, source2]);

			var concreteNode = _subject.Register(r => r.Literal("actual").Executes(_command));
			var redirectNode = _subject.Register(r => r.Literal("redirected").Fork(_subject.Root, modifier));

			var input = "redirected actual";
			var parse = _subject.Parse(input, _source);
			parse.Context.Range.Get(input).Should().BeEquivalentTo("redirected");
			parse.Context.Nodes.Count.Should().Be(1);
			parse.Context.RootNode.Should().BeEquivalentTo(_subject.Root);
			parse.Context.Nodes[0].Range.Should().BeEquivalentTo(parse.Context.Range);
			parse.Context.Nodes[0].Node.Should().Be(redirectNode);
			parse.Context.Source.Should().Be(_source);

			var parent = parse.Context.Child;
			parent.Should().NotBeNull();
			parent.Range.Get(input).Should().BeEquivalentTo("actual");
			parent.Nodes.Count.Should().Be(1);
			parse.Context.RootNode.Should().BeEquivalentTo(_subject.Root);
			parent.Nodes[0].Range.Should().BeEquivalentTo(parent.Range);
			parent.Nodes[0].Node.Should().Be(concreteNode);
			parent.Source.Should().Be(_source);

			_subject.Execute(parse).Should().Be(2);
			_command.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => c.Source == source1));
			_command.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => c.Source == source2));
		}

		[Fact]
		public void TestIncompleteRedirectShouldThrow()
		{
			var foo = _subject.Register(r => r.Literal("foo")
				.Then(r.Literal("bar")
					.Then(r.Argument("value", Integer()).Executes(c => c.GetArgument<int>("value"))))
				.Then(r.Literal("awa").Executes(_ => 2))
			);
			_subject.Register(r => r.Literal("baz").Redirect(foo));

			_subject.Invoking(s => s.Execute("baz bar", new object()))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand());
		}

		[Fact]
		public void TestRedirectModifierEmptyResult()
		{
			var foo = _subject.Register(r => r.Literal("foo")
				.Then(r.Literal("bar")
					.Then(r.Argument("value", Integer()).Executes(c => c.GetArgument<int>("value"))))
				.Then(r.Literal("awa").Executes(_ => 2))
			);
			RedirectModifier<object> emptyModifier = _ => Array.Empty<object>();
			_subject.Register(r => r.Literal("baz").Fork(foo, emptyModifier));

			var result = _subject.Execute("baz bar 100", new object());
			result.Should().Be(0);
		}

		[Fact]
		public void TestExecuteOrphanedSubCommand()
		{
			_subject.Register(r => r.Literal("foo").Then(r.Argument("bar", Integer())).Executes(_command));

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
			_subject.Register(r => r.Literal("foo").Then(r.Argument("bar", Integer()).Executes(_command)));

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

		[Fact]
		public void TestResultConsumerInNonErrorRun()
		{
			_subject.Consumer = _consumer;
			
			_subject.Register(r => r.Literal("foo").Executes(_command));
			_command.Invoke(Arg.Any<CommandContext<object>>()).Returns(5);

			_subject.Execute("foo", new object()).Should().Be(5);
			_consumer.Received().Invoke(Arg.Any<CommandContext<object>>(), true, 5);
			//verifyNoMoreInteractions(consumer);
		}

		[Fact]
		public void TestResultConsumerInForkedNonErrorRun()
		{
			_subject.Consumer = _consumer;
			
			_subject.Register(r => r.Literal("foo").Executes(c => (int)c.Source));
			var contexts = new object[] { 9, 10, 11 };

			_subject.Register(r => r.Literal("repeat").Fork(_subject.Root, _ => contexts.ToList()));

			_subject.Execute("repeat foo", new object()).Should().Be(contexts.Length);
			_consumer.Received().Invoke(Arg.Is<CommandContext<object>>(c => (int)c.Source == 9), true, 9);
			_consumer.Received().Invoke(Arg.Is<CommandContext<object>>(c => (int)c.Source == 10), true, 10);
			_consumer.Received().Invoke(Arg.Is<CommandContext<object>>(c => (int)c.Source == 11), true, 11);
			//verifyNoMoreInteractions(consumer);
		}

		[Fact]
		public void TestExceptionInNonForkedCommand()
		{
			_subject.Consumer = _consumer;
			_subject.Register(r => r.Literal("crash").Executes(_command));

			var exception = CommandSyntaxException.BuiltInExceptions.ReaderExpectedBool().Create();
			_command.Invoke(Arg.Any<CommandContext<object>>()).Returns(_ => { throw exception; });
			

			_subject.Invoking(s => s.Execute("crash", _source))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ReferenceEquals(ex, exception));
			_consumer.Received().Invoke(Arg.Any<CommandContext<object>>(), false, 0);
			//verifyNoMoreInteractions(consumer);
		}

		[Fact]
		public void TestExceptionInNonForkedRedirectedCommand()
		{
			var subject = new CommandDispatcher<object>();
			var consumer = Substitute.For<ResultConsumer<object>>();
			subject.Consumer = consumer;
			var command = Substitute.For<Command<object>>();
			var exception = CommandSyntaxException.BuiltInExceptions.ReaderExpectedBool().Create();
			command.Invoke(Arg.Any<CommandContext<object>>()).Returns(_ => { throw exception; });
			subject.Register(r => r.Literal("crash").Executes(command));
			subject.Register(r => r.Literal("redirect").Redirect(subject.Root));

			subject.Invoking(s => s.Execute("redirect crash", new object()))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ReferenceEquals(ex, exception));
			consumer.Received().Invoke(Arg.Any<CommandContext<object>>(), false, 0);
		}

		[Fact]
		public void TestExceptionInForkedRedirectedCommand()
		{
			_subject.Consumer = _consumer;
			_subject.Register(r => r.Literal("crash").Executes(_command));
			_subject.Register(r => r.Literal("redirect").Fork(_subject.Root, _ => new List<object> { new() }));

			var exception = CommandSyntaxException.BuiltInExceptions.ReaderExpectedBool().Create();
			_command.Invoke(Arg.Any<CommandContext<object>>()).Returns(_ => throw exception);
			

			_subject.Execute("redirect crash", new object()).Should().Be(0); // fork swallows exceptions
			_consumer.Received().Invoke(Arg.Any<CommandContext<object>>(), false, 0);
			//verifyNoMoreInteractions(_consumer);
		}

		[Fact]
		public void TestExceptionInNonForkedRedirect()
		{
			var exception = CommandSyntaxException.BuiltInExceptions.ReaderExpectedBool().Create();

			_subject.Consumer = _consumer;
			_subject.Register(r => r.Literal("noop").Executes(_command));
			_subject.Register(r => r.Literal("redirect").Redirect(_subject.Root, _ => throw exception));
			
			_command.Invoke(Arg.Any<CommandContext<object>>()).Returns(3);

			_subject.Invoking(s => s.Execute("redirect noop", new object()))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ReferenceEquals(ex, exception));
			
			_command.DidNotReceive().Invoke(Arg.Any<CommandContext<object>>());
			_consumer.Received().Invoke(Arg.Any<CommandContext<object>>(), false, 0);
			//verifyNoMoreInteractions(_consumer);
		}

		[Fact]
		public void TestExceptionInForkedRedirect()
		{
			var exception = CommandSyntaxException.BuiltInExceptions.ReaderExpectedBool().Create();

			_subject.Consumer = _consumer;
			_subject.Register(r => r.Literal("noop").Executes(_command));
			_subject.Register(r => r.Literal("redirect").Fork(_subject.Root, _ => throw exception));
			_command.Invoke(Arg.Any<CommandContext<object>>()).Returns(3);

			_subject.Execute("redirect noop", new object()).Should().Be(0); // fork -> 0 successes
			_command.DidNotReceive().Invoke(Arg.Any<CommandContext<object>>());
			_consumer.Received().Invoke(Arg.Any<CommandContext<object>>(), false, 0);
			//verifyNoMoreInteractions(_consumer);
		}

		[Fact]
		public void TestPartialExceptionInForkedRedirect()
		{
			var exception = CommandSyntaxException.BuiltInExceptions.ReaderExpectedBool().Create();
			var otherSource = new object();
			var rejectedSource = new object();
			
			_subject.Consumer = _consumer;
			

			_subject.Register(r => r.Literal("run").Executes(_command));
			_subject.Register(r => r.Literal("split").Fork(_subject.Root, _ => new List<object> { _source, rejectedSource, otherSource }));
			_subject.Register(r => r.Literal("filter").Fork(_subject.Root, ctx =>
			{
				var current = ctx.Source;
				if (ReferenceEquals(current, rejectedSource))
				{
					throw exception;
				}
				return new List<object> { current };
			}));
			_command.Invoke(Arg.Any<CommandContext<object>>()).Returns(3);

			_subject.Execute("split filter run", _source).Should().Be(2);
			_command.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, _source)));
			_command.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, otherSource)));
			_command.DidNotReceive().Invoke(Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, rejectedSource)));

			_consumer.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, rejectedSource)), false, 0);
			_consumer.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, _source)), true, 3);
			_consumer.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => ReferenceEquals(c.Source, otherSource)), true, 3);
		}
	}
}