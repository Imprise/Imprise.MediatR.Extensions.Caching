using System.Threading.Tasks;

namespace Imprise.MediatR.Extensions.Caching
{
    public interface ICacheInvalidator<in TRequest>
    {
        Task Invalidate(TRequest request);
    }
}