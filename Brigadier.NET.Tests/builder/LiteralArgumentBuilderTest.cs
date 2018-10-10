// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Arguments;
using Brigadier.NET.Builder;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.builder
{
	public class LiteralArgumentBuilderTest {
		private readonly LiteralArgumentBuilder<object> _builder;
		private readonly Command<object> _command = Substitute.For<Command<object>>();

		public LiteralArgumentBuilderTest()
		{
			_builder = new LiteralArgumentBuilder<object>("foo");
		}

		[Fact]
		public void TestBuild(){
			var node = _builder.Build();

			node.Literal.Should().Be("foo");
		}

		[Fact]
		public void TestBuildWithExecutor(){
			var node = _builder.Executes(_command).Build();

			node.Literal.Should().Be("foo");
			node.Command.Should().Be(_command);
		}

		[Fact]
		public void TestBuildWithChildren()
		{
			_builder.Then(c => c.Argument("bar", IntegerArgumentType.Integer()));
			_builder.Then(c => c.Argument("baz", IntegerArgumentType.Integer()));
			var node = _builder.Build();

			node.Children.Should().HaveCount(2);
		}
	}
}