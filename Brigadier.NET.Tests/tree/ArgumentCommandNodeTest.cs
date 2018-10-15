// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
	public class ArgumentCommandNodeTest : AbstractCommandNodeTest {
		private readonly ArgumentCommandNode<object, int> _node;
		private readonly CommandContextBuilder<object> _contextBuilder;

		protected override CommandNode<object> GetCommandNode() {
			return _node;
		}

		public ArgumentCommandNodeTest()
		{
			_node = RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Build();
			_contextBuilder = new CommandContextBuilder<object>(new CommandDispatcher<object>(), new object(), new RootCommandNode<object>(), 0);
		}

		[Fact]
		public void TestParse(){
			var reader = new StringReader("123 456");
			_node.Parse(reader, _contextBuilder);

			_contextBuilder.GetArguments().ContainsKey("foo").Should().Be(true);
			_contextBuilder.GetArguments()["foo"].Result.Should().Be(123);
		}

		[Fact]
		public void TestUsage(){
			_node.UsageText.Should().Be("<foo>");
		}

		[Fact]
		public void TestSuggestions(){
			var result = _node.ListSuggestions(_contextBuilder.Build(""), new SuggestionsBuilder("", 0));
			result.IsEmpty().Should().Be(true);
		}

		[Fact]
		public void TestEquals(){
			var command = Substitute.For<Command<object>>();

			new EqualsTester()
				.AddEqualityGroup(
					RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Build(),
					RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Build()
				)
				.AddEqualityGroup(
					RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Executes(command).Build(),
					RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Executes(command).Build()
				)
				.AddEqualityGroup(
					RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer(-100, 100)).Build(),
					RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer(-100, 100)).Build()
				)
				.AddEqualityGroup(
					RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer(-100, 100)).Build(),
					RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer(-100, 100)).Build()
				)
				.AddEqualityGroup(
					RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Then(
						RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer())
					).Build(),
					RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Then(
						RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer())
					).Build()
				)
				.TestEquals();
		}

		[Fact]
		public void TestCreateBuilder()
		{
			var builder = (RequiredArgumentBuilder<object, int>)_node.CreateBuilder();
			builder.Name.Should().Be(_node.Name);
			builder.Type.Should().Be(_node.Type);
			builder.Requirement.Should().Be(_node.Requirement);
			builder.Command.Should().Be(_node.Command);
		}
	}
}