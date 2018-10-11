# Brigadier [![Build Status](https://dev.azure.com/atomicblom/Brigadier.NET/_apis/build/status/Master%20Build)](https://dev.azure.com/atomicblom/Brigadier.NET/_build/latest?definitionId=3) [![Latest release](https://img.shields.io/github/release/atomicblom/Brigadier.NET.svg)](https://github.com/atomicblom/Brigadier.NET/releases/latest) [![License](https://img.shields.io/github/license/atomicblom/Brigadier.NET.svg)](https://github.com/atomicblom/Brigadier.NET/blob/master/LICENSE)

Brigadier.NET is a port of Mojang's [brigadier](https://github.com/mojang/brigadier) command line parser & dispatcher, originally for Minecraft: Java Edition and now freely available for use elsewhere under the MIT license.

This port is not supported by Mojang or Microsoft. For any issues, please report to [AtomicBlom/Brigadier.NET](https://www.github.com/AtomicBlom/Brigadier.NET/issues)

# Installation
<!--Brigadier.NET is available from NuGet, install it via the [BrigadierNET](https://nuget.org/) NuGet package-->
A NuGet Package will be provided shortly

# Contributing
Contributions are welcome ! :D

# Usage
At the heart of Brigadier, you need a `CommandDispatcher<TSource>`, where `<TSource>` is any custom object you choose to identify a "command source".

A command dispatcher holds a "command tree", which are a series of `CommandNode<TSource>` which represent the various possible syntax that form a valid command.

## Registering a new command
Before we can start parsing and dispatching commands, we need to build up our command tree. Every registration is an append operation,
so you can freely extend existing commands in a project without needing access to the source code that created them.

Command registration also encourages use of a builder pattern to keep code cruft to a minimum.

A "command" is a fairly loose term, but typically it means an exit point of the command tree.
Every node can have an `Executes` method attached to it, which signifies that if the input stops here then this function will be called with the context so far.

Consider the following example:
```csharp
var dispatcher = new CommandDispatcher<CommandSourceStack>();

dispatcher.Register(l =>
    l.Literal("foo")
        .Then(a =>
            a.Argument("bar", Arguments.Integer())
                .Executes(c => {
                    Console.WriteLine("Bar is " + Arguments.GetInteger(c, "bar"));
                    return 1;
                })
        )
        .Executes(c => {
            Console.WriteLine("Called foo with no arguments");
            return 1;
        })
);
``` 

This snippet registers two "commands": `foo` and `foo <bar>`. It is also common to refer to the `<bar>` as a "subcommand" of `foo`, as it's a child node.

At the start of the tree is a "root node", and it **must** have `LiteralCommandNode`s as children. Here, we register one command under the root: `literal("foo")`, which means "the user must type the literal string 'foo'".

Under that is two extra definitions: a child node for possible further evaluation, or an `Executes` block if the user input stops here.

The child node works exactly the same way, but is no longer limited to literals. The other type of node that is now allowed is an `ArgumentCommandNode`, which takes in a name and an argument type.

Arguments can be anything, and you are encouraged to build your own for seamless integration into your own product. There are some standard arguments included in brigadier, such as `IntegerArgumentType`.

Argument types will be asked to parse input as much as they can, and then store the "result" of that argument however they see fit or throw a relevant error if they can't parse.

For example, an integer argument would parse "123" and store it as `123` (`int`), but throw an error if the input were `onetwothree`.

When a command is actually run, it can access these arguments in the context provided to the registered function.

## Parsing user input
So, we've registered some commands and now we're ready to take in user input. If you're in a rush, you can just call `dispatcher.Execute("foo 123", source)` and call it a day.

The result of `Execute` is an integer returned by the command it evaluated. Its meaning varies depending on command, and typically will not be useful to programmers.

The `source` is an object of `<TSource>`, your own custom class to track users/players/etc. It will be provided to the command so that it has some context on what's happening.

If the command failed or could not parse, some form of `CommandSyntaxException` will be thrown. It is also possible for other kinds of `Exception` to be bubbled up, if not properly handled in a command.

If you wish to have more control over the parsing & executing of commands, or wish to cache the parse results so you can execute it multiple times, you can split it up into two steps:

```csharp
var parse = dispatcher.Parse("foo 123", source);
var result = dispatcher.Execute(parse);
``` 

This is highly recommended as the parse step is the most expensive, and may be easily cached depending on your application.

You can also use this to do further introspection on a command, before (or without) actually running it.

## Inspecting a command
If you `Parse` some input, you can find out what it will perform (if anything) and provide hints to the user safely and immediately.

The parse will never fail, and the `ParseResults<TSource>` it returns will contain a *possible* context that a command may be called with
(and from that, you can inspect which nodes the user entered, complete with start/end positions in the input string).
It also contains a map of parse exceptions for each command node it encountered. If it couldn't build a valid context, then
the reason why is inside this exception map.

## Displaying usage info
There are two forms of "usage strings" provided by this library, both require a target node.

`GetAllUsage(node, source, restricted)`  will return a list of all possible commands (executable end-points) under the target node and their human readable path. If `restricted`, it will ignore commands that `source` does not have access to. This will look like [`foo`, `foo <bar>`]

`GetSmartUsage(node, source)` will return a map of the child nodes to their "smart usage" human readable path. This tries to squash future-nodes together and show optional & typed information, and can look like `foo (<bar>)`

## Differences with Java Version
The current version of Brigadier.NET is based on the source code of brigadier at this [commit](https://github.com/Mojang/brigadier/commit/7ee589b29b7c72c423c1549f1edcb5c89981291a)

Changes have been made to bring the project to a more .NET feel, or to improve the simplicity of using the project in a .NET ecosystem

### Registering commands
.NET's Generics are limited in where it can infer type parameters from. We can make it easier to infer the type of TSource by providing a context in the form of a lambda.
```
var dispatcher = new CommandDispatcher<CommandSourceStack>();

dispatcher.Register(l => l.Literal("foo"));
```

The lambda can also be used to get the TSource for many argument builders.

```
dispatcher.Register(l => 
    l.Literal("foo")
        .Then(
            l.Argument("bar", Integer())
        )
);
```

Without the lambda, you will be forced to specify the generic type parameters manually.
```
dispatcher.Register(
    LiteralArgumentBuilder<CommandSourceStack>.LiteralArgument("foo")
        .Then(
            RequiredArgumentBuilder<CommandSourceStack, int>.RequiredArgument("bar", Integer())
        )
);
```

### Arguments
Arguments static methods have been renamed and moved to a single static class
* com.mojang.brigadier.arguments.BoolArgumentType.bool() 
  * -> Brigadier.NET.Arguments.Bool()
* com.mojang.brigadier.arguments.DoubleArgumentType.doubleArg() 
    * -> Brigadier.NET.Arguments.Double()
* com.mojang.brigadier.arguments.FloatArgumentType.floatArg() 
    * -> Brigadier.NET.Arguments.Float()
* com.mojang.brigadier.arguments.IntegerArgumentType.integer() 
    * -> Brigadier.NET.Arguments.Integer()
* com.mojang.brigadier.arguments.LongArgumentType.longArg() 
    * -> Brigadier.NET.Arguments.Long()
* com.mojang.brigadier.arguments.StringArgumentType.word() 
    * -> Brigadier.NET.Arguments.Word()
* com.mojang.brigadier.arguments.StringArgumentType.string()
    * -> Brigadier.NET.Arguments.String()
* com.mojang.brigadier.arguments.StringArgumentType.greedyString()
    * -> Brigadier.NET.Arguments.GreedyString()

You can import arguments easier in .NET by importing the static members of `Brigadier.NET.Arguments`
```csharp
using static Brigadier.NET.Arguments

...

dispatcher.Register(
    l => l.Literal("foo")
        .Then(a => a.Argument("bar", Integer()))
        .Then(a => a.Argument("fizz", Word()))
        .Then(a => a.Argument("buzz", Boolean()))
);

```

![GitHub forks](https://img.shields.io/github/forks/AtomicBlom/Brigadier.NET.svg?style=social&label=Fork) ![GitHub stars](https://img.shields.io/github/stars/AtomicBlom/Brigadier.NET.svg?style=social&label=Stars)