// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Brigadier.NET.Builder;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.builder
{
	public class ArgumentBuilderTest {
		private readonly TestableArgumentBuilder<object> _builder;

		public ArgumentBuilderTest()
		{
			_builder = new TestableArgumentBuilder<object>();
		}

		[Fact]
		public void TestArguments()
		{
			var argument = RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer());

			_builder.Then(argument);

			_builder.Arguments.Should().HaveCount(1);
			_builder.Arguments.Should().ContainSingle().Which.Should().Be(argument.Build());
		}

		[Fact]
		public void TestRedirect(){
			var target = Substitute.For<CommandNode<object>>(null, null, null, null, false);
			_builder.Redirect(target);
			_builder.RedirectTarget.Should().Be(target);
		}

		[Fact]
		public void testRedirect_withChild(){
			var target = Substitute.For<CommandNode<object>>(null, null, null, null, false);
			_builder.Then(r => r.Literal("foot"));
			_builder.Invoking(b => b.Redirect(target))
				.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void testThen_withRedirect()
		{
			var target = Substitute.For<CommandNode<object>>(null, null, null, null, false);

			_builder.Redirect(target);
			_builder.Invoking(b => b.Then(r => r.Literal("foot")))
				.Should().Throw<InvalidOperationException>();
		}

		internal class TestableArgumentBuilder<TSource> : ArgumentBuilder<TSource, TestableArgumentBuilder<TSource>, CommandNode<TSource>> {
			public override CommandNode<TSource> Build()
			{
				throw new NotImplementedException();
			}
		}
	}
}