using System.Runtime.CompilerServices;
using System.Threading.Tasks;

//[assembly: InternalsVisibleTo("SoftUnlimit.CQRS")]
namespace SoftUnlimit
{
    public class ValueTaskExtensions
    {
        public readonly static ValueTask CompletedTask = new(Task.CompletedTask);
    }
}
