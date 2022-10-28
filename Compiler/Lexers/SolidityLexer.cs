﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using Phantasma.Core.Cryptography;
using Phantasma.Core.Numerics;
using Phantasma.Tomb.AST;

namespace Phantasma.Tomb.Lexers
{
    public class SolidityLexer: Lexer
    {
        private readonly string[] _keywords = new string[]
         {
            "contract",
            "if",
            "else",
            "private",
            "public",
            "return",
            "pragma",
            "mapping",
            // TODO add more keywords
         };

        private readonly static string[] _varTypeNames = new[] { 
            "int", "uint", "int256", "uint256", "uint8", "bool", "address", "string"}; 
        public override string AssignmentOperator => "=";

        private readonly string[] _comparisonOperators = new[] { "==", "!=", ">=", "<=", ">", "<" };
        private readonly string[] _arithmeticOperators = new[] { "+", "-", "*", "/", "%" };
        private readonly string[] _logicalOperators = new[] { "!", "^", "&&", "||", "=>" };
        private readonly string[] _bitshiftOperators = new[] { ">>", "<<" };
        private readonly string[] _compoundAssigmentOperators;
        private readonly char[] _separators = new[] { ';', ':', ',', '{', '}', '(', ')', '.' };

        public override string[] ComparisonOperators => _comparisonOperators;
        public override string[] ArithmeticOperators => _arithmeticOperators;
        public override string[] LogicalOperators => _logicalOperators;
        public override string[] BitshiftOperators => _bitshiftOperators;

        public override string[] CompoundAssigmentOperators => _compoundAssigmentOperators;
        public override string[] VarTypeNames => _varTypeNames;
        public override string[] Keywords => _keywords;
        public override char[] Separators => _separators;

        public SolidityLexer() : base()
        {
            _compoundAssigmentOperators = ArithmeticOperators.Select(x => x + "=").ToArray(); // generates +=, -= etc
        }

        protected override LexerToken GenerateToken(int column, int line, string value)
        {
            decimal decval;

            TokenKind kind;

            if (value == "++" || value == "--")
            {
                kind = TokenKind.Postfix;
            }
            else
            if (GetOperatorMatches(value) > 0)
            {
                kind = TokenKind.Operator;
            }
            else
            if (value.StartsWith("0x"))
            {
                kind = TokenKind.Bytes;
            }
            else
            if (value.StartsWith(AsmTag))
            {
                value = value.Substring(AsmTag.Length);
                kind = TokenKind.Asm;
            }
            else
            if (value.StartsWith("@"))
            {
                kind = TokenKind.Address;
                value = value.Substring(1);

                Address addr;

                if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    addr = Address.Null;
                }
                else
                {
                    addr = Address.FromText(value);
                }

                value = "0x" + Base16.Encode(addr.ToByteArray());
            }
            else
            if (value.StartsWith("#"))
            {
                kind = TokenKind.Hash;
                value = value.Substring(1);

                Hash hash = Hash.Parse(value);

                value = "0x" + hash.ToString();
            }
            else
            if (value.StartsWith("$"))
            {
                kind = TokenKind.Macro;
                value = value.Substring(1);
            }
            else
            if (value == "true" || value == "false")
            {
                kind = TokenKind.Bool;
            }
            else
            if (value == ".")
            {
                kind = TokenKind.Selector;
            }
            else
            if (BigInteger.TryParse(value, out BigInteger temp))
            {
                kind = TokenKind.Number;
            }
            else
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decval))
            {
                kind = TokenKind.Decimal;
            }
            else
            {
                var first = value.Length > 0 ? value[0] : '\0';

                switch (first)
                {
                    case '\"':
                        kind = TokenKind.String;
                        break;

                    default:
                        if (IsSeparatorSymbol(first))
                        {
                            kind = TokenKind.Separator;
                        }
                        else
                        {
                            foreach (var varType in VarTypeNames)
                            {
                                if (varType == value)
                                {
                                    kind = TokenKind.Type;
                                    goto Finish;
                                }
                            }

                            foreach (var word in Keywords)
                            {
                                if (word == value)
                                {
                                    kind = TokenKind.Keyword;
                                    goto Finish;
                                }
                            }

                            // otherwise, fall back to identifier
                            if (IsValidIdentifier(value))
                            {
                                kind = TokenKind.Identifier;
                                goto Finish;
                            }

                            if (IsPragmaVersion(value))
                            {
                                value = $"\"{value}\"";
                                kind = TokenKind.String;
                                goto Finish;
                            }

                            if (!string.IsNullOrEmpty(value))
                            {
                                throw new CompilerException("Parsing failed, unsupported token: " + value);
                            }

                            kind = TokenKind.Invalid;
                        }
                        break;
                }
            }

            Finish:
            return new LexerToken(column, line, value, kind);
        }


        public static bool IsPragmaVersion(string value)
        {
            var count = 0;
            foreach (var ch in value)
            {
                if (ch == '.')
                {
                    count++;
                }
                else 
                if (!char.IsDigit(ch))
                {
                    return false;
                }
            }

            return count > 0;
        }
    }
}
