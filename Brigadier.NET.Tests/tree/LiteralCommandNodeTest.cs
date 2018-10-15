// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.tree
{
	public class LiteralCommandNodeTest : AbstractCommandNodeTest {
		private readonly LiteralCommandNode<object> _node;
		private readonly CommandContextBuilder<object> _contextBuilder;

		protected override CommandNode<object> GetCommandNode() {
			return _node;
		}

		public LiteralCommandNodeTest()
		{
			_node = LiteralArgumentBuilder<object>.LiteralArgument("foo").Build();
			_contextBuilder = new CommandContextBuilder<object>(new CommandDispatcher<object>(), new object(), new RootCommandNode<object>(), 0);
		}

		[Fact]
		public void TestParse(){
			var reader = new StringReader("foo bar");
			_node.Parse(reader, _contextBuilder);
			reader.Remaining.Should().BeEquivalentTo(" bar");
		}

		[Fact]
		public void TestParseExact(){
			var reader = new StringReader("foo");
			_node.Parse(reader, _contextBuilder);
			reader.Remaining.Should().BeEquivalentTo("");
		}

		[Fact]
		public void TestParseSimilar(){
			var reader = new StringReader("foobar");
			_node.Invoking(n => n.Parse(reader, _contextBuilder))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.LiteralIncorrect())
				.Where(ex => ex.Cursor == 0);
	
		}

		[Fact]
		public void TestParseInvalid(){
			var reader = new StringReader("bar");
			_node.Invoking(n => n.Parse(reader, _contextBuilder))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.LiteralIncorrect())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestUsage(){
			_node.UsageText.Should().Be("foo");
		}

		[Fact]
		public void TestSuggestions(){
			var empty = _node.ListSuggestions(_contextBuilder.Build(""), new SuggestionsBuilder("", 0));
			empty.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.At(0), "foo") });

			var foo = _node.ListSuggestions(_contextBuilder.Build("foo"), new SuggestionsBuilder("foo", 0));
			foo.IsEmpty().Should().Be(true);

			var food = _node.ListSuggestions(_contextBuilder.Build("food"), new SuggestionsBuilder("food", 0));
			food.IsEmpty().Should().Be(true);

			var b = _node.ListSuggestions(_contextBuilder.Build("b"), new SuggestionsBuilder("b", 0));
			b.IsEmpty().Should().Be(true);
		}

		[Fact]
		public void TestEquals()
		{
			var command = Substitute.For<Command<object>>();

			new EqualsTester()
				.AddEqualityGroup(
					LiteralArgumentBuilder<object>.LiteralArgument("foo").Build(),
					LiteralArgumentBuilder<object>.LiteralArgument("foo").Build()
				)
				.AddEqualityGroup(
					LiteralArgumentBuilder<object>.LiteralArgument("bar").Executes(command).Build(),
					LiteralArgumentBuilder<object>.LiteralArgument("bar").Executes(command).Build()
				)
				.AddEqualityGroup(
					LiteralArgumentBuilder<object>.LiteralArgument("bar").Build(),
					LiteralArgumentBuilder<object>.LiteralArgument("bar").Build()
				)
				.AddEqualityGroup(
					LiteralArgumentBuilder<object>.LiteralArgument("foo").Then(LiteralArgumentBuilder<object>.LiteralArgument("bar")).Build(),
					LiteralArgumentBuilder<object>.LiteralArgument("foo").Then(LiteralArgumentBuilder<object>.LiteralArgument("bar")).Build()
				)
				.TestEquals();
		}

		[Fact]
		public void TestCreateBuilder()
		{
			var builder = (LiteralArgumentBuilder<object>)_node.CreateBuilder();
			builder.Literal.Should().Be(_node.Literal);
			builder.Requirement.Should().Be(_node.Requirement);
			builder.Command.Should().Be(_node.Command);
		}
	}
}