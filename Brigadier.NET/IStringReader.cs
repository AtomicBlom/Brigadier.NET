namespace Brigadier.NET
{
	public interface IStringReader : IImmutableStringReader
	{
		//I hate this, but .net doesn't allow you to add a setter to a property defined in an interface with only a getter.
		new int Cursor { get; set; }
		char Next();
		void Skip();
		void SkipWhitespace();
		int ReadInt();
		long ReadLong();
		double ReadDouble();
		float ReadFloat();
		string ReadUnquotedString();
		string ReadQuotedString();
		string ReadString();
		bool ReadBoolean();
		void Expect(char c);
	}
}