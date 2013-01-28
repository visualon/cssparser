using System;
using CSSParser.ContentProcessors.CharacterProcessors;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;
using CSSParser.ContentProcessors.StringProcessors;
using CSSParser.StringNavigators;
using CSSRenderers;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			var processorFactory = new CachingCharacterProcessorsFactory(
				new CharacterProcessorsFactory()
			);
			Func<string, string> Processor = content =>
			{
				return (new PrettyPrintStyleHtmlRenderer()).Render(
					(new ProcessedCharactersGrouper()).GetStrings(
						new StringNavigator(content),
						processorFactory.Get<SelectorOrStylePropertySegment>(processorFactory)
					)
				);
			};

			var value = "   \n\n/* Test */\n.Content .Woo {\n  color: \"black and white\";\n  @media screen and (max-width: 70em) {\n    h2 { background: white; }\n  }\n}\n";

			Console.WriteLine(value);
			Console.WriteLine(Processor(value));
			Console.ReadLine();
		}
	}
}
