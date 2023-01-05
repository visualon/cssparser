[![build](https://github.com/visualon/cssparser/actions/workflows/build.yml/badge.svg)](https://github.com/visualon/cssparser/actions/workflows/build.yml)
[![VisualOn.CSSParser](https://img.shields.io/nuget/v/VisualOn.CSSParser?label=VisualOn.CSSParser)](https://www.nuget.org/packages/VisualOn.CssParser)
[![VisualOn.CSSParser.ExtendedLESSParser](https://img.shields.io/nuget/v/VisualOn.CSSParser.ExtendedLESSParser?label=VisualOn.CSSParser.ExtendedLESSParser)](https://www.nuget.org/packages/VisualOn.CssParser.ExtendedLESSParser)
[![codecov](https://codecov.io/gh/visualon/cssparser/branch/main/graph/badge.svg?token=EEGfq3zpqS)](https://codecov.io/gh/visualon/cssparser)
![LICENSE](https://img.shields.io/github/license/visualon/cssparser)

# CSS Parser 

This is a fork from [Dan Roberts](https://bitbucket.org/DanRoberts/cssparser).

A simple CSS and LESS parser to categorise strings of content and optionally generate a hierarchical representation of it.

## Changes
See [Releases](https://github.com/visualon/cssparser/releases)

## License
[MIT License](LICENSE.txt)


### Howto

This was written to fulfill a need I had to run quickly through a string of CSS and identify what "type" each character was. With an optional ability to parse it into hierarchical data describing nested selectors and/or media queries (most applicable to LESS rather than vanilla CSS since LESS supports nesting of selectors whereas CSS only supports single nesting of a selector within a media query).

    IEnumerable<CategorisedCharacterString> ParseCSS(string content)

and

    IEnumerable<CategorisedCharacterString> ParseLESS(string content)

(within the static class **CSSParser.Parser**) do the basic parsing work where **CategorisedCharacterString** has the properties

    public class CategorisedCharacterString
    {
      /// <summary>
      /// This will never be null or an empty string
      /// </summary>
      public string Value { get; }

      /// <summary>
      /// This is the location of the start of the string in the source data
      /// </summary>
      public int IndexInSource { get; }

      public CharacterCategorisationOptions CharacterCategorisation { get; }
    }

    public enum CharacterCategorisationOptions
    {
      Comment,
      CloseBrace,
      OpenBrace,
      SemiColon,
      SelectorOrStyleProperty,
      StylePropertyColon,
      Value,
      Whitespace
    }

so calling ParseCSS on

    /* Test */ .Content { color: black; }

will return **CategorisedCharacterString** instances with the data

    "/* Test */"     Comment (IndexInSource 0)
    " "              Whitespace (IndexInSource 10)
    ".Content"       SelectorOrStyleProperty (IndexInSource 11)
    " "              Whitespace (IndexInSource 19)
    "{"              OpenBrace (IndexInSource 20)
    " "              Whitespace (IndexInSource 19)
    "color"          SelectorOrStyleProperty (IndexInSource 22)
    ":"              StylePropertyColon (IndexInSource 27)
    " "              Whitespace (IndexInSource 28)
    "black"          Value (IndexInSource 29)
    ";"              SemiColon (IndexInSource 34)
    " "              Whitespace (IndexInSource 35)
    "}"              CloseBrace (IndexInSource 36)

This analysis can be done very cheaply as it is only a very simple representation. It does not, for example, differentiate between the type of the ".Content" value or "color", they are both considered to be of type **SelectorOrStyleProperty**.

To arrange in a hierchical manner and to categorise more strictly, the data return from ParseCSS or ParseLess can be passed into

    IEnumerable<ICSSFragment> ParseIntoStructuredData(
      IEnumerable<CategorisedCharacterString> segments
    )

(within the static class **CSSParser.ExtendedLESSParser.LessCssHierarchicalParser**) which transforms the data again. The interface **ICSSFragment** is implemented by the classes **Import** (describing an import statement for another stylesheet), a **MediaQuery** or a **Selector** (both of which have a "ChildFragments" set as they may contain other media queries, selectors and/or properties), a **StylePropertyName** or **StylePropertyValue**. Note that comments and whitespace are not included in this data.

So content such as

    // Example
    html
    {
      h1
      {
        color: black;
        background: white url("background.jpg") no-repeat top left;
      }
      p.Intro { padding: 8px; }
    }

becomes something like

    html
      h1
        color
          black
        background
          white
          url("background.jpg")
          no-repat
          top
          left
      p.Intro
        padding
          8px

*(Above: "html" represent a Selector instance with a ChildFragments property containing Selector instances for the "h1" and "p", each with ChildFragments data made up of StylePropertyValue and StylePropertyValue instances).*
