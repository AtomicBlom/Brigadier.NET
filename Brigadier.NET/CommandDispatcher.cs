using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;

namespace Brigadier.NET
{
	public class CommandDispatcher<TSource>
	{
		/// <summary>
		/// The string required to separate individual arguments in an input string
		/// </summary>
		/// <seealso cref="ArgumentSeparatorChar" />
		public string ArgumentSeparator = " ";

		/// <summary>
		/// The char required to separate individual arguments in an input string
		/// </summary>
		/// <seealso cref="ArgumentSeparator" />
		public char ArgumentSeparatorChar = ' ';

		private const string UsageOptionalOpen = "[";
		private const string UsageOptionalClose = "]";
		private const string UsageRequiredOpen = "(";
		private const string UsageRequiredClose = ")";
		private const string UsageOr = "|";

		private readonly RootCommandNode<TSource> _root;

		private readonly Predicate<CommandNode<TSource>> _hasCommand;

		/// <summary>Sets a callback to be informed of the result of every command.</summary>
		public ResultConsumer<TSource> Consumer { get; set; } = (c, s, r) => { };
		
		/// <summary>Create a new CommandDispatcher with the specified root node.</summary>
		/// 
		/// <remarks> This is often useful to copy existing or pre-defined command trees. </remarks>
		/// 
		/// <param name="root">The existing <see cref="RootCommandNode{TSource}"/> to use as the basis for this tree.</param>
		public CommandDispatcher(RootCommandNode<TSource> root)
		{
			_root = root;
			_hasCommand = input => input != null && (input.Command != null || input.Children.Any(c => _hasCommand?.Invoke(c) ?? false));
		}

		/// <summary> Creates a new <see cref="CommandDispatcher{TSource}"/> with an empty command tree. </summary>
		public CommandDispatcher()
			: this(new RootCommandNode<TSource>())
		{
		}

		/// <summary>Utility method for registering new commands.</summary>
		/// 
		/// <remarks>
		/// This is a shortcut for building the provided <see cref="LiteralArgumentBuilder{TSource}"/> and
		/// adding it to the root via <see cref="RootCommandNode{TSource}.AddChild"/>. As <see cref="RootCommandNode{TSource}"/> can
		/// only hold literals, this method will only allow literal arguments.
		/// </remarks>
		/// 
		/// <param name="command">A literal argument builder to add to this command tree.</param>
		/// <returns>The node added to this tree.</returns>
		public LiteralCommandNode<TSource> Register(LiteralArgumentBuilder<TSource> command)
		{
			var build = command.Build();
			_root.AddChild(build);
			return build;
		}

		/// <summary>Utility method for registering new commands using a builder factory. </summary>
		/// 
		/// <remarks>
		/// This is a shortcut for building the provided <see cref="LiteralArgumentBuilder{TSource}" /> (created
		/// by invoking the factory) and adding it to the root. As <see cref="RootCommandNode{TSource}" /> can
		/// only hold literals, this method will only allow literal arguments.
		/// </remarks>
		/// 
		/// <param name="command">A factory that produces a  <see cref="LiteralArgumentBuilder{TSource}" /> to add to this command tree.</param>
		/// <returns>The node added to this tree.</returns>
		public LiteralCommandNode<TSource> Register(Func<IArgumentContext<TSource>, LiteralArgumentBuilder<TSource>> command)
		{
			var build = command(default(ArgumentContext<TSource>)).Build();
			_root.AddChild(build);
			return build;
		}

