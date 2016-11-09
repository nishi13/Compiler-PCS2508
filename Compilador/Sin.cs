using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class Sin : IEventMachine<bool>
    {
        private IEventMachine<SinEvent> _provider;
        private bool _sintaxError = false;
        private Program program = new Program();

        public Sin(IEventMachine<SinEvent> provider)
        {
            _provider = provider;
        }

        public bool GetEvent()
        {
            var current = _provider.GetEvent();
            var next = _provider.GetEvent();
            while (!program.Completed)
            {
                Console.WriteLine(current.ToString());
                program.Step(current, next);
                current = next;
                next = _provider.GetEvent();
            }
            return program.Success;
        }

        interface Rotine
        {
            bool Completed { get; set; }
            bool Success { get; set; }
            void Step(SinEvent current, SinEvent next);
        }

        class Program : Rotine
        {
            private VarDef varDef;
            private FuncDef funcDef;
            public bool Completed { get; set; }
            public bool Success { get; set; }

            public Program()
            {
                varDef = new VarDef();
                funcDef = new FuncDef();
            }

            public void Step (SinEvent current, SinEvent next)
            {
                if (varDef.Success || funcDef.Success)
                {
                    varDef = new VarDef();
                    funcDef = new FuncDef();
                }
                else if (!varDef.Completed || !funcDef.Completed)
                {
                    if (!varDef.Completed)
                    {
                        varDef.Step(current, next);
                    }
                    if (!funcDef.Completed)
                    {
                        funcDef.Step(current, next);
                    }
                }
                else
                {
                    if (current.Token == Token.EndOfFile)
                    {
                        Success = true;
                        Completed = true;
                    }
                    else
                    {
                        error("Unexpected " + current.ToString());
                        Success = false;
                        Completed = true;
                    }
                }
            }
        }

        class VarDef : Rotine
        {
            private Exp _exp;
            private Token _type;
            private bool _isArray;
            private int arraySize;
            private int _step = 0;
            public bool Completed { get; set; }
            public bool Success { get; set; }

            public void Step (SinEvent current, SinEvent next)
            {
                var doStep = true;
                switch (_step)
                {
                    case 0:
                        _type = current.Token;
                        if (!IsVarType(current.Token))
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 1:
                        if (current.Token == Token.OpenBracket)
                        {
                            _isArray = true;
                        }
                        else
                        {
                            doStep = false;
                            _step = 4;
                            Step(current, next);
                        }
                        break;
                    case 2:
                        if (current.Token == Token.Int)
                        {
                            arraySize = (int)current.Content;
                        }
                        else
                        {
                            error("Expected int but got " + current.ToString());
                            Completed = true;
                        }
                        break;
                    case 3:
                        if (current.Token != Token.CloseBracket)
                        {
                            error("Expected ']' but got " + current.ToString());
                            Completed = true;
                        }
                        break;
                    case 4:
                        if (current.Token != Token.Id)
                        {
                            error("Expected VarName but got " + current.ToString());
                            Completed = true;
                        }
                        else
                        {
                            //TODO: add to var name list
                        }
                        break;
                    case 5:
                        if (current.Token == Token.Semicolon)
                        {
                            Completed = true;
                            Success = true;
                        }
                        else if (current.Token == Token.Assign)
                        {
                            doStep = false;
                            _step = 7;
                            _exp = new Exp();
                        }
                        else if(current.Token != Token.Comma)
                        {
                            error("Unexpected " + current.ToString());
                            Completed = true;
                        }
                        break;
                    case 6:
                        if (current.Token == Token.Id)
                        {
                            doStep = false;
                            _step = 5;
                        }
                        else
                        {
                            error("Expected VarName but got " + current.ToString());
                            Completed = true;
                        }
                        break;
                    case 7:
                        doStep = false;
                        _exp.Step(current, next);
                        if (_exp.Completed)
                        {
                            if (_exp.Success)
                            {
                                doStep = true;
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 8:
                        if (current.Token == Token.Semicolon)
                        {
                            Completed = true;
                            Success = true;
                        }
                        else
                        {
                            error("Unexpected " + current.ToString());
                            Completed = true;
                        }
                        break;
                    default:
                        Completed = true;
                        break;
                }
                if (doStep)
                {
                    _step ++;
                }
            }
        }

        class FuncDef : Rotine
        {
            private VarDef varDef;
            private Token _type;
            private Token _paramType;
            private int _step = 0;
            public bool Completed { get; set; }
            public bool Success { get; set; }

            public void Step(SinEvent current, SinEvent next)
            {
                var doStep = true;
                switch (_step)
                {
                    case 0:
                        _type = current.Token;
                        if (!IsVarType(current.Token) &&
                            _type != Token.Void)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 1:
                        if (current.Token == Token.Id)
                        {
                            //TODO: add to func name list
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 2:
                        if (current.Token != Token.OpenParentheses)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 3:
                        _paramType = current.Token;
                        if (!IsVarType(_paramType))
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 4:
                        if (current.Token == Token.Id)
                        {
                            //TODO: resolve paramenter name usig _paramType
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 5:
                        if (current.Token == Token.Comma)
                        {
                            doStep = false;
                            _step = 3;
                        }
                        break;
                    case 6:
                        if (current.Token != Token.CloseParentheses)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 7:
                        if (current.Token != Token.OpenBrace)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 8:
                        doStep = false;
                        if (varDef == null)
                        {
                            varDef = new VarDef();
                        }
                        varDef.Step(current, next);
                        if (varDef.Completed)
                        {
                            if (varDef.Success)
                            {
                                varDef = new VarDef();
                            }
                            else
                            {
                                _step = 9;
                                Step(current, next);
                            }
                        }
                        break;
                    case 9:
                        doStep = false;
                        if (current.Token == Token.CloseBrace)
                        {
                            _step = 10;
                            Step(current, next);
                        }
                        break;
                    case 10:
                        Completed = true;
                        if (current.Token == Token.CloseBrace)
                        {
                            Success = true;
                        }
                        else
                        {
                            Success = false;
                        }
                        break;
                    default:
                        Completed = true;
                        break;
                }
                if (doStep)
                {
                    _step++;
                }
            }
        }

        class Exp : Rotine
        {
            private int _step = 0;
            public bool Completed { get; set; }
            public bool Success { get; set; }

            public void Step(SinEvent current, SinEvent next)
            {
                if (next.Token == Token.Semicolon)
                {
                    Completed = true;
                    Success = true;
                }
            }
        }

        private static bool IsVarType(Token token)
        {
            return token == Token.IntDeclaration || token == Token.FloatDeclaration || token == Token.BooleanDeclaration;
        }

        private static void error(string msg)
        {
            Console.WriteLine("Sintax Error: " + msg);
        }
    }
}
