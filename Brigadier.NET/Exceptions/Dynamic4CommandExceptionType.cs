namespace Brigadier.NET.Exceptions
{
	public class Dynamic4CommandExceptionType : ICommandExceptionType
	{
		private readonly Function _function;

		public Dynamic4CommandExceptionType(Function function)
		{
			_function = function;
		}

		public CommandSyntaxException Create(object a, object b, object c, object d)
		{
			return new CommandSyntaxException(this, _function(a, b, c, d));
		}

		public CommandSyntaxException CreateWithContext(IImmutableStringReader reader, object a, object b, object c, object d)
		{
			return new CommandSyntaxException(this, _function(a, b, c, d), reader.String, reader.Cursor);
		}

		public delegate IMessage Function(object a, object b, object c, object d);
	}
}