		/// <summary>Parses and executes a given command.</summary>
		/// 
		/// <remarks>
		/// <para>This is a shortcut to first <see cref="Parse(StringReader, TSource)"/> and then <see cref="Execute(ParseResults{TSource})"/>.</para>
		/// 
		/// <para>It is recommended to parse and execute as separate steps, as parsing is often the most expensive step, and easiest to cache.</para>
		/// 
		/// <para>If this command returns a value, then it successfully executed something. If it could not parse the command, or the execution was a failure,
		/// then an exception will be thrown. Most exceptions will be of type <see cref="CommandSyntaxException" />, but it is possible that an <see cref="Exception" />
		/// may bubble up from the result of a command. The meaning behind the returned result is arbitrary, and will depend
		/// entirely on what command was performed.</para>
		/// 
		/// <para>If the command passes through a node that is <see cref="CommandNode{TSource}.IsFork" /> then it will be 'forked'.
		/// A forked command will not bubble up any <see cref="CommandSyntaxException"/>s, and the 'result' returned will turn into
		/// 'amount of successful commands executes'.</para>
		/// 
		/// <para>After each and any command is ran, a registered callback given to <see cref="Consumer"/>
		/// will be notified of the result and success of the command. You can use that method to gather more meaningful
		/// results than this method will return, especially when a command forks.</para>
		/// </remarks>
		/// 
		/// <param name="input">A command string to parse and execute.</param>
		/// <param name="source">A custom source object, representing the originator of this command.</param>
		/// <returns>A numeric result from the command that was performed.</returns>
		/// <exception cref="CommandSyntaxException">If the command failed to parse or execute.</exception>
		/// <seealso cref="Parse(string, TSource)"/>
		/// <seealso cref="Parse(StringReader, TSource)"/>
		/// <seealso cref="Execute(string, TSource)"/>
		/// <seealso cref="Execute(StringReader, TSource)"/>
		public int Execute(string input, TSource source)
		{
			return Execute(new StringReader(input), source);
		}

		/// <summary>Parses and executes a given command.</summary>
		/// 
		/// <remarks>
		/// <para>This is a shortcut to first <see cref="Parse(StringReader, TSource)"/> and then <see cref="Execute(ParseResults{TSource})"/>.</para>
		/// 
		/// <para>It is recommended to parse and execute as separate steps, as parsing is often the most expensive step, and easiest to cache.</para>
		/// 
		/// <para>If this command returns a value, then it successfully executed something. If it could not parse the command, or the execution was a failure,
		/// then an exception will be thrown. Most exceptions will be of type <see cref="CommandSyntaxException" />, but it is possible that an <see cref="Exception" />
		/// may bubble up from the result of a command. The meaning behind the returned result is arbitrary, and will depend
		/// entirely on what command was performed.</para>
		/// 
		/// <para>If the command passes through a node that is <see cref="CommandNode{TSource}.IsFork" /> then it will be 'forked'.
		/// A forked command will not bubble up any <see cref="CommandSyntaxException"/>s, and the 'result' returned will turn into
		/// 'amount of successful commands executes'.</para>
		/// 
		/// <para>After each and any command is ran, a registered callback given to <see cref="Consumer"/>
		/// will be notified of the result and success of the command. You can use that method to gather more meaningful
		/// results than this method will return, especially when a command forks.</para>
		/// </remarks>
		/// 
		/// <param name="input">A StringReader to parse and execute.</param>
		/// <param name="source">A custom source object, representing the originator of this command.</param>
		/// <returns>A numeric result from the command that was performed.</returns>
		/// <exception cref="CommandSyntaxException">If the command failed to parse or execute.</exception>
		/// <seealso cref="Parse(string, TSource)"/>
		/// <seealso cref="Parse(StringReader, TSource)"/>
		/// <seealso cref="Execute(string, TSource)"/>
		/// <seealso cref="Execute(StringReader, TSource)"/>
		public int Execute(StringReader input, TSource source)
		{
			var parse = Parse(input, source);
			return Execute(parse);
		}

