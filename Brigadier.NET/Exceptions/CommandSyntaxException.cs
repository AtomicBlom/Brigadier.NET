using System;
using System.Text;

namespace Brigadier.NET.Exceptions
{
	public class CommandSyntaxException : Exception
	{
		public static readonly int ContextAmount = 10;
		public static readonly IBuiltInExceptionProvider BuiltInExceptions = new BuiltInExceptions();

		private readonly IMessage _message;

		public CommandSyntaxException(ICommandExceptionType type, IMessage message)
			: base(message.String, null)
		{
			Type = type;
			_message = message;
			Input = null;
			Cursor = -1;
		}

		public CommandSyntaxException(ICommandExceptionType type, IMessage message, string input, int cursor) 
			: base(message.String, null)
		{
			Type = type;
			_message = message;
			Input = input;
			Cursor = cursor;
		}

		public override string Message
		{
			get
			{
				var message = _message.String;
				var context = Context;
				if (context != null)
				{
					message += $" at position {Cursor}: {context}";
				}
				return message;

			}
		}

		public IMessage RawMessage() => _message;

		public string Context
		{
			get
			{
				if (Input == null || Cursor < 0)
				{
					return null;
				}

				var builder = new StringBuilder();
				var cursor = Math.Min(Input.Length, Cursor);

				if (cursor > ContextAmount)
				{
					builder.Append("...");
				}

				var start = Math.Max(0, cursor - ContextAmount);
				builder.Append(Input.Substring(start, cursor - start));
				builder.Append("<--[HERE]");

				return builder.ToString();
			}
		}

		public ICommandExceptionType Type { get; }

		public string Input { get; }

		public int Cursor { get; }
	}
}
