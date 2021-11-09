using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SoftUnlimit.CQRS, PublicKey=0024000004800000940000000602000000240000525341310004000001000100cddc4d3e754901821317d39615f8b21d777c5fc9e5d6ce272ef630d52ca53f11c79f52019e24e013b84cd576fdb292669c238c8ffc59e60e8f82374c651b8d8a7c7f7f5a14d6acedd422ebaab1e333bcab34267e1c84561e1f1caa8921c2f8f656ca895e1d0d149da896abf3f10516bdb275f3aabf4aaec9117161853b987bc1")]
namespace SoftUnlimit
{
    internal class ValueTaskExtensions
    {
        internal readonly static ValueTask CompletedTask = new(Task.CompletedTask);
    }
}