		/// <summary>Executes a given pre-parsed command.</summary>
		/// 
		/// <remarks>
		/// <para>If this command returns a value, then it successfully executed something. If the execution was a failure,
		/// then an exception will be thrown.
		/// Most exceptions will be of type <see cref="CommandSyntaxException" />, but it is possible that an <see cref="Exception" />
		/// may bubble up from the result of a command. The meaning behind the returned result is arbitrary, and will depend
		/// entirely on what command was performed.</para>
		/// 
		/// <para>If the command passes through a node that is <see cref="CommandNode{TSource}.IsFork" /> then it will be "forked".
		/// A forked command will not bubble up any <see cref="CommandSyntaxException" />s, and the "result" returned will turn into
		/// "amount of successful commands executions".</para>
		/// 
		/// <para>After each and any command is run, a registered callback given to <see cref="Consumer"/>
		/// will be notified of the result and success of the command. You can use that method to gather more meaningful
		/// results than this method will return, especially when a command forks.</para>
		/// </remarks>
		/// 
		/// <param name="parse">The result of a successful <see cref="Parse(string,TSource)" />.</param>
		/// <returns>A numeric result from the command that was performed.</returns>
		/// <exception cref="CommandSyntaxException" />
		/// <seealso cref="Parse(string, TSource)"/>
		/// <seealso cref="Parse(StringReader, TSource)"/>
		/// <seealso cref="Execute(string, TSource)"/>
		/// <seealso cref="Execute(StringReader, TSource)"/>
		public int Execute(ParseResults<TSource> parse)
		{
			if (parse.Reader.CanRead())
			{
				if (parse.Exceptions.Count == 1)
				{
					throw parse.Exceptions.Values.Single();
				}
				else if (parse.Context.Range.IsEmpty)
				{
					throw CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand().CreateWithContext(parse.Reader);
				}
				else
				{
					throw CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument().CreateWithContext(parse.Reader);
				}
			}

			var command = parse.Reader.String;
			var original = parse.Context.Build(command);

			if (!original.TryFlatten(out var chain))
			{
				Consumer(original, false, 0);
				throw CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand().CreateWithContext(parse.Reader);
			}
			
			return chain.ExecuteAll(original.Source, Consumer);
		}

		/// <summary>Parses a given command string.</summary>
		/// 
		/// <remarks> 
		/// <para>The result of this method can be cached, and it is advised to do so where appropriate. Parsing is often the
		/// most expensive step, and this allows you to essentially "precompile" a command if it will be run often.</para>
		/// 
		/// <para>If the command passes through a node that is <see cref="CommandNode{TSource}.IsFork"/> then the resulting context will be marked as "forked".
		/// Forked contexts may contain child contexts, which may be modified by the <see cref="RedirectModifier{TSource}" /> attached to the fork.</para>
		/// 
		/// <para>Parsing a command can never fail, you will always be provided with a new <see cref="ParseResults{TSource}"/>.
		/// However, that does not mean that it will always parse into a valid command. You should inspect the returned results
		/// to check for validity. If its <see cref="ParseResults{TSource}.Reader" /> <see cref="StringReader.CanRead()"/> then it did not finish
		/// parsing successfully. You can use that position as an indicator to the user where the command stopped being valid.
		/// You may inspect <see cref="ParseResults{TSource}.Exceptions"/> if you know the parse failed, as it will explain why it could
		/// not find any valid commands. It may contain multiple exceptions, one for each "potential node" that it could have visited,
		/// explaining why it did not go down that node.</para>
		/// 
		/// <para>When you eventually call <see cref="ParseResults{TSource}" /> with the result of this method, the above error checking
		/// will occur. You only need to inspect it yourself if you wish to handle that yourself.</para>
		/// </remarks>
		/// 
		/// <param name="command">A command string to parse.</param>
		/// <param name="source">A custom source object, representing the originator of this command.</param>
		/// <returns>The result of parsing this command.</returns>
		/// <seealso cref="Parse(StringReader, TSource)"/>
		/// <seealso cref="Execute(string, TSource)"/>
		/// <seealso cref="Execute(StringReader, TSource)"/>
		public ParseResults<TSource> Parse(string command, TSource source)
		{
			return Parse(new StringReader(command), source);
		}

