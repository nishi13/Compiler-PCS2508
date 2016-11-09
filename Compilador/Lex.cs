using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class Lex : IEventMachine<SinEvent>
    {
        private IEventMachine<LexEvent> _provider;
        private char _savedContent;
        private Dictionary<string, Token> _reservedWord;

        public Lex (IEventMachine<LexEvent> provider)
        {
            _provider = provider;
            _reservedWord = new Dictionary<string, Token>()
            {
                { "if", Token.If },
                { "else", Token.Else },
                { "while", Token.While },
                { "void", Token.Void },
                { "return", Token.Return },
                { "int", Token.IntDeclaration },
                { "float", Token.FloatDeclaration },
                { "bool", Token.BooleanDeclaration },
                { "true", Token.True },
                { "false", Token.False }
            };
        }

        public SinEvent GetEvent()
        {
            var content = NextEvent();
            while (Char.IsWhiteSpace(content) || Char.IsSeparator(content) || content == '\t')
            {
                content = NextEvent();
            }
            switch (content)
            {
                case '+':
                    return new SinEvent(Token.Plus);
                case '-':
                    return new SinEvent(Token.Minus);
                case '*':
                    return new SinEvent(Token.Multiplication);
                case '/':
                    return new SinEvent(Token.Division);
                case ';':
                    return new SinEvent(Token.Semicolon);
                case ',':
                    return new SinEvent(Token.Comma); 
                case '(':
                    return new SinEvent(Token.OpenParentheses);
                case ')':
                    return new SinEvent(Token.CloseParentheses);
                case '[':
                    return new SinEvent(Token.OpenBracket);
                case ']':
                    return new SinEvent(Token.CloseBracket);
                case '{':
                    return new SinEvent(Token.OpenBrace);
                case '}':
                    return new SinEvent(Token.CloseBrace);
                case '\0':
                    return new SinEvent(Token.EndOfFile);
                case '=':
                    content = NextEvent();
                    if (content == '=')
                    {
                        return new SinEvent(Token.Equals);
                    }
                    else
                    {
                        _savedContent = content;
                        return new SinEvent(Token.Assign);
                    }
                case '!':
                    content = NextEvent();
                    if (content == '=')
                    {
                        return new SinEvent(Token.NotEquals);
                    }
                    else
                    {
                        _savedContent = content;
                        return new SinEvent(Token.Not);
                    }
                case '<':
                    content = NextEvent();
                    if (content == '=')
                    {
                        return new SinEvent(Token.LessOrEquals);
                    }
                    else
                    {
                        _savedContent = content;
                        return new SinEvent(Token.Less);
                    }
                case '>':
                    content = NextEvent();
                    if (content == '=')
                    {
                        return new SinEvent(Token.GreaterOrEquals);
                    }
                    else
                    {
                        _savedContent = content;
                        return new SinEvent(Token.Greater);
                    }
                case '|':
                    content = NextEvent();
                    if (content == '|')
                    {
                        return new SinEvent(Token.Or);
                    }
                    else
                    {
                        _savedContent = content;
                        return null;
                    }
                case '&':
                    content = NextEvent();
                    if (content == '&')
                    {
                        return new SinEvent(Token.And);
                    }
                    else
                    {
                        _savedContent = content;
                        return null;
                    }
                default:
                    if (IsDigit(content)){
                        int buffer = 0;
                        while (IsDigit(content)){
                            buffer = buffer * 10 + (int)Char.GetNumericValue(content);
                            content = NextEvent();
                        }

                        if (content == '.')
                        {
                            float fbuffer = buffer;
                            int mag = 10;
                            content = NextEvent();
                            while (IsDigit(content)){
                                fbuffer = fbuffer + ((int)Char.GetNumericValue(content))/mag;
                                mag = mag * 10;
                                content = NextEvent();
                            }
                            _savedContent = content;
                            return new SinEvent(Token.Float, fbuffer);
                        }
                        else 
                        {
                            _savedContent = content;
                            return new SinEvent(Token.Int, buffer);
                        }
                    }
                    else if (IsLetter(content)){
                        string buffer = "";
                        while (IsDigit(content) || IsLetter(content)){
                            buffer = buffer + content;
                            content = NextEvent();
                        }
                        _savedContent = content;
                        if (_reservedWord.ContainsKey(buffer))
                        {
                            return new SinEvent(_reservedWord[buffer]);
                        }
                        else 
                        {
                            return new SinEvent(Token.Id, buffer);
                        }
                    }
                    else {
                        return new SinEvent(Token.Unknown, content);
                    }
            }
        }

        public bool IsDigit(char content)
        {
            return Char.IsDigit(content);
        }

        public bool IsLetter(char content)
        {
            return Char.IsLetter(content) || content == '_';
        }

        private char NextEvent ()
        {
            if (_savedContent != '\0')
            {
                var ret = _savedContent;
                _savedContent = '\0';
                return ret;
            }
            else
            {
                var ev = _provider.GetEvent();
                if (ev == null)
                {
                    return '\0';
                }
                else 
                {
                    return ev.Content;
                }
            }
        }
    }
}
