// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Exceptions;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.arguments;

public class LongArgumentTypeTest {
	[Fact]
	public void Parse(){
		var reader = new StringReader("15");
		Arguments.Long().Parse(reader).Should().Be(15L);
		reader.CanRead().Should().Be(false);
	}

	[Fact]
	public void parse_tooSmall(){
		var reader = new StringReader("-5");
		Arguments.Long(0, 100).Invoking(l => l.Parse(reader))
			.Should().Throw<CommandSyntaxException>()
			.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.LongTooLow())
			.Where(ex => ex.Cursor == 0);
	}

	[Fact]
	public void parse_tooBig(){
		var reader = new StringReader("5");

		Arguments.Long(-100, 0).Invoking(l => l.Parse(reader))
			.Should().Throw<CommandSyntaxException>()
			.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.LongTooHigh())
			.Where(ex => ex.Cursor == 0);
	}

	[Fact]
	public void TestEquals(){
		new EqualsTester()
			.AddEqualityGroup(Arguments.Long(), Arguments.Long())
			.AddEqualityGroup(Arguments.Long(-100, 100), Arguments.Long(-100, 100))
			.AddEqualityGroup(Arguments.Long(-100, 50), Arguments.Long(-100, 50))
			.AddEqualityGroup(Arguments.Long(-50, 100), Arguments.Long(-50, 100))
			.TestEquals();
	}

	[Fact]
	public void TestToString(){
		Arguments.Long().ToString().Should().BeEquivalentTo("longArg()");
		Arguments.Long(-100).ToString().Should().BeEquivalentTo("longArg(-100)");
		Arguments.Long(-100, 100).ToString().Should().BeEquivalentTo("longArg(-100, 100)");
		Arguments.Long(long.MinValue, 100).ToString().Should().BeEquivalentTo("longArg(-9223372036854775808, 100)");
	}
}