		/// <summary>Parses a given command from a StringReader.</summary>
		/// 
		/// <remarks>
		/// <para>The result of this method can be cached, and it is advised to do so where appropriate. Parsing is often the
		/// most expensive step, and this allows you to essentially "precompile" a command if it will be run often.</para>
		///
		/// <para>If the command passes through a node that is <see cref="CommandNode{TSource}.IsFork"/> then the resulting context will be marked as 'forked'.
		/// Forked contexts may contain child contexts, which may be modified by the <see cref="RedirectModifier{TSource}" /> attached to the fork.</para>
		///
		/// <para>Parsing a command can never fail, you will always be provided with a new <see cref="ParseResults{TSource}" />.
		/// However, that does not mean that it will always parse into a valid command. You should inspect the returned results
		/// to check for validity. If its <see cref="ParseResults{TSource}.Reader" /> <see cref="StringReader.CanRead()" /> then it did not finish
		/// parsing successfully. You can use that position as an indicator to the user where the command stopped being valid.
		/// You may inspect <see cref="ParseResults{TSource}.Exceptions"/> if you know the parse failed, as it will explain why it could
		/// not find any valid commands. It may contain multiple exceptions, one for each "potential node" that it could have visited,
		/// explaining why it did not go down that node.</para>
		///
		/// <para>When you eventually call <see cref="Execute(ParseResults{TSource})"/> with the result of this method, the above error checking
		/// will occur. You only need to inspect it yourself if you wish to handle that yourself.</para>
		/// </remarks>
		/// 
		/// <param name="command">A StringReader containing the command to parse.</param>
		/// <param name="source">A custom source object, representing the originator of this command.</param>
		/// <returns>The result of parsing this command.</returns>
		public ParseResults<TSource> Parse(StringReader command, TSource source)
		{
			var context = new CommandContextBuilder<TSource>(this, source, _root, command.Cursor);
			return ParseNodes(_root, command, context);
		}

		private ParseResults<TSource> ParseNodes(CommandNode<TSource> node, StringReader originalReader, CommandContextBuilder<TSource> contextSoFar)
		{
			var source = contextSoFar.Source;
			IDictionary<CommandNode<TSource>, CommandSyntaxException>? errors = null;
			List<ParseResults<TSource>>? potentials = null;
			var cursor = originalReader.Cursor;

			foreach (var child in node.GetRelevantNodes(originalReader))
			{
				if (!child.CanUse(source))
				{
					continue;
				}

				var context = contextSoFar.Copy();
				var reader = new StringReader(originalReader);
				try
				{
					try
					{
						child.Parse(reader, context);
					}
					catch (CommandSyntaxException)
					{
						throw;
					}
					catch (Exception ex)
					{
						throw CommandSyntaxException.BuiltInExceptions.DispatcherParseException().CreateWithContext(reader, ex.Message);
					}

					if (reader.CanRead())
					{
						if (reader.Peek() != ArgumentSeparatorChar)
						{
							throw CommandSyntaxException.BuiltInExceptions.DispatcherExpectedArgumentSeparator().CreateWithContext(reader);
						}
					}
				}
				catch (CommandSyntaxException ex)
				{
					if (errors == null)
					{
						errors = new Dictionary<CommandNode<TSource>, CommandSyntaxException>();
					}

					errors.Add(child, ex);
					reader.Cursor = cursor;
					continue;
				}

				context.WithCommand(child.Command);
				if (reader.CanRead(child.Redirect == null ? 2 : 1))
				{
					reader.Skip();
					if (child.Redirect != null)
					{
						var childContext = new CommandContextBuilder<TSource>(this, source, child.Redirect, reader.Cursor);
						var parse = ParseNodes(child.Redirect, reader, childContext);
						context.WithChild(parse.Context);
						return new ParseResults<TSource>(context, parse.Reader, parse.Exceptions);
					}
					else
					{
						var parse = ParseNodes(child, reader, context);
						if (potentials == null)
						{
							potentials = new List<ParseResults<TSource>>(1);
						}

						potentials.Add(parse);
					}
				}
				else
				{
					if (potentials == null)
					{
						potentials = new List<ParseResults<TSource>>(1);
					}

					potentials.Add(new ParseResults<TSource>(context, reader, new Dictionary<CommandNode<TSource>, CommandSyntaxException>()));
				}
			}

			if (potentials != null)
			{
				if (potentials.Count > 1)
				{
					potentials.Sort((a, b) =>
					{
						if (!a.Reader.CanRead() && b.Reader.CanRead())
						{
							return -1;
						}

						if (a.Reader.CanRead() && !b.Reader.CanRead())
						{
							return 1;
						}

						if (a.Exceptions.Count == 0 && b.Exceptions.Count > 0)
						{
							return -1;
						}

						if (a.Exceptions.Count > 0 && b.Exceptions.Count == 0)
						{
							return 1;
						}

						return 0;
					});
				}

				return potentials[0];
			}

			return new ParseResults<TSource>(contextSoFar, originalReader, errors ?? new Dictionary<CommandNode<TSource>, CommandSyntaxException>());
		}

