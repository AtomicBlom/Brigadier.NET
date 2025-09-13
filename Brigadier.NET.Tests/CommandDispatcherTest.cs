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

		/*

    @Test
    public void testCorrectExecuteContextAfterRedirect() throws Exception {
        final CommandDispatcher<Integer> subject = new CommandDispatcher<>();

        final RootCommandNode<Integer> root = subject.getRoot();
        final LiteralArgumentBuilder<Integer> add = literal("add");
        final LiteralArgumentBuilder<Integer> blank = literal("blank");
        final RequiredArgumentBuilder<Integer, Integer> addArg = argument("value", integer());
        final LiteralArgumentBuilder<Integer> run = literal("run");

        subject.register(add.then(addArg.redirect(root, c -> c.getSource() + getInteger(c, "value"))));
        subject.register(blank.redirect(root));
        subject.register(run.executes(CommandContext::getSource));

        assertThat(subject.execute("run", 0), is(0));
        assertThat(subject.execute("run", 1), is(1));

        assertThat(subject.execute("add 5 run", 1), is(1 + 5));
        assertThat(subject.execute("add 5 add 6 run", 2), is(2 + 5 + 6));
        assertThat(subject.execute("add 5 blank run", 1), is(1 + 5));
        assertThat(subject.execute("blank add 5 run", 1), is(1 + 5));
        assertThat(subject.execute("add 5 blank add 6 run", 2), is(2 + 5 + 6));
        assertThat(subject.execute("add 5 blank blank add 6 run", 2), is(2 + 5 + 6));
    }

    @Test
    public void testSharedRedirectAndExecuteNodes() throws CommandSyntaxException {
        final CommandDispatcher<Integer> subject = new CommandDispatcher<>();

        final RootCommandNode<Integer> root = subject.getRoot();
        final LiteralArgumentBuilder<Integer> add = literal("add");
        final RequiredArgumentBuilder<Integer, Integer> addArg = argument("value", integer());

        subject.register(add.then(
            addArg
                .redirect(root, c -> c.getSource() + getInteger(c, "value"))
                .executes(CommandContext::getSource)
        ));

        assertThat(subject.execute("add 5", 1), is(1));
        assertThat(subject.execute("add 5 add 6", 1), is(1 + 5));
    }
		 */

		[Fact]
		public void TestExecuteRedirected()
		{
			var modifier = Substitute.For<RedirectModifier<object>>();
			var source1 = new object();
			var source2 = new object();
			modifier.Invoke(Arg.Is<CommandContext<object>>(s => s.Source == _source)).Returns([source1, source2]);

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

		/*
    @Test
    public void testIncompleteRedirectShouldThrow() {
        final LiteralCommandNode<Object> foo = subject.register(literal("foo")
            .then(literal("bar")
                .then(argument("value", integer()).executes(context -> IntegerArgumentType.getInteger(context, "value"))))
            .then(literal("awa").executes(context -> 2)));
        subject.register(literal("baz").redirect(foo));
        try {
            subject.execute("baz bar", source);
            fail("Should have thrown an exception");
        } catch (CommandSyntaxException e) {
            assertThat(e.getType(), is(CommandSyntaxException.BUILT_IN_EXCEPTIONS.dispatcherUnknownCommand()));
        }
    }

    @Test
    public void testRedirectModifierEmptyResult() throws CommandSyntaxException {
        final LiteralCommandNode<Object> foo = subject.register(literal("foo")
            .then(literal("bar")
                .then(argument("value", integer()).executes(context -> IntegerArgumentType.getInteger(context, "value"))))
            .then(literal("awa").executes(context -> 2)));
        final RedirectModifier<Object> emptyModifier = context -> Collections.emptyList();
        subject.register(literal("baz").fork(foo, emptyModifier));
        int result = subject.execute("baz bar 100", source);
        assertThat(result, is(0)); // No commands executed, so result is 0
    }
		 */
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

		/*

    @Test
    public void testResultConsumerInNonErrorRun() throws CommandSyntaxException {
        subject.setConsumer(consumer);

        subject.register(literal("foo").executes(command));
        when(command.run(any())).thenReturn(5);

        assertThat(subject.execute("foo", source), is(5));
        verify(consumer).onCommandComplete(any(), eq(true), eq(5));
        verifyNoMoreInteractions(consumer);
    }

    @Test
    public void testResultConsumerInForkedNonErrorRun() throws CommandSyntaxException {
        subject.setConsumer(consumer);

        subject.register(literal("foo").executes(c -> (Integer)(c.getSource())));
        final Object[] contexts = new Object[] {9, 10, 11};

        subject.register(literal("repeat").fork(subject.getRoot(), context -> Arrays.asList(contexts)));

        assertThat(subject.execute("repeat foo", source), is(contexts.length));
        verify(consumer).onCommandComplete(argThat(contextSourceMatches(contexts[0])), eq(true), eq(9));
        verify(consumer).onCommandComplete(argThat(contextSourceMatches(contexts[1])), eq(true), eq(10));
        verify(consumer).onCommandComplete(argThat(contextSourceMatches(contexts[2])), eq(true), eq(11));
        verifyNoMoreInteractions(consumer);
    }

    @Test
    public void testExceptionInNonForkedCommand() throws CommandSyntaxException {
        subject.setConsumer(consumer);
        subject.register(literal("crash").executes(command));
        final CommandSyntaxException exception = CommandSyntaxException.BUILT_IN_EXCEPTIONS.readerExpectedBool().create();
        when(command.run(any())).thenThrow(exception);

        try {
            subject.execute("crash", source);
            fail();
        } catch (final CommandSyntaxException ex) {
            assertThat(ex, is(exception));
        }

        verify(consumer).onCommandComplete(any(), eq(false), eq(0));
        verifyNoMoreInteractions(consumer);
    }

    @Test
    public void testExceptionInNonForkedRedirectedCommand() throws CommandSyntaxException {
        subject.setConsumer(consumer);
        subject.register(literal("crash").executes(command));
        subject.register(literal("redirect").redirect(subject.getRoot()));

        final CommandSyntaxException exception = CommandSyntaxException.BUILT_IN_EXCEPTIONS.readerExpectedBool().create();
        when(command.run(any())).thenThrow(exception);

        try {
            subject.execute("redirect crash", source);
            fail();
        } catch (final CommandSyntaxException ex) {
            assertThat(ex, is(exception));
        }

        verify(consumer).onCommandComplete(any(), eq(false), eq(0));
        verifyNoMoreInteractions(consumer);
    }

    @Test
    public void testExceptionInForkedRedirectedCommand() throws CommandSyntaxException {
        subject.setConsumer(consumer);
        subject.register(literal("crash").executes(command));
        subject.register(literal("redirect").fork(subject.getRoot(), Collections::singleton));

        final CommandSyntaxException exception = CommandSyntaxException.BUILT_IN_EXCEPTIONS.readerExpectedBool().create();
        when(command.run(any())).thenThrow(exception);

        assertThat(subject.execute("redirect crash", source), is(0));
        verify(consumer).onCommandComplete(any(), eq(false), eq(0));
        verifyNoMoreInteractions(consumer);
    }

    @Test
    public void testExceptionInNonForkedRedirect() throws CommandSyntaxException {
        final CommandSyntaxException exception = CommandSyntaxException.BUILT_IN_EXCEPTIONS.readerExpectedBool().create();

        subject.setConsumer(consumer);
        subject.register(literal("noop").executes(command));
        subject.register(literal("redirect").redirect(subject.getRoot(), context -> {
            throw exception;
        }));

        when(command.run(any())).thenReturn(3);

        try {
            subject.execute("redirect noop", source);
            fail();
        } catch (final CommandSyntaxException ex) {
            assertThat(ex, is(exception));
        }

        verifyZeroInteractions(command);
        verify(consumer).onCommandComplete(any(), eq(false), eq(0));
        verifyNoMoreInteractions(consumer);
    }

    @Test
    public void testExceptionInForkedRedirect() throws CommandSyntaxException {
        final CommandSyntaxException exception = CommandSyntaxException.BUILT_IN_EXCEPTIONS.readerExpectedBool().create();

        subject.setConsumer(consumer);
        subject.register(literal("noop").executes(command));
        subject.register(literal("redirect").fork(subject.getRoot(), context -> {
            throw exception;
        }));

        when(command.run(any())).thenReturn(3);


        assertThat(subject.execute("redirect noop", source), is(0));

        verifyZeroInteractions(command);
        verify(consumer).onCommandComplete(any(), eq(false), eq(0));
        verifyNoMoreInteractions(consumer);
    }

    @Test
    public void testPartialExceptionInForkedRedirect() throws CommandSyntaxException {
        final CommandSyntaxException exception = CommandSyntaxException.BUILT_IN_EXCEPTIONS.readerExpectedBool().create();
        final Object otherSource = new Object();
        final Object rejectedSource = new Object();

        subject.setConsumer(consumer);
        subject.register(literal("run").executes(command));
        subject.register(literal("split").fork(subject.getRoot(), context -> Arrays.asList(source, rejectedSource, otherSource)));
        subject.register(literal("filter").fork(subject.getRoot(), context -> {
            final Object currentSource = context.getSource();
            if (currentSource == rejectedSource) {
                throw exception;
            }
            return Collections.singleton(currentSource);
        }));

        when(command.run(any())).thenReturn(3);

        assertThat(subject.execute("split filter run", source), is(2));

        verify(command).run(argThat(contextSourceMatches(source)));
        verify(command).run(argThat(contextSourceMatches(otherSource)));
        verifyNoMoreInteractions(command);

        verify(consumer).onCommandComplete(argThat(contextSourceMatches(rejectedSource)), eq(false), eq(0));
        verify(consumer).onCommandComplete(argThat(contextSourceMatches(source)), eq(true), eq(3));
        verify(consumer).onCommandComplete(argThat(contextSourceMatches(otherSource)), eq(true), eq(3));
        verifyNoMoreInteractions(consumer);
    }

    public static Matcher<CommandContext<Object>> contextSourceMatches(final Object source) {
        return new CustomMatcher<CommandContext<Object>>("source " + source) {
            @Override
            public boolean matches(Object object) {
                return (object instanceof CommandContext) && ((CommandContext<?>) object).getSource() == source;
            }
        };
    }
		 */
	}
}