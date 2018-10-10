namespace Brigadier.NET.Exceptions
{
	public class Dynamic2CommandExceptionType : ICommandExceptionType
	{
		private readonly Function _function;

		public Dynamic2CommandExceptionType(Function function)
		{
			_function = function;
		}

		public CommandSyntaxException Create(object a, object b)
		{
			return new CommandSyntaxException(this, _function(a, b));
		}

		public CommandSyntaxException CreateWithContext(IImmutableStringReader reader, object a, object b)
		{
			return new CommandSyntaxException(this, _function(a, b), reader.String, reader.Cursor);
		}

		public delegate IMessage Function(object a, object b);
	}
}
