// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Arguments;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.arguments
{
	public class StringArgumentTypeTest {

		[Fact]
		public void TestParseWord()
		{
			var reader = Substitute.For<IStringReader>();
			reader.ReadUnquotedString().Returns("hello");
			StringArgumentType.Word().Parse(reader).Should().BeEquivalentTo("hello");

			reader.Received().ReadUnquotedString();
		}

		[Fact]
		public void TestParseString(){
			var reader = Substitute.For<IStringReader>();
			reader.ReadString().Returns("hello world");
			StringArgumentType.Phrase().Parse(reader).Should().BeEquivalentTo("hello world");
			reader.Received().ReadString();
		}

		[Fact]
		public void TestParseGreedyString(){
			var reader = new StringReader("Hello world! This is a test.");
			StringArgumentType.GreedyString().Parse(reader).Should().BeEquivalentTo("Hello world! This is a test.");
			reader.CanRead().Should().Be(false);
		}

		[Fact]
		public void TestToString(){
			StringArgumentType.Phrase().ToString().Should().BeEquivalentTo("string()");
		}

		[Fact]
		public void testEscapeIfRequired_notRequired(){
			StringArgumentType.EscapeIfRequired("hello").Should().BeEquivalentTo("hello");
			StringArgumentType.EscapeIfRequired("").Should().BeEquivalentTo("");
		}

		[Fact]
		public void testEscapeIfRequired_multipleWords(){
			StringArgumentType.EscapeIfRequired("hello world").Should().BeEquivalentTo("\"hello world\"");
		}

		[Fact]
		public void testEscapeIfRequired_quote(){
			StringArgumentType.EscapeIfRequired("hello \"world\"!").Should().BeEquivalentTo("\"hello \\\"world\\\"!\"");
		}

		[Fact]
		public void testEscapeIfRequired_escapes(){
			StringArgumentType.EscapeIfRequired("\\").Should().BeEquivalentTo("\"\\\\\"");
		}

		[Fact]
		public void testEscapeIfRequired_singleQuote(){
			StringArgumentType.EscapeIfRequired("\"").Should().BeEquivalentTo("\"\\\"\"");
		}
	}
}