// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Brigadier.NET.Builder;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests;

public class CommandDispatcherUsagesTest
{
	private CommandDispatcher<object> _subject;

	private readonly object _source = Substitute.For<object>();
	private readonly Command<object> _command = Substitute.For<Command<object>>();

	public CommandDispatcherUsagesTest()
	{
		_subject = new CommandDispatcher<object>();

		_subject.Register(
			r => r.Literal("a")
				.Then(
					r.Literal("1")
						.Then(r.Literal("i").Executes(_command))
						.Then(r.Literal("ii").Executes(_command))
				)
				.Then(
					r.Literal("2")
						.Then(r.Literal("i").Executes(_command))
						.Then(r.Literal("ii").Executes(_command))
				)
		);
		_subject.Register(r => r.Literal("b").Then(r.Literal("1").Executes(_command)));
		_subject.Register(r => r.Literal("c").Executes(_command));
		_subject.Register(r => r.Literal("d").Requires(s => false).Executes(_command));
		_subject.Register(r => r.Literal("e")
			.Executes(_command)
			.Then(
				r.Literal("1")
					.Executes(_command)
					.Then(r.Literal("i").Executes(_command))
					.Then(r.Literal("ii").Executes(_command))
			)
		);
		_subject.Register(r => 
			r.Literal("f")
				.Then(
					r.Literal("1")
						.Then(r.Literal("i").Executes(_command))
						.Then(r.Literal("ii").Executes(_command).Requires(s => false))
				)
				.Then(
					r.Literal("2")
						.Then(r.Literal("i").Executes(_command).Requires(s => false))
						.Then(r.Literal("ii").Executes(_command))
				)
		);
		_subject.Register(r => 
			r.Literal("g")
				.Executes(_command)
				.Then(r.Literal("1").Then(r.Literal("i").Executes(_command)))
		);
		_subject.Register(r => 
			r.Literal("h")
				.Executes(_command)
				.Then(r.Literal("1").Then(r.Literal("i").Executes(_command)))
				.Then(r.Literal("2").Then(r.Literal("i").Then(r.Literal("ii").Executes(_command))))
				.Then(r.Literal("3").Executes(_command))
		);
		_subject.Register(r =>
			r.Literal("i")
				.Executes(_command)
				.Then(r.Literal("1").Executes(_command))
				.Then(r.Literal("2").Executes(_command))
		);
		_subject.Register(r =>
			r.Literal("j")
				.Redirect(_subject.Root)
		);
		_subject.Register(r =>
			r.Literal("k")
				.Redirect(Get("h"))
		);
	}

	private CommandNode<object> Get(string command)
	{
		return _subject.Parse(command, _source).Context.Nodes.Last().Node;
	}

	private CommandNode<object> Get(StringReader command)
	{
		return _subject.Parse(command, _source).Context.Nodes.Last().Node;
	}

	[Fact]
	public void testAllUsage_noCommands()
	{
		_subject = new CommandDispatcher<object>();
		var results = _subject.GetAllUsage(_subject.Root, _source, true);
		results.Should().BeEmpty();
	}

	[Fact]
	public void testSmartUsage_noCommands()
	{
		_subject = new CommandDispatcher<object>();
		var results = _subject.GetSmartUsage(_subject.Root, _source);
		results.Should().BeEmpty();
	}

	[Fact]
	public void testAllUsage_root()
	{
		var results = _subject.GetAllUsage(_subject.Root, _source, true);
		results.Should().ContainInOrder(
			"a 1 i", 
			"a 1 ii", 
			"a 2 i", 
			"a 2 ii", 
			"b 1", 
			"c", 
			"e", 
			"e 1", 
			"e 1 i", 
			"e 1 ii", 
			"f 1 i", 
			"f 2 ii", 
			"g", 
			"g 1 i", 
			"h", 
			"h 1 i", 
			"h 2 i ii", 
			"h 3", 
			"i", 
			"i 1", 
			"i 2", 
			"j ...", 
			"k -> h"
		);
	}

	[Fact]
	public void testSmartUsage_root()
	{
		var results = _subject.GetSmartUsage(_subject.Root, _source);
		results.Should().Contain(new Dictionary<CommandNode<object>, string>
		{
			{Get("a"), "a (1|2)"},
			{Get("b"), "b 1"},
			{Get("c"), "c"},
			{Get("e"), "e [1]"},
			{Get("f"), "f (1|2)"},
			{Get("g"), "g [1]"},
			{Get("h"), "h [1|2|3]"},
			{Get("i"), "i [1|2]"},
			{Get("j"), "j ..."},
			{Get("k"), "k -> h"}
		});
	}

	[Fact]
	public void testSmartUsage_h()
	{
		var results = _subject.GetSmartUsage(Get("h"), _source);
		results.Should().Contain(new Dictionary<CommandNode<object>, string>
		{
			{Get("h 1"), "[1] i"},
			{Get("h 2"), "[2] i ii"},
			{Get("h 3"), "[3]"}
		});
	}

	[Fact]
	public void testSmartUsage_offsetH()
	{
		var offsetH = new StringReader("/|/|/h")
		{
			Cursor = 5
		};

		var results = _subject.GetSmartUsage(Get(offsetH), _source);
		results.Should().Contain(new Dictionary<CommandNode<object>, string>
		{
			{Get("h 1"), "[1] i"},
			{Get("h 2"), "[2] i ii"},
			{Get("h 3"), "[3]"}
		});
	}
}