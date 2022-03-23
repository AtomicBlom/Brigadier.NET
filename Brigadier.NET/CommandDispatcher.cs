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
		/// <see cref="ArgumentSeparatorChar"/>
		public string ArgumentSeparator = " ";

		/// <summary>
		/// The char required to separate individual arguments in an input string
		/// </summary>
		/// <see cref="ArgumentSeparator"/>
		public char ArgumentSeparatorChar = ' ';

		private const string UsageOptionalOpen = "[";
		private const string UsageOptionalClose = "]";
		private const string UsageRequiredOpen = "(";
		private const string UsageRequiredClose = ")";
		private const string UsageOr = "|";

		private readonly RootCommandNode<TSource> _root;

		private readonly Predicate<CommandNode<TSource>> _hasCommand;

		/**
		 * Sets a callback to be informed of the result of every command.
		 *
		 * @param consumer the new result consumer to be called
		 */
		private ResultConsumer<TSource> Consumer { get; } = (c, s, r) => { };
		
		/**
		 * Create a new {@link CommandDispatcher} with the specified root node.
		 *
		 * <p>This is often useful to copy existing or pre-defined command trees.</p>
		 *
		 * @param root the existing {@link RootCommandNode} to use as the basis for this tree
		 */
		public CommandDispatcher(RootCommandNode<TSource> root)
		{
			_root = root;
			_hasCommand = input => input != null && (input.Command != null || input.Children.Any(c => _hasCommand(c)));
		}

		/**
		 * Creates a new {@link CommandDispatcher} with an empty command tree.
		 */
		public CommandDispatcher()
			: this(new RootCommandNode<TSource>())
		{
		}

		/**
		 * Utility method for registering new commands.
		 *
		 * <p>This is a shortcut for calling {@link RootCommandNode#addChild(CommandNode)} after building the provided {@code command}.</p>
		 *
		 * <p>As {@link RootCommandNode} can only hold literals, this method will only allow literal arguments.</p>
		 *
		 * @param command a literal argument builder to add to this command tree
		 * @return the node added to this tree
		 */
		public LiteralCommandNode<TSource> Register(LiteralArgumentBuilder<TSource> command)
		{
			var build = command.Build();
			_root.AddChild(build);
			return build;
		}

		/**
		 * Utility method for registering new commands.
		 *
		 * <p>This is a shortcut for calling {@link RootCommandNode#addChild(CommandNode)} after building the provided {@code command}.</p>
		 *
		 * <p>As {@link RootCommandNode} can only hold literals, this method will only allow literal arguments.</p>
		 *
		 * @param command a literal argument builder to add to this command tree
		 * @return the node added to this tree
		 */
		public LiteralCommandNode<TSource> Register(Func<IArgumentContext<TSource>, LiteralArgumentBuilder<TSource>> command)
		{
			var build = command(default(ArgumentContext<TSource>)).Build();
			_root.AddChild(build);
			return build;
		}

		/**
		 * Parses and executes a given command.
		 *
		 * <p>This is a shortcut to first {@link #parse(StringReader, Object)} and then {@link #execute(ParseResults)}.</p>
		 *
		 * <p>It is recommended to parse and execute as separate steps, as parsing is often the most expensive step, and easiest to cache.</p>
		 *
		 * <p>If this command returns a value, then it successfully executed something. If it could not parse the command, or the execution was a failure,
		 * then an exception will be thrown. Most exceptions will be of type {@link CommandSyntaxException}, but it is possible that a {@link RuntimeException}
		 * may bubble up from the result of a command. The meaning behind the returned result is arbitrary, and will depend
		 * entirely on what command was performed.</p>
		 *
		 * <p>If the command passes through a node that is {@link CommandNode#isFork()} then it will be 'forked'.
		 * A forked command will not bubble up any {@link CommandSyntaxException}s, and the 'result' returned will turn into
		 * 'amount of successful commands executes'.</p>
		 *
		 * <p>After each and any command is ran, a registered callback given to {@link #setConsumer(ResultConsumer)}
		 * will be notified of the result and success of the command. You can use that method to gather more meaningful
		 * results than this method will return, especially when a command forks.</p>
		 *
		 * @param input a command string to parse &amp; execute
		 * @param source a custom "source" object, usually representing the originator of this command
		 * @return a numeric result from a "command" that was performed
		 * @throws CommandSyntaxException if the command failed to parse or execute
		 * @throws RuntimeException if the command failed to execute and was not handled gracefully
		 * @see #parse(String, Object)
		 * @see #parse(StringReader, Object)
		 * @see #execute(ParseResults)
		 * @see #execute(StringReader, Object)
		 */
		/// <exception cref="CommandSyntaxException"></exception>
		public int Execute(string input, TSource source)
		{
			return Execute(new StringReader(input), source);
		}

		/**
		 * Parses and executes a given command.
		 *
		 * <p>This is a shortcut to first {@link #parse(StringReader, Object)} and then {@link #execute(ParseResults)}.</p>
		 *
		 * <p>It is recommended to parse and execute as separate steps, as parsing is often the most expensive step, and easiest to cache.</p>
		 *
		 * <p>If this command returns a value, then it successfully executed something. If it could not parse the command, or the execution was a failure,
		 * then an exception will be thrown. Most exceptions will be of type {@link CommandSyntaxException}, but it is possible that a {@link RuntimeException}
		 * may bubble up from the result of a command. The meaning behind the returned result is arbitrary, and will depend
		 * entirely on what command was performed.</p>
		 *
		 * <p>If the command passes through a node that is {@link CommandNode#isFork()} then it will be 'forked'.
		 * A forked command will not bubble up any {@link CommandSyntaxException}s, and the 'result' returned will turn into
		 * 'amount of successful commands executes'.</p>
		 *
		 * <p>After each and any command is ran, a registered callback given to {@link #setConsumer(ResultConsumer)}
		 * will be notified of the result and success of the command. You can use that method to gather more meaningful
		 * results than this method will return, especially when a command forks.</p>
		 *
		 * @param input a command string to parse &amp; execute
		 * @param source a custom "source" object, usually representing the originator of this command
		 * @return a numeric result from a "command" that was performed
		 * @throws CommandSyntaxException if the command failed to parse or execute
		 * @throws RuntimeException if the command failed to execute and was not handled gracefully
		 * @see #parse(String, Object)
		 * @see #parse(StringReader, Object)
		 * @see #execute(ParseResults)
		 * @see #execute(String, Object)
		 */
		/// <exception cref="CommandSyntaxException" />
		public int Execute(StringReader input, TSource source)
		{
			var parse = Parse(input, source);
			return Execute(parse);
		}

		/**
		 * Executes a given pre-parsed command.
		 *
		 * <p>If this command returns a value, then it successfully executed something. If the execution was a failure,
		 * then an exception will be thrown.
		 * Most exceptions will be of type {@link CommandSyntaxException}, but it is possible that a {@link RuntimeException}
		 * may bubble up from the result of a command. The meaning behind the returned result is arbitrary, and will depend
		 * entirely on what command was performed.</p>
		 *
		 * <p>If the command passes through a node that is {@link CommandNode#isFork()} then it will be 'forked'.
		 * A forked command will not bubble up any {@link CommandSyntaxException}s, and the 'result' returned will turn into
		 * 'amount of successful commands executes'.</p>
		 *
		 * <p>After each and any command is ran, a registered callback given to {@link #setConsumer(ResultConsumer)}
		 * will be notified of the result and success of the command. You can use that method to gather more meaningful
		 * results than this method will return, especially when a command forks.</p>
		 *
		 * @param parse the result of a successful {@link #parse(StringReader, Object)}
		 * @return a numeric result from a "command" that was performed.
		 * @throws CommandSyntaxException if the command failed to parse or execute
		 * @throws RuntimeException if the command failed to execute and was not handled gracefully
		 * @see #parse(String, Object)
		 * @see #parse(StringReader, Object)
		 * @see #execute(String, Object)
		 * @see #execute(StringReader, Object)
		 */
		///<exception cref="CommandSyntaxException" />
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

			var result = 0;
			var successfulForks = 0;
			var forked = false;
			var foundCommand = false;
			var command = parse.Reader.String;
			var original = parse.Context.Build(command);
			var contexts = new List<CommandContext<TSource>> {original};
			List<CommandContext<TSource>> next = null;

			while (contexts != null)
			{
				var size = contexts.Count;
				for (var i = 0; i < size; i++)
				{
					var context = contexts[i];
					var child = context.Child;
					if (child != null)
					{
						forked |= context.IsForked();
						if (child.HasNodes())
						{
							foundCommand = true;
							var modifier = context.RedirectModifier;
							if (modifier == null)
							{
								if (next == null)
								{
									next = new List<CommandContext<TSource>>(1);
								}

								next.Add(child.CopyFor(context.Source));
							}
							else
							{
								try
								{
									var results = modifier(context);
									if (results.Count > 0)
									{
										if (next == null)
										{
											next = new List<CommandContext<TSource>>(results.Count());
										}

										foreach (var source in results)
										{
											next.Add(child.CopyFor(source));
										}
									}
								}
								catch (CommandSyntaxException)
								{
									Consumer(context, false, 0);
									if (!forked)
									{
										throw;
									}
								}
							}
						}
					}
					else if (context.Command != null)
					{
						foundCommand = true;
						try
						{
							var value = context.Command(context);
							result += value;
							Consumer(context, true, value);
							successfulForks++;
						}
						catch (CommandSyntaxException)
						{
							Consumer(context, false, 0);
							if (!forked)
							{
								throw;
							}
						}
					}
				}

				contexts = next;
				next = null;
			}

			if (!foundCommand)
			{
				Consumer(original, false, 0);
				throw CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand().CreateWithContext(parse.Reader);
			}

			return forked ? successfulForks : result;
		}

		/**
		 * Parses a given command.
		 *
		 * <p>The result of this method can be cached, and it is advised to do so where appropriate. Parsing is often the
		 * most expensive step, and this allows you to essentially "precompile" a command if it will be ran often.</p>
		 *
		 * <p>If the command passes through a node that is {@link CommandNode#isFork()} then the resulting context will be marked as 'forked'.
		 * Forked contexts may contain child contexts, which may be modified by the {@link RedirectModifier} attached to the fork.</p>
		 *
		 * <p>Parsing a command can never fail, you will always be provided with a new {@link ParseResults}.
		 * However, that does not mean that it will always parse into a valid command. You should inspect the returned results
		 * to check for validity. If its {@link ParseResults#getReader()} {@link StringReader#canRead()} then it did not finish
		 * parsing successfully. You can use that position as an indicator to the user where the command stopped being valid.
		 * You may inspect {@link ParseResults#getExceptions()} if you know the parse failed, as it will explain why it could
		 * not find any valid commands. It may contain multiple exceptions, one for each "potential node" that it could have visited,
		 * explaining why it did not go down that node.</p>
		 *
		 * <p>When you eventually call {@link #execute(ParseResults)} with the result of this method, the above error checking
		 * will occur. You only need to inspect it yourself if you wish to handle that yourself.</p>
		 *
		 * @param command a command string to parse
		 * @param source a custom "source" object, usually representing the originator of this command
		 * @return the result of parsing this command
		 * @see #parse(StringReader, Object)
		 * @see #execute(ParseResults)
		 * @see #execute(String, Object)
		 */
		public ParseResults<TSource> Parse(string command, TSource source)
		{
			return Parse(new StringReader(command), source);
		}

		/**
		 * Parses a given command.
		 *
		 * <p>The result of this method can be cached, and it is advised to do so where appropriate. Parsing is often the
		 * most expensive step, and this allows you to essentially "precompile" a command if it will be ran often.</p>
		 *
		 * <p>If the command passes through a node that is {@link CommandNode#isFork()} then the resulting context will be marked as 'forked'.
		 * Forked contexts may contain child contexts, which may be modified by the {@link RedirectModifier} attached to the fork.</p>
		 *
		 * <p>Parsing a command can never fail, you will always be provided with a new {@link ParseResults}.
		 * However, that does not mean that it will always parse into a valid command. You should inspect the returned results
		 * to check for validity. If its {@link ParseResults#getReader()} {@link StringReader#canRead()} then it did not finish
		 * parsing successfully. You can use that position as an indicator to the user where the command stopped being valid.
		 * You may inspect {@link ParseResults#getExceptions()} if you know the parse failed, as it will explain why it could
		 * not find any valid commands. It may contain multiple exceptions, one for each "potential node" that it could have visited,
		 * explaining why it did not go down that node.</p>
		 *
		 * <p>When you eventually call {@link #execute(ParseResults)} with the result of this method, the above error checking
		 * will occur. You only need to inspect it yourself if you wish to handle that yourself.</p>
		 *
		 * @param command a command string to parse
		 * @param source a custom "source" object, usually representing the originator of this command
		 * @return the result of parsing this command
		 * @see #parse(String, Object)
		 * @see #execute(ParseResults)
		 * @see #execute(String, Object)
		 */
		public ParseResults<TSource> Parse(StringReader command, TSource source)
		{
			var context = new CommandContextBuilder<TSource>(this, source, _root, command.Cursor);
			return ParseNodes(_root, command, context);
		}

		private ParseResults<TSource> ParseNodes(CommandNode<TSource> node, StringReader originalReader, CommandContextBuilder<TSource> contextSoFar)
		{
			var source = contextSoFar.Source;
			IDictionary<CommandNode<TSource>, CommandSyntaxException> errors = null;
			List<ParseResults<TSource>> potentials = null;
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

		/**
		 * Gets all possible executable commands following the given node.
		 *
		 * <p>You may use {@link #getRoot()} as a target to get all usage data for the entire command tree.</p>
		 *
		 * <p>The returned syntax will be in "simple" form: {@code <param>} and {@code literal}. "Optional" nodes will be
		 * listed as multiple entries: the parent node, and the child nodes.
		 * For example, a required literal "foo" followed by an optional param "int" will be two nodes:</p>
		 * <ul>
		 *     <li>{@code foo}</li>
		 *     <li>{@code foo <int>}</li>
		 * </ul>
		 *
		 * <p>The path to the specified node will <b>not</b> be prepended to the output, as there can theoretically be many
		 * ways to reach a given node. It will only give you paths relative to the specified node, not absolute from root.</p>
		 *
		 * @param node target node to get child usage strings for
		 * @param source a custom "source" object, usually representing the originator of this command
		 * @param restricted if true, commands that the {@code source} cannot access will not be mentioned
		 * @return array of full usage strings under the target node
		 */
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

/**
 * Gets the possible executable commands from a specified node.
 *
 * <p>You may use {@link #getRoot()} as a target to get usage data for the entire command tree.</p>
 *
 * <p>The returned syntax will be in "smart" form: {@code <param>}, {@code literal}, {@code [optional]} and {@code (either|or)}.
 * These forms may be mixed and matched to provide as much information about the child nodes as it can, without being too verbose.
 * For example, a required literal "foo" followed by an optional param "int" can be compressed into one string:</p>
 * <ul>
 *     <li>{@code foo [<int>]}</li>
 * </ul>
 *
 * <p>The path to the specified node will <b>not</b> be prepended to the output, as there can theoretically be many
 * ways to reach a given node. It will only give you paths relative to the specified node, not absolute from root.</p>
 *
 * <p>The returned usage will be restricted to only commands that the provided {@code source} can use.</p>
 *
 * @param node target node to get child usage strings for
 * @param source a custom "source" object, usually representing the originator of this command
 * @return array of full usage strings under the target node
 */
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

		private string GetSmartUsage(CommandNode<TSource> node, TSource source, bool optional, bool deep)
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

/**
 * Gets suggestions for a parsed input string on what comes next.
 *
 * <p>As it is ultimately up to custom argument types to provide suggestions, it may be an asynchronous operation,
 * for example getting in-game data or player names etc. As such, this method returns a future and no guarantees
 * are made to when or how the future completes.</p>
 *
 * <p>The suggestions provided will be in the context of the end of the parsed input string, but may suggest
 * new or replacement strings for earlier in the input string. For example, if the end of the string was
 * {@code foobar} but an argument preferred it to be {@code minecraft:foobar}, it will suggest a replacement for that
 * whole segment of the input.</p>
 *
 * @param parse the result of a {@link #parse(StringReader, Object)}
 * @return a future that will eventually resolve into a {@link Suggestions} object
 */
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

		/**
		 * Gets the root of this command tree.
		 *
		 * <p>This is often useful as a target of a {@link com.mojang.brigadier.builder.ArgumentBuilder#redirect(CommandNode)},
		 * {@link #getAllUsage(CommandNode, Object, bool)} or {@link #getSmartUsage(CommandNode, Object)}.
		 * You may also use it to clone the command tree via {@link #CommandDispatcher(RootCommandNode)}.</p>
		 *
		 * @return root of the command tree
		 */
		public RootCommandNode<TSource> GetRoot()
		{
			return _root;
		}

/**
 * Finds a valid path to a given node on the command tree.
 *
 * <p>There may theoretically be multiple paths to a node on the tree, especially with the use of forking or redirecting.
 * As such, this method makes no guarantees about which path it finds. It will not look at forks or redirects,
 * and find the first instance of the target node on the tree.</p>
 *
 * <p>The only guarantee made is that for the same command tree and the same version of this library, the result of
 * this method will <b>always</b> be a valid input for {@link #findNode(Collection)}, which should return the same node
 * as provided to this method.</p>
 *
 * @param target the target node you are finding a path for
 * @return a path to the resulting node, or an empty list if it was not found
 */
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

/**
 * Finds a node by its path
 *
 * <p>Paths may be generated with {@link #getPath(CommandNode)}, and are guaranteed (for the same tree, and the
 * same version of this library) to always produce the same valid node by this method.</p>
 *
 * <p>If a node could not be found at the specified path, then {@code null} will be returned.</p>
 *
 * @param path a generated path to a node
 * @return the node at the given path, or null if not found
 */
		public CommandNode<TSource> FindNode(IEnumerable<string> path)
		{
			CommandNode<TSource> node = _root;
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

/**
 * Scans the command tree for potential ambiguous commands.
 *
 * <p>This is a shortcut for {@link CommandNode#findAmbiguities(AmbiguityConsumer)} on {@link #getRoot()}.</p>
 *
 * <p>Ambiguities are detected by testing every {@link CommandNode#getExamples()} on one node verses every sibling
 * node. This is not fool proof, and relies a lot on the providers of the used argument types to give good examples.</p>
 *
 * @param consumer a callback to be notified of potential ambiguities
 */
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