// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.suggestion
{
	public class SuggestionsTest {
		[Fact]
		public void merge_empty() {
			var merged = Suggestions.Merge("foo b", new List<Suggestions>());
			merged.IsEmpty().Should().Be(true);
		}

		[Fact]
		public void merge_single() {
			var suggestions = new Suggestions(StringRange.At(5), new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.At(5), "ar") });
			var merged = Suggestions.Merge("foo b", new List<Suggestions> { suggestions });
			merged.Should().BeEquivalentTo(suggestions);
		}

		[Fact]
		public void merge_multiple() {
			var a = new Suggestions(StringRange.At(5), new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.At(5), "ar"), new Suggestion.Suggestion(StringRange.At(5), "az"), new Suggestion.Suggestion(StringRange.At(5), "Az") });
			var b = new Suggestions(StringRange.Between(4, 5), new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.Between(4, 5), "foo"), new Suggestion.Suggestion(StringRange.Between(4, 5), "qux"), new Suggestion.Suggestion(StringRange.Between(4, 5), "apple"), new Suggestion.Suggestion(StringRange.Between(4, 5), "Bar") });
			var merged = Suggestions.Merge("foo b", new List<Suggestions> { a, b });
			merged.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.Between(4, 5), "apple"), new Suggestion.Suggestion(StringRange.Between(4, 5), "bar"), new Suggestion.Suggestion(StringRange.Between(4, 5), "Bar"), new Suggestion.Suggestion(StringRange.Between(4, 5), "baz"), new Suggestion.Suggestion(StringRange.Between(4, 5), "bAz"), new Suggestion.Suggestion(StringRange.Between(4, 5), "foo"), new Suggestion.Suggestion(StringRange.Between(4, 5), "qux") });
		}
	}
}