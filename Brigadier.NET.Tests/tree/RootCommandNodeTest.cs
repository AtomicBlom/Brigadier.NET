// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.tree
{
	public class RootCommandNodeTest : AbstractCommandNodeTest {
		private readonly RootCommandNode<object> _node;

		protected override CommandNode<object> GetCommandNode() {
			return _node;
		}

		public RootCommandNodeTest()
		{
			_node = new RootCommandNode<object>();
		}

		[Fact]
		public void TestParse(){
			var reader = new StringReader("hello world");
			_node.Parse(reader, new CommandContextBuilder<object>(new CommandDispatcher<object>(), new object(), new RootCommandNode<object>(), 0));
			reader.Cursor.Should().Be(0);
		}

		[Fact]
		public void TestAddChildNoRoot(){
			_node.Invoking(n => n.AddChild(new RootCommandNode<object>()))
				.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void TestUsage(){
			_node.UsageText.Should().Be("");
		}

		[Fact]
		public async Task TestSuggestions(){
			var context = Substitute.For<CommandContext<object>>(null, null, null, null, null, null, null, null, null, false);
			var result = await _node.ListSuggestions(context, new SuggestionsBuilder("", 0));
			result.IsEmpty().Should().Be(true);
		}

		[Fact]// (expected = IllegalStateException.class)
		public void TestCreateBuilder(){
			_node.Invoking(n => n.CreateBuilder())
				.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void TestEquals(){
		
			new EqualsTester()
				.AddEqualityGroup(
					new RootCommandNode<object>(),
					new RootCommandNode<object>()
				)
				.AddEqualityGroup(
					new RootCommandNode<object> {
						LiteralArgumentBuilder<object>.LiteralArgument("foo").Build(),
					},
					new RootCommandNode<object> {
						LiteralArgumentBuilder<object>.LiteralArgument("foo").Build()
					}
				)
				.TestEquals();
		}
	}
}