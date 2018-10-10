// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Builder;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.builder
{
	public class RequiredArgumentBuilderTest {
		private readonly ArgumentType<int> _type = Substitute.For<ArgumentType<int>>();
		private readonly RequiredArgumentBuilder<object, int> _builder;
		private readonly Command<object> _command = Substitute.For<Command<object>>();

		public RequiredArgumentBuilderTest()
		{
			_builder = RequiredArgumentBuilder<object, int>.RequiredArgument("foo", _type);
		}

		[Fact]
		public void TestBuild(){
			var node = _builder.Build();

			node.Name.Should().Be("foo");
			node.Type.Should().Be(_type);
		}

		[Fact]
		public void TestBuildWithExecutor(){
			var node = _builder.Executes(_command).Build();

			node.Name.Should().Be("foo");
			node.Type.Should().Be(_type);
			node.Command.Should().Be(_command);
		}

		[Fact]
		public void TestBuildWithChildren(){
			_builder.Then(r => r.Argument("bar", Arguments.Integer()));
			_builder.Then(r => r.Argument("baz", Arguments.Integer()));
			var node = _builder.Build();

			node.Children.Should().HaveCount(2);
		}
	}
}