using System;
using CSSParser;
using CSSRenderers;
using CSSParser.ExtendedLESSParser;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{
            var value = "   \n\n/* Test */\n.Content .Woo {\n  color: \"black and white\";\n  @media screen and (max-width: 70em) {\n    h2 { background: white; }\n  }\n}\n";
            var renderContent =
                new PrettyPrintStyleHtmlRenderer().Render(
                    Parser.ParseCSS(value)
                );

            Console.WriteLine(value);
            Console.WriteLine(renderContent);
            Console.ReadLine();
		}
	}
}
