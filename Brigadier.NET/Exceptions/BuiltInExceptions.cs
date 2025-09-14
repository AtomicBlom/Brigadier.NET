namespace Brigadier.NET.Exceptions;

public class BuiltInExceptions : IBuiltInExceptionProvider
{

	private static readonly Dynamic2CommandExceptionType DOUBLE_TOO_SMALL = new((found, min) => new LiteralMessage($"Double must not be less than {min}, found {found}"));
	private static readonly Dynamic2CommandExceptionType DOUBLE_TOO_BIG = new((found, max) => new LiteralMessage($"Double must not be more than {max}, found {found}"));

	private static readonly Dynamic2CommandExceptionType FLOAT_TOO_SMALL = new((found, min) => new LiteralMessage($"Float must not be less than {min}, found {found}"));
	private static readonly Dynamic2CommandExceptionType FLOAT_TOO_BIG = new((found, max) => new LiteralMessage($"Float must not be more than {max}, found {found}"));

	private static readonly Dynamic2CommandExceptionType INTEGER_TOO_SMALL = new((found, min) => new LiteralMessage($"Integer must not be less than {min}, found {found}"));
	private static readonly Dynamic2CommandExceptionType INTEGER_TOO_BIG = new((found, max) => new LiteralMessage($"Integer must not be more than {max}, found {found}"));

	private static readonly Dynamic2CommandExceptionType LONG_TOO_SMALL = new((found, min) => new LiteralMessage($"Long must not be less than {min}, found {found}"));
	private static readonly Dynamic2CommandExceptionType LONG_TOO_BIG = new((found, max) => new LiteralMessage($"Long must not be more than {max}, found {found}"));

	private static readonly DynamicCommandExceptionType LITERAL_INCORRECT = new(expected => new LiteralMessage($"Expected literal {expected}"));

	private static readonly SimpleCommandExceptionType READER_EXPECTED_START_OF_QUOTE = new(new LiteralMessage("Expected quote to start a string"));
	private static readonly SimpleCommandExceptionType READER_EXPECTED_END_OF_QUOTE = new(new LiteralMessage("Unclosed quoted string"));
	private static readonly DynamicCommandExceptionType READER_INVALID_ESCAPE = new(character => new LiteralMessage($"Invalid escape sequence '{character}' in quoted string"));
	private static readonly DynamicCommandExceptionType READER_INVALID_BOOL = new(value => new LiteralMessage($"Invalid bool, expected true or false but found '{value}'"));
	private static readonly DynamicCommandExceptionType READER_INVALID_INT = new(value => new LiteralMessage($"Invalid integer '{value}'"));
	private static readonly SimpleCommandExceptionType READER_EXPECTED_INT = new(new LiteralMessage("Expected integer"));
	private static readonly DynamicCommandExceptionType READER_INVALID_LONG = new(value => new LiteralMessage($"Invalid long '{value}'"));
	private static readonly SimpleCommandExceptionType READER_EXPECTED_LONG = new((new LiteralMessage("Expected long")));
	private static readonly DynamicCommandExceptionType READER_INVALID_DOUBLE = new(value => new LiteralMessage($"Invalid double '{value}'"));
	private static readonly SimpleCommandExceptionType READER_EXPECTED_DOUBLE = new(new LiteralMessage("Expected double"));
	private static readonly DynamicCommandExceptionType READER_INVALID_FLOAT = new(value => new LiteralMessage($"Invalid float '{value}'"));
	private static readonly SimpleCommandExceptionType READER_EXPECTED_FLOAT = new(new LiteralMessage("Expected float"));
	private static readonly SimpleCommandExceptionType READER_EXPECTED_BOOL = new(new LiteralMessage("Expected bool"));
	private static readonly DynamicCommandExceptionType READER_EXPECTED_SYMBOL = new(symbol => new LiteralMessage($"Expected '{symbol}'"));

