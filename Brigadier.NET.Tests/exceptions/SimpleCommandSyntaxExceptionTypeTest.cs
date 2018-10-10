// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.exceptions
{
	public class SimpleCommandSyntaxExceptionTypeTest {
		[Fact]
		public void CreateWithContext(){
			var type = new SimpleCommandExceptionType(new LiteralMessage("error"));
			var reader = new StringReader("Foo bar")
			{
				Cursor = 5
			};
			var exception = type.CreateWithContext(reader);
			exception.Type.Should().Be(type);
			exception.Input.Should().Be("Foo bar");
			exception.Cursor.Should().Be(5);
		}


		[Fact]
		public void getContext_none(){
			var exception = new CommandSyntaxException(Substitute.For<ICommandExceptionType>(), new LiteralMessage("error"));
			exception.Context.Should().BeNull();
		}

		[Fact]
		public void getContext_short(){
			var exception = new CommandSyntaxException(Substitute.For<ICommandExceptionType>(), new LiteralMessage("error"), "Hello world!", 5);

			exception.Context.Should().BeEquivalentTo("Hello<--[HERE]");
		}

		[Fact]
		public void getContext_long(){
			var exception = new CommandSyntaxException(Substitute.For<ICommandExceptionType>(), new LiteralMessage("error"), "Hello world! This has an error in it. Oh dear!", 20);
			exception.Context.Should().BeEquivalentTo("...d! This ha<--[HERE]");
		}
	}
}