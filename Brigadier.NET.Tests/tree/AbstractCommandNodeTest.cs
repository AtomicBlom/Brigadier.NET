// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Brigadier.NET.Builder;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.tree
{
	public abstract class AbstractCommandNodeTest {
		private readonly Command<object> _command = Substitute.For<Command<object>>();

		protected abstract CommandNode<object> GetCommandNode();

		[Fact]
		public void TestAddChild(){
			var node = GetCommandNode();

		

			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child1").Build());
			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child2").Build());
			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child1").Build());

			node.Children.Should().HaveCount(2);
		}

		[Fact]
		public void TestAddChildMergesGrandchildren(){
			var node = GetCommandNode();

			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child")
				.Then(c => c.Literal("grandchild1"))
				.Build());

			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child")
				.Then(c => c.Literal("grandchild2"))
				.Build());

			node.Children.Should().HaveCount(1);
			node.Children.First().Children.Should().HaveCount(2);
		}

		[Fact]
		public void TestAddChildPreservesCommand(){
			var node = GetCommandNode();

			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child").Executes(_command).Build());
			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child").Build());

			node.Children.First().Command.Should().Be(_command);
		}

		[Fact]
		public void TestAddChildOverwritesCommand(){
			var node = GetCommandNode();

			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child").Build());
			node.AddChild(LiteralArgumentBuilder<object>.LiteralArgument("child").Executes(_command).Build());

			node.Children.First().Command.Should().Be(_command);
		}
	}
}
