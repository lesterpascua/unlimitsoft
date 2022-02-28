using System.IO;
using Antlr4.Runtime;

namespace UnlimitSoft.Evals.Utils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S"></typeparam>
    public class ErrorListener<S> : ConsoleErrorListener<S>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="recognizer"></param>
        /// <param name="offendingSymbol"></param>
        /// <param name="line"></param>
        /// <param name="charPositionInLine"></param>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, S offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            HadError = true;
            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
        }

        /// <summary>
        /// If there is an error true, false in other case.
        /// </summary>
        public bool HadError { get; private set; }
    }
}