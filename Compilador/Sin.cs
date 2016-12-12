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
                if (!varDef.Completed)
                {
                    varDef.Step(current, next);
                }
                if (!funcDef.Completed)
                {
                    funcDef.Step(current, next);
                }

                if (varDef.Completed && funcDef.Completed)
                {
                    if (varDef.Success || funcDef.Success)
                    {
                        varDef = new VarDef();
                        funcDef = new FuncDef();
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
                            Completed = true;
                        }
                        break;
                    case 3:
                        if (current.Token != Token.CloseBracket)
                        {
                            Completed = true;
                        }
                        break;
                    case 4:
                        if (current.Token != Token.Id)
                        {
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
            private Stat stat;
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
                        if (current.Token == Token.CloseParentheses)
                        {
                            doStep = false;
                            _step = 8;
                        }
                        else
                        {
                            doStep = false;
                            _step = 4;
                            Step(current, next);
                        }
                        break;
                    case 4:
                        _paramType = current.Token;
                        if (!IsVarType(_paramType))
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 5:
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
                    case 6:
                        doStep = false;
                        if (current.Token == Token.Comma)
                        {
                            _step = 4;
                        }
                        else
                        {
                            _step = 7;
                            Step(current, next);
                        }
                        break;
                    case 7:
                        if (current.Token != Token.CloseParentheses)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 8:
                        if (current.Token != Token.OpenBrace)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 9:
                        doStep = false;
                        if (varDef == null)
                        {
                            varDef = new VarDef();
                        }
                        if (stat == null)
                        {
                            stat = new Stat();
                        }
                        varDef.Step(current, next);
                        if (!stat.Completed)
                        {
                            stat.Step(current, next);
                        }
                        if (varDef.Completed)
                        {
                            if (varDef.Success)
                            {
                                varDef = new VarDef();
                                stat = new Stat();
                            }
                            else
                            {
                                _step = 10;
                            }
                        }
                        break;
                    case 10:
                        doStep = false;
                        stat.Step(current, next);
                        if (stat.Completed)
                        {
                            if (stat.Success)
                            {
                                stat = new Stat();
                            }
                            else
                            {
                                _step = 11;
                                Step(current, next);
                            }
                        }
                        break;
                    case 11:
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
            private Prim _prim = new Prim();
            public bool Completed { get; set; }
            public bool Success { get; set; }

            public void Step(SinEvent current, SinEvent next)
            {
                var doStep = true;
                switch (_step)
                {
                    case 0:
                        doStep = false;
                        _prim.Step(current, next);
                        if (_prim.Completed)
                        {
                            if (_prim.Success)
                            {
                                if (next.Token == Token.And ||
                                    next.Token == Token.Or)
                                {
                                    doStep = true;
                                }
                                else
                                {
                                    Completed = true;
                                    Success = true;
                                }
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 1:
                        if (current.Token == Token.And ||
                            current.Token == Token.Or)
                        {
                            _prim = new Prim();
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 2:
                        doStep = false;
                        _prim.Step(current, next);
                        if (_prim.Completed)
                        {
                            Completed = true;
                            Success = _prim.Success;
                        }
                        break;
                    default:
                        doStep = false;
                        break;
                }
                if (doStep)
                {
                    _step++;
                }
            }
        }

        class Prim : Rotine
        {
            private int _step = 0;
            private SimpExp _simpleExp = new SimpExp();
            public bool IsNegative { get; set; }
            public bool Completed { get; set; }
            public bool Success { get; set; }
            public void Step(SinEvent current, SinEvent next)
            {
                var doStep = true;
                switch (_step)
                {
                    case 0:
                        doStep = false;
                        _simpleExp.Step(current, next);
                        if (_simpleExp.Completed)
                        {
                            if (_simpleExp.Success)
                            {
                                if (next.Token == Token.Less ||
                                    next.Token == Token.LessOrEquals ||
                                    next.Token == Token.NotEquals ||
                                    next.Token == Token.Equals ||
                                    next.Token == Token.Greater ||
                                    next.Token == Token.GreaterOrEquals)
                                {
                                    doStep = true;
                                }
                                else
                                {
                                    Completed = true;
                                    Success = true;
                                }
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 1:
                        if (current.Token == Token.Less ||
                            current.Token == Token.LessOrEquals ||
                            current.Token == Token.NotEquals ||
                            current.Token == Token.Equals ||
                            current.Token == Token.Greater ||
                            current.Token == Token.GreaterOrEquals)
                        {
                            _simpleExp = new SimpExp();
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 2:
                        doStep = false;
                        _simpleExp.Step(current, next);
                        if (_simpleExp.Completed)
                        {
                            Completed = true;
                            Success = _simpleExp.Success;
                        }
                        break;
                    default:
                        doStep = false;
                        break;
                }
                if (doStep)
                {
                    _step++;
                }
            }
        }

        class SimpExp : Rotine
        {
            private int _step = 0;
            private Term _term = new Term();
            public bool IsNegative { get; set; }
            public bool Completed { get; set; }
            public bool Success { get; set; }
            public void Step(SinEvent current, SinEvent next)
            {
                var doStep = true;
                switch (_step)
                {
                    case 0:
                        doStep = false;
                        if (current.Token == Token.Minus)
                        {
                            IsNegative = true;
                        }
                        else
                        {
                            _step = 1;
                            Step(current, next);
                        }
                        break;
                    case 1:
                        doStep = false;
                        _term.Step(current, next);
                        if (_term.Completed)
                        {
                            if (_term.Success)
                            {
                                if (next.Token == Token.Plus ||
                                    next.Token == Token.Minus)
                                {
                                    doStep = true;
                                }
                                else
                                {
                                    Completed = true;
                                    Success = true;
                                }
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 2:
                        if (current.Token == Token.Plus ||
                            current.Token == Token.Minus)
                        {
                            _term = new Term();
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 3:
                        doStep = false;
                        _term.Step(current, next);
                        if (_term.Completed)
                        {
                            Completed = true;
                            Success = _term.Success;
                        }
                        break;
                    default:
                        doStep = false;
                        break;
                }
                if (doStep)
                {
                    _step++;
                }
            }
        }

        class Term : Rotine
        {
            private int _step = 0;
            private Fact _fact = new Fact();
            public bool Completed { get; set; }
            public bool Success { get; set; }
            public void Step(SinEvent current, SinEvent next)
            {
                var doStep = true;
                switch (_step)
                {
                    case 0:
                        doStep = false;
                        _fact.Step(current, next);
                        if (_fact.Completed)
                        {
                            if (_fact.Success)
                            {
                                if (next.Token == Token.Multiplication ||
                                    next.Token == Token.Division)
                                {
                                    doStep = true;
                                }
                                else
                                {
                                    Completed = true;
                                    Success = true;
                                }
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 1:
                        if (current.Token == Token.Multiplication ||
                            current.Token == Token.Division)
                        {
                            _fact = new Fact();
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 2:
                        doStep = false;
                        _fact.Step(current, next);
                        if (_fact.Completed)
                        {
                            Completed = true;
                            Success = _fact.Success;
                        }
                        break;
                    default:
                        doStep = false;
                        break;
                }
                if (doStep)
                {
                    _step++;
                }
            }
        }

        class Fact : Rotine
        {
            private int _step = 0;
            private Exp _exp = null;
            public bool IsNot { get; set; }
            public bool Completed { get; set; }
            public bool Success { get; set; }
            public void Step(SinEvent current, SinEvent next)
            {
                var doStep = true;
                switch (_step)
                {
                    case 0:
                        doStep = false;
                        if (current.Token == Token.Not && !IsNot)
                        {
                            IsNot = true;
                        }
                        else if (current.Token == Token.Id)
                        {
                            if (next.Token == Token.OpenParentheses ||
                                next.Token == Token.OpenBracket)
                            {
                                _step = 1;
                            }
                            else
                            {
                                Completed = true;
                                Success = true;
                            }
                        }
                        else if (current.Token == Token.OpenParentheses)
                        {
                            _step = 6;
                            _exp = new Exp();
                        }
                        else if (current.Token == Token.True)
                        {
                            Completed = true;
                            Success = true;
                        }
                        else if (current.Token == Token.False)
                        {
                            Completed = true;
                            Success = true;
                        }
                        else if (current.Token == Token.Int)
                        {
                            Completed = true;
                            Success = true;
                        }
                        else if (current.Token == Token.Float)
                        {
                            Completed = true;
                            Success = true;
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 1:
                        doStep = false;
                        if (current.Token == Token.OpenParentheses)
                        {
                            _step = 2;
                            _exp = new Exp();
                        }
                        else if (current.Token == Token.OpenBracket)
                        {
                            _step = 4;
                            _exp = new Exp();
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 2:
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
                    case 3:
                        doStep = false;
                        if (current.Token == Token.CloseParentheses)
                        {
                            Completed = true;
                            Success = true;
                        }
                        else if (current.Token == Token.Comma)
                        {
                            _step = 2;
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 4:
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
                    case 5:
                        Completed = true;
                        Success = current.Token == Token.CloseBracket;
                        break;
                    case 6:
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
                    case 7:
                        Completed = true;
                        Success = current.Token == Token.CloseParentheses;
                        break;
                    default:
                        doStep = false;
                        break;
                }
                if (doStep)
                {
                    _step++;
                }
            }
        }

        class Stat : Rotine
        {
            private int _step = 0;
            private Stat stat;
            private Exp exp;
            private bool isIf = false;
            public bool Completed { get; set; }
            public bool Success { get; set; }

            public void Step(SinEvent current, SinEvent next)
            {
                var doStep = true;
                switch (_step)
                {
                    case 0:
                        doStep = false;
                        exp = new Exp();
                        switch (current.Token)
                        {
                            case Token.Return:
                                _step = 1;
                                break;
                            case Token.Id:
                                _step = 2;
                                break;
                            case Token.If:
                                isIf = true;
                                _step = 10;
                                break;
                            case Token.While:
                                isIf = false;
                                _step = 10;
                                break;
                            default:
                                Completed = true;
                                Success = false;
                                break;
                        }
                        break;
                    case 1:
                        doStep = false;
                        exp.Step(current, next);
                        if (exp.Completed)
                        {
                            if (exp.Success)
                            {
                                _step = 20;
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 2:
                        doStep = false;
                        if (current.Token == Token.OpenBracket)
                        {
                            _step = 3;
                            exp = new Exp();
                        }
                        else if (current.Token == Token.Assign)
                        {
                            _step = 5;
                        }
                        else if (current.Token == Token.OpenParentheses){
                            _step = 6;
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 3:
                        doStep = false;
                        exp.Step(current, next);
                        if (exp.Completed)
                        {
                            doStep = true;
                            if (!exp.Success)
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 4:
                        if (current.Token != Token.CloseBracket)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 5:
                        doStep = false;
                        exp.Step(current, next);
                        if (exp.Completed)
                        {
                            if (exp.Success)
                            {
                                _step = 20;
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 6:
                        doStep = false;
                        if (current.Token == Token.CloseParentheses)
                        {
                            _step = 20;
                        }
                        else
                        {
                            _step = 7;
                            exp = new Exp();
                            Step(current, next);
                        }
                        break;
                    case 7:
                        doStep = false;
                        exp.Step(current, next);
                        if (exp.Completed)
                        {
                            if (exp.Success)
                            {
                                _step = 9;
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 8:
                        if (current.Token == Token.Comma)
                        {
                            _step = 7;
                            exp = new Exp();
                        }
                        else if (current.Token == Token.CloseParentheses)
                        {
                            _step = 20;
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 9:
                        if (current.Token == Token.Comma)
                        {
                            _step = 7;
                            exp = new Exp();
                        }
                        else if (current.Token == Token.CloseParentheses)
                        {
                            _step = 20;
                        }
                        else
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 10:
                        if (current.Token != Token.OpenParentheses) {
                            Completed = true;
                            Success = false;
                        }
                        exp = new Exp();
                        break;
                    case 11:
                        exp.Step(current, next);
                        doStep = false;
                        if (exp.Completed)
                        {
                            if (exp.Success)
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
                    case 12:
                        if (current.Token != Token.CloseParentheses)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 13:
                        doStep = false;
                        stat = new Stat();
                        if (current.Token == Token.OpenBrace)
                        {
                            _step = 15;
                        }
                        else
                        {
                            _step = 14;
                            Step(current, next);
                        }
                        break;
                    case 14:
                        doStep = false;
                        stat.Step(current, next);
                        if (stat.Completed)
                        {
                            if (stat.Success)
                            {
                                if (isIf && next.Token == Token.Else)
                                {
                                    _step = 16;
                                }
                                else
                                {
                                    Completed = true;
                                    Success = true;
                                }
                            }
                            else
                            {
                                Completed = true;
                                Success = false;
                            }
                        }
                        break;
                    case 15:
                        doStep = false;
                        stat.Step(current, next);
                        if (stat.Completed)
                        {
                            if (stat.Success)
                            {
                                stat = new Stat();
                            }
                            else
                            {
                                if (current.Token != Token.CloseBrace)
                                {
                                    Completed = true;
                                    Success = false;
                                }
                                else
                                {
                                    if (isIf && next.Token == Token.Else)
                                    {
                                        _step = 16;
                                    }
                                    else
                                    {
                                        Completed = true;
                                        Success = true;
                                    }
                                }
                            }
                        }
                        break;
                    case 16:
                        if (current.Token != Token.Else)
                        {
                            Completed = true;
                            Success = false;
                        }
                        break;
                    case 17:
                        doStep = false;
                        stat = new Stat();
                        if (current.Token == Token.OpenBrace)
                        {
                            _step = 19;
                        }
                        else
                        {
                            _step = 18;
                            Step(current, next);
                        }
                        break;
                    case 18:
                        doStep = false;
                        stat.Step(current, next);
                        if (stat.Completed)
                        {
                            Completed = true;
                            Success = stat.Success;
                        }
                        break;
                    case 19:
                        doStep = false;
                        stat.Step(current, next);
                        if (stat.Completed)
                        {
                            if (stat.Success)
                            {
                                stat = new Stat();
                            }
                            else
                            {
                                Completed = true;
                                Success = current.Token == Token.CloseBrace;
                            }
                        }
                        break;
                    case 20:
                        Completed = true;
                        Success = current.Token == Token.Semicolon;
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
