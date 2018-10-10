// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Context;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.context
{
	public class ParsedArgumentTest {
		[Fact]
		public void TestEquals(){
			new EqualsTester()
				.AddEqualityGroup(new ParsedArgument<object, string>(0, 3, "bar"), new ParsedArgument<object, string>(0, 3, "bar"))
				.AddEqualityGroup(new ParsedArgument<object, string>(3, 6, "baz"), new ParsedArgument<object, string>(3, 6, "baz"))
				.AddEqualityGroup(new ParsedArgument<object, string>(6, 9, "baz"), new ParsedArgument<object, string>(6, 9, "baz"))
				.TestEquals();
		}

		[Fact]
		public void GetRaw(){
			var reader = new StringReader("0123456789");
			var argument = new ParsedArgument<object, string>(2, 5, "");
			argument.Range.Get(reader).Should().BeEquivalentTo("234");
		}
	}
}