// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.suggestion
{
	public class SuggestionsBuilderTest {
		private readonly SuggestionsBuilder _builder;

		public SuggestionsBuilderTest()
		{
			_builder = new SuggestionsBuilder("Hello w", 6);
		}

		[Fact]
		public void suggest_appends() {
			var result = _builder.Suggest("world!").Build();
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.Between(6, 7), "world!") });
			result.Range.Should().BeEquivalentTo(StringRange.Between(6, 7));
			result.IsEmpty().Should().Be(false);
		}

		[Fact]
		public void suggest_replaces() {
			var result = _builder.Suggest("everybody").Build();
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.Between(6, 7), "everybody") });
			result.Range.Should().BeEquivalentTo(StringRange.Between(6, 7));
			result.IsEmpty().Should().Be(false);
		}

		[Fact]
		public void suggest_noop() {
			var result = _builder.Suggest("w").Build();
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>());
			result.IsEmpty().Should().Be(true);
		}

		[Fact]
		public void suggest_multiple() {
			var result = _builder.Suggest("world!").Suggest("everybody").Suggest("weekend").Build();
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.Between(6, 7), "everybody"), new Suggestion.Suggestion(StringRange.Between(6, 7), "weekend"), new Suggestion.Suggestion(StringRange.Between(6, 7), "world!") });
			result.Range.Should().BeEquivalentTo(StringRange.Between(6, 7));
			result.IsEmpty().Should().Be(false);
		}

		[Fact]
		public void Restart() {
			_builder.Suggest("won't be included in restart");
			var other = _builder.Restart();
			other.Should().NotBe(_builder);
			other.Input.Should().BeEquivalentTo(_builder.Input);
			other.Start.Should().Be(_builder.Start);
			other.Remaining.Should().BeEquivalentTo(_builder.Remaining);
		}

		[Fact]
		public void sort_alphabetical() {
			var result = _builder.Suggest("2").Suggest("4").Suggest("6").Suggest("8").Suggest("30").Suggest("32").Build();
			var actual = result.List.Select(s => s.Text).ToList();
			actual.Should().BeEquivalentTo(new List<string> { "2", "30", "32", "4", "6", "8" });
		}

		[Fact]
		public void sort_numerical() {
			var result = _builder.Suggest(2).Suggest(4).Suggest(6).Suggest(8).Suggest(30).Suggest(32).Build();
			var actual = result.List.Select(s => s.Text).ToList();
			actual.Should().BeEquivalentTo(new List<string> { "2", "4", "6", "8", "30", "32" });
		}

		[Fact]
		public void sort_mixed() {
			var result = _builder.Suggest("11").Suggest("22").Suggest("33").Suggest("a").Suggest("b").Suggest("c").Suggest(2).Suggest(4).Suggest(6).Suggest(8).Suggest(30).Suggest(32).Suggest("3a").Suggest("a3").Build();
			var actual = result.List.Select(s => s.Text).ToList();
			actual.Should().BeEquivalentTo(new List<string> { "11", "2", "22", "33", "3a", "4", "6", "8", "30", "32", "a", "a3", "b", "c" });
		}
	}
}
