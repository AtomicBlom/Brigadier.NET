// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Brigadier.NET.Context;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.context
{
	public class CommandContextTest {
		private readonly CommandContextBuilder<object> _builder;
		private readonly object _source = Substitute.For<object>();
		private readonly CommandDispatcher<object> _dispatcher = Substitute.For<CommandDispatcher<object>>();
		private readonly CommandNode<object> _rootNode = Substitute.For<CommandNode<object>>(null, null, null, null, false);

		public CommandContextTest()
		{
			_builder = new CommandContextBuilder<object>(_dispatcher, _source, _rootNode, 0);
		}

		[Fact]
		public void testGetArgument_nonexistent(){
			_builder.Build("").Invoking(b => b.GetArgument<object>("foo"))
				.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void testGetArgument_wrongType(){
			var context = _builder.WithArgument("foo", new ParsedArgument<object, int>(0, 1, 123)).Build("123");
			context.Invoking(c => c.GetArgument<string>("foo"))
				.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void TestGetArgument(){
			var context = _builder.WithArgument("foo", new ParsedArgument<object, int>(0, 1, 123)).Build("123");
			context.GetArgument<int>("foo").Should().Be(123);
		}

		[Fact]
		public void TestSource(){
			_builder.Build("").Source.Should().Be(_source);
		}

		[Fact]
		public void TestRootNode(){
			_builder.Build("").RootNode.Should().Be(_rootNode);
		}

		[Fact]
		public void TestEquals(){
			var otherSource = new object();
			var command = Substitute.For<Command<object>>();
			var otherCommand = Substitute.For<Command<object>>();
			var rootNode = Substitute.For<CommandNode<object>>(null, null, null, null, false);
			var otherRootNode = Substitute.For<CommandNode<object>>(null, null, null, null, false);
			var node = Substitute.For<CommandNode<object>>(null, null, null, null, false);
			var otherNode = Substitute.For<CommandNode<object>>(null, null, null, null, false);
			new EqualsTester()
				.AddEqualityGroup(new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).Build(""), new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).Build(""))
				.AddEqualityGroup(new CommandContextBuilder<object>(_dispatcher, _source, otherRootNode, 0).Build(""), new CommandContextBuilder<object>(_dispatcher, _source, otherRootNode, 0).Build(""))
				.AddEqualityGroup(new CommandContextBuilder<object>(_dispatcher, otherSource, rootNode, 0).Build(""), new CommandContextBuilder<object>(_dispatcher, otherSource, rootNode, 0).Build(""))
				.AddEqualityGroup(new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithCommand(command).Build(""), new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithCommand(command).Build(""))
				.AddEqualityGroup(new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithCommand(otherCommand).Build(""), new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithCommand(otherCommand).Build(""))
				.AddEqualityGroup(new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithArgument("foo", new ParsedArgument<object, int>(0, 1, 123)).Build("123"), new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithArgument("foo", new ParsedArgument<object, int>(0, 1, 123)).Build("123"))
				.AddEqualityGroup(new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithNode(node, StringRange.Between(0, 3)).WithNode(otherNode, StringRange.Between(4, 6)).Build("123 456"), new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithNode(node, StringRange.Between(0, 3)).WithNode(otherNode, StringRange.Between(4, 6)).Build("123 456"))
				.AddEqualityGroup(new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithNode(otherNode, StringRange.Between(0, 3)).WithNode(node, StringRange.Between(4, 6)).Build("123 456"), new CommandContextBuilder<object>(_dispatcher, _source, rootNode, 0).WithNode(otherNode, StringRange.Between(0, 3)).WithNode(node, StringRange.Between(4, 6)).Build("123 456"))
				.TestEquals();
		}
	}
}