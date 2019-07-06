using System.Threading.Tasks;
using MediatR;

namespace Imprise.MediatR.Extensions.Caching
{
    public interface ICache<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Get(TRequest request);
        Task Set(TRequest request, TResponse value);
        Task Remove(string cacheKeyIdentifier);
    }
}