// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Context;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.suggestion;

public class SuggestionTest {
	[Fact]
	public void apply_insertion_start() {
		var suggestion = new Suggestion.Suggestion(StringRange.At(0), "And so I said: ");
		suggestion.Apply("Hello world!").Should().BeEquivalentTo("And so I said: Hello world!");
	}

	[Fact]
	public void apply_insertion_middle() {
		var suggestion = new Suggestion.Suggestion(StringRange.At(6), "small ");
		suggestion.Apply("Hello world!").Should().BeEquivalentTo("Hello small world!");
	}

	[Fact]
	public void apply_insertion_end() {
		var suggestion = new Suggestion.Suggestion(StringRange.At(5), " world!");
		suggestion.Apply("Hello").Should().BeEquivalentTo("Hello world!");
	}

	[Fact]
	public void apply_replacement_start() {
		var suggestion = new Suggestion.Suggestion(StringRange.Between(0, 5), "Goodbye");
		suggestion.Apply("Hello world!").Should().BeEquivalentTo("Goodbye world!");
	}

	[Fact]
	public void apply_replacement_middle() {
		var suggestion = new Suggestion.Suggestion(StringRange.Between(6, 11), "Alex");
		suggestion.Apply("Hello world!").Should().BeEquivalentTo("Hello Alex!");
	}

	[Fact]
	public void apply_replacement_end() {
		var suggestion = new Suggestion.Suggestion(StringRange.Between(6, 12), "Creeper!");
		suggestion.Apply("Hello world!").Should().BeEquivalentTo("Hello Creeper!");
	}

	[Fact]
	public void apply_replacement_everything() {
		var suggestion = new Suggestion.Suggestion(StringRange.Between(0, 12), "Oh dear.");
		suggestion.Apply("Hello world!").Should().BeEquivalentTo("Oh dear.");
	}

	[Fact]
	public void expand_unchanged() {
		var suggestion = new Suggestion.Suggestion(StringRange.At(1), "oo");
		suggestion.Expand("f", StringRange.At(1)).Should().BeEquivalentTo(suggestion);
	}

	[Fact]
	public void expand_left() {
		var suggestion = new Suggestion.Suggestion(StringRange.At(1), "oo");
		suggestion.Expand("f", StringRange.Between(0, 1)).Should().BeEquivalentTo(new Suggestion.Suggestion(StringRange.Between(0, 1), "foo"));
	}

	[Fact]
	public void expand_right() {
		var suggestion = new Suggestion.Suggestion(StringRange.At(0), "minecraft:");
		suggestion.Expand("fish", StringRange.Between(0, 4)).Should().BeEquivalentTo(new Suggestion.Suggestion(StringRange.Between(0, 4), "minecraft:fish"));
	}

	[Fact]
	public void expand_both() {
		var suggestion = new Suggestion.Suggestion(StringRange.At(11), "minecraft:");
		suggestion.Expand("give Steve fish_block", StringRange.Between(5, 21)).Should().BeEquivalentTo(new Suggestion.Suggestion(StringRange.Between(5, 21), "Steve minecraft:fish_block"));
	}

	[Fact]
	public void expand_replacement() {
		var suggestion = new Suggestion.Suggestion(StringRange.Between(6, 11), "strangers");
		suggestion.Expand("Hello world!", StringRange.Between(0, 12)).Should().BeEquivalentTo(new Suggestion.Suggestion(StringRange.Between(0, 12), "Hello strangers!"));
	}
}