		/// <summary>Gets all possible executable commands following the given node.</summary>
		/// 
		/// <remarks>
		/// <para>You may use <see cref="Root" /> as a target to get all usage data for the entire command tree.</para>
		/// 
		/// <para>The returned syntax will be in "simple" form: <c>&lt;param&gt;</c> and <c>literal</c>. "Optional" nodes will be
		/// listed as multiple entries: the parent node, and the child nodes.
		/// For example, a required literal "foo" followed by an optional param "int" will be two nodes:</para>
		/// <list type="table">
		///     <item><c>foo</c></item>
		///     <item><c>foo &lt;int&gt;</c></item>
		/// </list>
		/// 
		/// <para>The path to the specified node will <b>not</b> be prepended to the output, as there can theoretically be many
		/// ways to reach a given node. It will only give you paths relative to the specified node, not absolute from root.</para>
		/// </remarks>
		/// 
		/// <param name="node">Target node to get child usage strings for.</param>
		/// <param name="source">A custom source object, usually representing the originator of this command.</param>
		/// <param name="restricted">If true, commands that the source cannot access will not be mentioned.</param>
		/// <returns>Array of full usage strings under the target node.</returns>
		public string[] GetAllUsage(CommandNode<TSource> node, TSource source, bool restricted)
		{
			var result = new List<string>();
			GetAllUsage(node, source, result, "", restricted);
			return result.ToArray();
		}

		private void GetAllUsage(CommandNode<TSource> node, TSource source, List<string> result, string prefix, bool restricted)
		{
			if (restricted && !node.CanUse(source))
			{
				return;
			}

			if (node.Command != null)
			{
				result.Add(prefix);
			}

			if (node.Redirect != null)
			{
				var redirect = node.Redirect == _root ? "..." : "-> " + node.Redirect.UsageText;
				result.Add(prefix.Length == 0 ? node.UsageText + ArgumentSeparator + redirect : prefix + ArgumentSeparator + redirect);
			}
			else if (node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					GetAllUsage(child, source, result, prefix.Length == 0 ? child.UsageText : prefix + ArgumentSeparator + child.UsageText, restricted);
				}
			}
		}

		/// <summary>Gets the possible executable commands from a specified node.</summary>
		/// 
		/// <remarks>
		/// <para>You may use <see cref="Root"/> as a target to get usage data for the entire command tree.</para>
		///
		/// <para>The returned syntax will be in "smart" form: <c>&lt;param&gt;</c>, <c>literal</c>, <c>[optional]</c> and <c>either|or</c>.
		/// These forms may be mixed and matched to provide as much information about the child nodes as it can, without being too verbose.
		/// For example, a required literal "foo" followed by an optional param "int" can be compressed into one string:</para>
		/// <list type="table">
		///		<item><c>foo [&lt;int&gt;]</c></item>
		/// </list>
		///
		/// <para>The path to the specified node will <b>not</b> be prepended to the output, as there can theoretically be many
		/// ways to reach a given node. It will only give you paths relative to the specified node, not absolute from root.</para>
		///
		/// <para>The returned usage will be restricted to only commands that the provided {@code source} can use.</para>
		/// </remarks>
		/// 
		/// <param name="node">Target node to get child usage strings for.</param>
		/// <param name="source">A custom source object, usually representing the originator of this command.</param>
		/// <returns>Dictionary mapping child CommandNode to its usage string.</returns>
		public IDictionary<CommandNode<TSource>, string> GetSmartUsage(CommandNode<TSource> node, TSource source)
		{
			IDictionary<CommandNode<TSource>, string> result = new Dictionary<CommandNode<TSource>, string>();

			var optional = node.Command != null;
			foreach (var child in node.Children)
			{
				var usage = GetSmartUsage(child, source, optional, false);
				if (usage != null)
				{
					result.Add(child, usage);
				}
			}

			return result;
		}