	private static readonly SimpleCommandExceptionType DISPATCHER_UNKNOWN_COMMAND = new(new LiteralMessage("Unknown command"));
	private static readonly SimpleCommandExceptionType DISPATCHER_UNKNOWN_ARGUMENT = new(new LiteralMessage("Incorrect argument for command"));
	private static readonly SimpleCommandExceptionType DISPATCHER_EXPECTED_ARGUMENT_SEPARATOR = new(new LiteralMessage("Expected whitespace to end one argument, but found trailing data"));
	private static readonly DynamicCommandExceptionType DISPATCHER_PARSE_EXCEPTION = new(message => new LiteralMessage($"Could not parse command: {message}"));

	public Dynamic2CommandExceptionType DoubleTooLow()
	{
		return DOUBLE_TOO_SMALL;
	}

	public Dynamic2CommandExceptionType DoubleTooHigh()
	{
		return DOUBLE_TOO_BIG;
	}

	public Dynamic2CommandExceptionType FloatTooLow()
	{
		return FLOAT_TOO_SMALL;
	}

	public Dynamic2CommandExceptionType FloatTooHigh()
	{
		return FLOAT_TOO_BIG;
	}

	public Dynamic2CommandExceptionType IntegerTooLow()
	{
		return INTEGER_TOO_SMALL;
	}

	public Dynamic2CommandExceptionType IntegerTooHigh()
	{
		return INTEGER_TOO_BIG;
	}

	public Dynamic2CommandExceptionType LongTooLow()
	{
		return LONG_TOO_SMALL;
	}

	public Dynamic2CommandExceptionType LongTooHigh()
	{
		return LONG_TOO_BIG;
	}

	public DynamicCommandExceptionType LiteralIncorrect()
	{
		return LITERAL_INCORRECT;
	}

	public SimpleCommandExceptionType ReaderExpectedStartOfQuote()
	{
		return READER_EXPECTED_START_OF_QUOTE;
	}

	public SimpleCommandExceptionType ReaderExpectedEndOfQuote()
	{
		return READER_EXPECTED_END_OF_QUOTE;
	}

	public DynamicCommandExceptionType ReaderInvalidEscape()
	{
		return READER_INVALID_ESCAPE;
	}

	public DynamicCommandExceptionType ReaderInvalidBool()
	{
		return READER_INVALID_BOOL;
	}

	public DynamicCommandExceptionType ReaderInvalidInt()
	{
		return READER_INVALID_INT;
	}

	public SimpleCommandExceptionType ReaderExpectedInt()
	{
		return READER_EXPECTED_INT;
	}

	public DynamicCommandExceptionType ReaderInvalidLong()
	{
		return READER_INVALID_LONG;
	}

	public SimpleCommandExceptionType ReaderExpectedLong()
	{
		return READER_EXPECTED_LONG;
	}

	public DynamicCommandExceptionType ReaderInvalidDouble()
	{
		return READER_INVALID_DOUBLE;
	}

	public SimpleCommandExceptionType ReaderExpectedDouble()
	{
		return READER_EXPECTED_DOUBLE;
	}

	public DynamicCommandExceptionType ReaderInvalidFloat()
	{
		return READER_INVALID_FLOAT;
	}

	public SimpleCommandExceptionType ReaderExpectedFloat()
	{
		return READER_EXPECTED_FLOAT;
	}

	public SimpleCommandExceptionType ReaderExpectedBool()
	{
		return READER_EXPECTED_BOOL;
	}

	public DynamicCommandExceptionType ReaderExpectedSymbol()
	{
		return READER_EXPECTED_SYMBOL;
	}

	public SimpleCommandExceptionType DispatcherUnknownCommand()
	{
		return DISPATCHER_UNKNOWN_COMMAND;
	}

	public SimpleCommandExceptionType DispatcherUnknownArgument()
	{
		return DISPATCHER_UNKNOWN_ARGUMENT;
	}

	public SimpleCommandExceptionType DispatcherExpectedArgumentSeparator()
	{
		return DISPATCHER_EXPECTED_ARGUMENT_SEPARATOR;
	}

	public DynamicCommandExceptionType DispatcherParseException()
	{
		return DISPATCHER_PARSE_EXCEPTION;
	}
}