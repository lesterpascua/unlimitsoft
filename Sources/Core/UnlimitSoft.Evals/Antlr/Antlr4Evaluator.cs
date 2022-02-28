using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using UnlimitSoft.Evals.Compiler;
using UnlimitSoft.Evals.Grammar;
using UnlimitSoft.Evals.Utils;

namespace UnlimitSoft.Evals.Antlr
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Antlr4Evaluator<T> : IEvaluator<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        public Antlr4Evaluator(IFunctionTable table)
        {
            MethodTable = table;
        }

        /// <summary>
        /// 
        /// </summary>
        protected IFunctionTable MethodTable { get; }

        /// <summary>
        /// Eval expression and return a result.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="expression"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public double Eval(IServiceProvider provider, string expression, T argument)
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

            var visitor = new EvaluatorVisitor(provider, MethodTable, argument);
            return (double)visitor.Visit(compileUnit);
        }


        #region Nested Classes
#pragma warning disable CS8601 // Possible null reference assignment.
        private class EvaluatorVisitor : ArithmeticExpressionBaseVisitor<object>
        {
            private readonly T _context;
            private readonly IFunctionTable _methodTable;
            private readonly IServiceProvider _provider;


            /// <summary>
            /// 
            /// </summary>
            /// <param name="provider"></param>
            /// <param name="table"></param>
            /// <param name="context"></param>
            public EvaluatorVisitor(IServiceProvider provider, IFunctionTable table, T context)
            {
                _context = context;
                _methodTable = table;
                _provider = provider;
            }

            public override object VisitCompileUnit([NotNull] ArithmeticExpressionParser.CompileUnitContext context)
            {
                return Visit(context.expression());
            }
            public override object VisitTerm([NotNull] ArithmeticExpressionParser.TermContext context)
            {
                var text = context.GetText();
                double result = double.Parse(text);

                return result;
            }
            public override object VisitParenExpression([NotNull] ArithmeticExpressionParser.ParenExpressionContext context)
            {
                return Visit(context.expression());
            }
            public override object VisitFunctionExpression([NotNull] ArithmeticExpressionParser.FunctionExpressionContext context)
            {
                var trees = context.expression();
                var method = context.FUNCTION().GetText();

                var arguments = new object[trees.Length + 2];
                arguments[0] = _provider;
                arguments[1] = _context;
                for (int i = 0; i < trees.Length; i++)
                    arguments[i + 2] = Visit(trees[i]);

                return _methodTable.Invoke(method, arguments);
            }
            public override object VisitUnaryExpression([NotNull] ArithmeticExpressionParser.UnaryExpressionContext context)
            {
                var term = context.expression();
                var ope = context.op.Text;
                return ope switch
                {
                    "-" => -(double)Visit(term),
                    _ => 0,
                };
            }
            public override object VisitMultExpression([NotNull] ArithmeticExpressionParser.MultExpressionContext context)
            {
                var term = context.expression();
                var op1 = Visit(term[0]);
                var op2 = Visit(term[1]);

                return context.op.Text switch
                {
                    "*" => (double)op1 * (double)op2,
                    "/" => (double)op1 / (double)op2,
                    "%" => (double)op1 % (double)op2,
                    _ => 0,
                };
            }
            public override object VisitSumExpression([NotNull] ArithmeticExpressionParser.SumExpressionContext context)
            {
                var term = context.expression();
                var op1 = Visit(term[0]);
                var op2 = Visit(term[1]);

                return context.op.Text switch
                {
                    "+" => (double)op1 + (double)op2,
                    "-" => (double)op1 - (double)op2,
                    _ => 0,
                };
            }
            public override object VisitLogicalExpression([NotNull] ArithmeticExpressionParser.LogicalExpressionContext context)
            {
                var term = context.expression();
                var op1 = Visit(term[0]);
                var op2 = Visit(term[1]);

                return context.op.Text switch
                {
                    "<" => (double)op1 < (double)op2 ? 1 : 0,
                    ">" => (double)op1 > (double)op2 ? 1 : 0,
                    "<=" => (double)op1 <= (double)op2 ? 1 : 0,
                    ">=" => (double)op1 >= (double)op2 ? 1 : 0,
                    "||" => (double)op1 != 0 ? (double)op1 : (double)op2,
                    "&&" => (double)op1 == 0 ? (double)op1 : (double)op2,
                    "==" => (double)op1 == (double)op2 ? 1 : 0,
                    "!=" => (double)op1 != (double)op2 ? 1 : 0,
                    _ => 0,
                };
            }
        }
#pragma warning restore CS8601 // Possible null reference assignment.
        #endregion
    }
}