		private string? GetSmartUsage(CommandNode<TSource> node, TSource source, bool optional, bool deep)
		{
			if (!node.CanUse(source))
			{
				return null;
			}

			var self = optional ? UsageOptionalOpen + node.UsageText + UsageOptionalClose : node.UsageText;
			var childOptional = node.Command != null;
			var open = childOptional ? UsageOptionalOpen : UsageRequiredOpen;
			var close = childOptional ? UsageOptionalClose : UsageRequiredClose;

			if (!deep)
			{
				if (node.Redirect != null)
				{
					var redirect = node.Redirect == _root ? "..." : "-> " + node.Redirect.UsageText;
					return self + ArgumentSeparator + redirect;
				}
				else
				{
					var children = node.Children.Where(c => c.CanUse(source)).ToList();
					if (children.Count == 1)
					{
						var usage = GetSmartUsage(children.Single(), source, childOptional, childOptional);
						if (usage != null)
						{
							return self + ArgumentSeparator + usage;
						}
					}
					else if (children.Count > 1)
					{
						ISet<string> childUsage = new HashSet<string>();
						foreach (var child in children)
						{
							var usage = GetSmartUsage(child, source, childOptional, true);
							if (usage != null)
							{
								childUsage.Add(usage);
							}
						}

						if (childUsage.Count == 1)
						{
							var usage = childUsage.Single();
							return self + ArgumentSeparator + (childOptional ? UsageOptionalOpen + usage + UsageOptionalClose : usage);
						}
						else if (childUsage.Count > 1)
						{
							var builder = new StringBuilder(open);
							var count = 0;
							foreach (var child in children)
							{
								if (count > 0)
								{
									builder.Append(UsageOr);
								}

								builder.Append(child.UsageText);
								count++;
							}

							if (count > 0)
							{
								builder.Append(close);
								return self + ArgumentSeparator + builder;
							}
						}
					}
				}
			}

			return self;
		}

		/// <summary>Gets suggestions for a parsed input string on what comes next.</summary>
		/// 
		/// <remarks>
		/// <p>As it is ultimately up to custom argument types to provide suggestions, it may be an asynchronous operation,
		/// for example getting in-game data or player names etc. As such, this method returns a future and no guarantees
		/// are made to when or how the future completes.</p>
		///
		/// <p>The suggestions provided will be in the context of the end of the parsed input string, but may suggest
		/// new or replacement strings for earlier in the input string. For example, if the end of the string was
		/// <c>foobar</c> but an argument preferred it to be <c>minecraft:foobar</c>, it will suggest a replacement for that
		/// whole segment of the input.</p>
		/// </remarks>
		/// 
		/// <param name="parse">The result of a Parse(StringReader, Object).</param>
		/// <returns>A Task that will eventually resolve into a Suggestions object.</returns>
		public Task<Suggestions> GetCompletionSuggestions(ParseResults<TSource> parse)
		{
			return GetCompletionSuggestions(parse, parse.Reader.TotalLength);
		}

		public async Task<Suggestions> GetCompletionSuggestions(ParseResults<TSource> parse, int cursor)
		{
			var context = parse.Context;

			var nodeBeforeCursor = context.FindSuggestionContext(cursor);
			var parent = nodeBeforeCursor.Parent;
			var start = Math.Min(nodeBeforeCursor.StartPos, cursor);

			var fullInput = parse.Reader.String;
			var truncatedInput = fullInput.Substring(0, cursor);
            var truncatedInputLowerCase = truncatedInput.ToLowerInvariant();
			var futures = new Task<Suggestions>[parent.Children.Count()];
			var i = 0;
			foreach (var node in parent.Children)
			{
				var future = Suggestions.Empty();
				try
				{
					future = node.ListSuggestions(context.Build(truncatedInput), new SuggestionsBuilder(truncatedInput, truncatedInputLowerCase, start));
				}
				catch (CommandSyntaxException)
				{
				}

				futures[i++] = future;
			}

			await Task.WhenAll(futures);
			
			return Suggestions.Merge(fullInput, futures.Select(s => s.Result).ToArray());
		}

