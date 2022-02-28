using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Reflection;
using System.Reflection.Emit;
using UnlimitSoft.Evals.Compiler;
using UnlimitSoft.Evals.Grammar;
using UnlimitSoft.Evals.Utils;

namespace UnlimitSoft.Evals.Antlr
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Antlr4Compiler<T> : ICompiler<T>
    {
        private readonly Module _module;
        private readonly IFunctionTable _table;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="table"></param>
        public Antlr4Compiler(Module module, IFunctionTable table)
        {
            _module = module;
            _table = table;
        }

        /// <summary>
        /// Compile expression and return a method whit compiled code.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ArithmeticExpression<T> Compile(string expression, string name)
        {
            var stream = new AntlrInputStream(expression);
            var lexer = new ArithmeticExpressionLexer(stream);

            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ArithmeticExpressionParser(tokenStream);

            var listener = new ErrorListener<IToken>();
            parser.AddErrorListener(listener);

            var compileUnit = parser.compileUnit();
            if (listener.HadError)
                throw new InvalidOperationException();

#if DEBUG
            Console.WriteLine(tokenStream.GetTokens().OutputTokens());
            Console.WriteLine(compileUnit.OutputTree(tokenStream));
#endif

            var visitor = new CompilerVisitor(_table, _module, name);
            return visitor.Visit(compileUnit);
        }


        #region Nested Classes
#pragma warning disable CS8603 // Possible null reference return.
        /// <summary>
        /// Internal compiler 
        /// </summary>
        private class CompilerVisitor : ArithmeticExpressionBaseVisitor<ArithmeticExpression<T>>
        {
            private readonly ILGenerator _iLGenerator;
            private readonly DynamicMethod _dynamicMethod;
            private readonly IFunctionTable _methodTable;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="table"></param>
            /// <param name="module"></param>
            /// <param name="name"></param>
            public CompilerVisitor(IFunctionTable table, Module module, string name)
            {
                Type[] args = { typeof(IServiceProvider), typeof(T) };

                _methodTable = table;
                _dynamicMethod = new DynamicMethod(name, typeof(double), args, module);
                _iLGenerator = _dynamicMethod.GetILGenerator();
            }

            public override ArithmeticExpression<T> VisitCompileUnit([NotNull] ArithmeticExpressionParser.CompileUnitContext context)
            {
                Visit(context.expression());
                _iLGenerator.Emit(OpCodes.Ret);

                return (ArithmeticExpression<T>)_dynamicMethod.CreateDelegate(typeof(ArithmeticExpression<T>));
            }
            public override ArithmeticExpression<T> VisitTerm([NotNull] ArithmeticExpressionParser.TermContext context)
            {
                ArithmeticExpression<T> ParseDouble(ITerminalNode node)
                {
                    var strValue = node.GetText();
                    var value = double.Parse(strValue);

                    _iLGenerator.Emit(OpCodes.Ldc_R8, value);
                    return null;
                }
                ArithmeticExpression<T> ParseString(ITerminalNode node)
                {
                    var text = node.GetText();
                    var value = text.Substring(1, text.Length - 2);
                    _iLGenerator.Emit(OpCodes.Ldstr, value);
                    return null;
                }

                // ====================================================================================
                var number = context.NUMBER();
                if (number is not null)
                    return ParseDouble(number);

                return ParseString(context.LITERAL());
            }
            public override ArithmeticExpression<T> VisitParenExpression([NotNull] ArithmeticExpressionParser.ParenExpressionContext context)
            {
                return Visit(context.expression());
            }
            public override ArithmeticExpression<T> VisitFunctionExpression([NotNull] ArithmeticExpressionParser.FunctionExpressionContext context)
            {
                _iLGenerator.Emit(OpCodes.Ldarg_0);        // load IServiceProvider from first argument in method
                _iLGenerator.Emit(OpCodes.Ldarg_1);        // load ContractContext from second argument in method
                foreach (var arg in context.expression())
                    Visit(arg);

                string function = context.FUNCTION().GetText();
                var method = _methodTable.Get(function);
                if (method is null)
                    throw new InvalidOperationException("Invalid method can't compiler the expression");

                _iLGenerator.Emit(OpCodes.Call, method);

                return null;
            }
            public override ArithmeticExpression<T> VisitUnaryExpression([NotNull] ArithmeticExpressionParser.UnaryExpressionContext context)
            {
                Visit(context.expression());

                var ope = context.op.Text;
                switch (ope)
                {
                    case "-":
                        _iLGenerator.Emit(OpCodes.Neg);
                        break;
                }
                return null;
            }
            public override ArithmeticExpression<T> VisitMultExpression([NotNull] ArithmeticExpressionParser.MultExpressionContext context)
            {
                var term = context.expression();

                Visit(term[0]);
                Visit(term[1]);

                switch (context.op.Text)
                {
                    case "*":
                        _iLGenerator.Emit(OpCodes.Mul);
                        break;
                    case "/":
                        _iLGenerator.Emit(OpCodes.Div);
                        break;
                    case "%":
                        _iLGenerator.Emit(OpCodes.Rem);
                        break;
                }
                return null;
            }
            public override ArithmeticExpression<T> VisitSumExpression([NotNull] ArithmeticExpressionParser.SumExpressionContext context)
            {
                var term = context.expression();

                Visit(term[0]);
                Visit(term[1]);

                switch (context.op.Text)
                {
                    case "+":
                        _iLGenerator.Emit(OpCodes.Add);
                        break;
                    case "-":
                        _iLGenerator.Emit(OpCodes.Sub);
                        break;
                }
                return null;
            }
            public override ArithmeticExpression<T> VisitLogicalExpression([NotNull] ArithmeticExpressionParser.LogicalExpressionContext context)
            {
                var term = context.expression();

                Visit(term[0]);
                Visit(term[1]);
                switch (context.op.Text)
                {
                    case "<":
                        _iLGenerator.Emit(OpCodes.Clt);
                        _iLGenerator.Emit(OpCodes.Conv_R8);
                        break;
                    case "<=":
                        _iLGenerator.Emit(OpCodes.Cgt);
                        _iLGenerator.Emit(OpCodes.Ldc_I4_1);
                        _iLGenerator.Emit(OpCodes.Xor);
                        _iLGenerator.Emit(OpCodes.Conv_R8);
                        break;
                    case ">":
                        _iLGenerator.Emit(OpCodes.Cgt);
                        _iLGenerator.Emit(OpCodes.Conv_R8);
                        break;
                    case ">=":
                        _iLGenerator.Emit(OpCodes.Clt);
                        _iLGenerator.Emit(OpCodes.Ldc_I4_1);
                        _iLGenerator.Emit(OpCodes.Xor);
                        _iLGenerator.Emit(OpCodes.Conv_R8);
                        break;
                    case "||":
                        {
                            var end = _iLGenerator.DefineLabel();
                            var @else = _iLGenerator.DefineLabel();
                            var tmp = _iLGenerator.DeclareLocal(typeof(double));

                            _iLGenerator.Emit(OpCodes.Stloc, tmp);
                            _iLGenerator.Emit(OpCodes.Dup);
                            _iLGenerator.Emit(OpCodes.Ldc_R8, 0.0d);
                            _iLGenerator.Emit(OpCodes.Ceq);

                            _iLGenerator.Emit(OpCodes.Brtrue, @else);               // if
                            _iLGenerator.Emit(OpCodes.Br, end);

                            _iLGenerator.MarkLabel(@else);                          // else
                            _iLGenerator.Emit(OpCodes.Pop);
                            _iLGenerator.Emit(OpCodes.Ldloc, tmp);

                            _iLGenerator.MarkLabel(end);
                        }
                        break;
                    case "&&":
                        {
                            var end = _iLGenerator.DefineLabel();
                            var tmp = _iLGenerator.DeclareLocal(typeof(double));

                            _iLGenerator.Emit(OpCodes.Stloc, tmp);
                            _iLGenerator.Emit(OpCodes.Dup);
                            _iLGenerator.Emit(OpCodes.Ldc_R8, 0.0d);
                            _iLGenerator.Emit(OpCodes.Ceq);

                            _iLGenerator.Emit(OpCodes.Brtrue, end);
                            _iLGenerator.Emit(OpCodes.Pop);
                            _iLGenerator.Emit(OpCodes.Ldloc, tmp);

                            _iLGenerator.MarkLabel(end);
                        }
                        break;
                    case "==":
                        _iLGenerator.Emit(OpCodes.Ceq);
                        _iLGenerator.Emit(OpCodes.Conv_R8);
                        break;
                    case "!=":
                        _iLGenerator.Emit(OpCodes.Ceq);
                        _iLGenerator.Emit(OpCodes.Ldc_I4_1);
                        _iLGenerator.Emit(OpCodes.Xor);
                        _iLGenerator.Emit(OpCodes.Conv_R8);
                        break;
                }
                return null;
            }
        }
#pragma warning restore CS8603 // Possible null reference assignment.
        #endregion
    }
}