		/// <summary>Gets the root of this command tree.</summary>
		/// 
		/// <remarks>
		/// <para>This is often useful as a target of a <see cref="ArgumentBuilder{TSource,TThis,TNode}.Redirect(CommandNode{TSource})"/>,
		/// <see cref="GetAllUsage" /> or <see cref="GetSmartUsage" />.
		/// You may also use it to clone the command tree via <see cref="CommandDispatcher(RootCommandNode{TSource})"/>.</para>
		/// </remarks>
		/// 
		/// <returns>Root of the command tree.</returns>
		public RootCommandNode<TSource> Root => _root;

		/// <summary>Finds a valid path to a given node on the command tree.</summary>
		/// 
		/// <remarks>
		/// <para>There may theoretically be multiple paths to a node on the tree, especially with the use of forking or redirecting.
		/// As such, this method makes no guarantees about which path it finds. It will not look at forks or redirects,
		/// and find the first instance of the target node on the tree.</para>
		///
		/// <para>The only guarantee made is that for the same command tree and the same version of this library, the result of
		/// this method will <b>always</b> be a valid input for <see cref="FindNode" />, which should return the same node
		/// as provided to this method.</para>
		/// </remarks>
		/// 
		/// <param name="target">The target node to find a path for.</param>
		/// <returns>A path to the resulting node, or an empty list if it was not found.</returns>
		public List<string> GetPath(CommandNode<TSource> target)
		{
			var nodes = new List<List<CommandNode<TSource>>>();
			AddPaths(_root, nodes, new List<CommandNode<TSource>>());

			foreach (var list in nodes)
			{
				if (list[list.Count - 1] == target)
				{
					var result = new List<string>(list.Count);
					foreach (var node in list)
					{
						if (node != _root)
						{
							result.Add(node.Name);
						}
					}

					return result;
				}
			}

			return new List<string>();
		}

		/// <summary>Finds a node by its path.</summary>
		///
		/// <remarks>
		/// <p>Paths may be generated with <see cref="GetPath"/>, and are guaranteed (for the same tree, and the
		/// same version of this library) to always produce the same valid node by this method.</p>
		///
		/// <p>If a node could not be found at the specified path, then <c>null</c> will be returned.</p>
		/// </remarks>
		/// 
		/// <param name="path">A generated path to a node.</param>
		/// <returns>The node at the given path, or null if not found.</returns>
		public CommandNode<TSource>? FindNode(IEnumerable<string> path)
		{
			CommandNode<TSource>? node = _root;
			foreach (var name in path)
			{
				node = node.GetChild(name);
				if (node == null)
				{
					return null;
				}
			}

			return node;
		}

		/// <summary>Scans the command tree for potential ambiguous commands.</summary>
		/// 
		/// <remarks>
		/// <p>This is a shortcut for <see cref="CommandNode{TSource}.FindAmbiguities"/> on <see cref="Root" />.</p>
		///
		/// <p>Ambiguities are detected by testing every <see cref="CommandNode{TSource}.Examples" /> on one node verses every sibling
		/// node. This is not fool-proof, and relies a lot on the providers of the used argument types to give good examples.</p>
		/// </remarks>
		/// 
		/// <param name="consumer">A callback to be notified of potential ambiguities.</param>
		public void FindAmbiguities(AmbiguityConsumer<TSource> consumer)
		{
			_root.FindAmbiguities(consumer);
		}

		private void AddPaths(CommandNode<TSource> node, List<List<CommandNode<TSource>>> result, List<CommandNode<TSource>> parents)
		{
			var current = new List<CommandNode<TSource>>(parents)
			{
				node
			};
			result.Add(current);

			foreach (var child in node.Children)
			{
				AddPaths(child, result, current);
			}
		}
	}
}