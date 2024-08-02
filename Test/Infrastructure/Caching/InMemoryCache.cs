namespace CoreFtp.Infrastructure.Caching
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;

    public class InMemoryCache : ICache
    {
        private readonly IMemoryCache _cache;

        public InMemoryCache()
        {
            _cache = new MemoryCache( new MemoryCacheOptions() );
        }

        public bool HasKey( string key )
        {
            return _cache.Get( key ) != null;
        }

        public T Get< T >( string key ) where T : class
        {
            T outValue;
            _cache.TryGetValue( key, out outValue );
            return outValue;
        }

        public void Remove( string key )
        {
            _cache.Remove( key );
        }

        public T GetOrSet< T >( string key, Func<T> expression, TimeSpan expiresIn ) where T : class
        {
            var found = Get<T>( key );
            if ( !Equals( found, default( T ) ) )
                return found;

            var executed = expression.Invoke();
            _cache.Set( key, executed, DateTime.Now + expiresIn );

            return executed;
        }

        public void Add< T >( string key, T value, TimeSpan timespan ) where T : class
        {
            _cache.Set( key, value, timespan );
        }

        public async Task<T> GetOrSetAsync< T >( string key, Func<Task<T>> expression, TimeSpan expiresIn ) where T : class
        {
            var found = Get<T>( key );
            if ( found != null )
                return await Task.FromResult( found );

            var executed = await Task.Run( expression );
            _cache.Set( key, executed, DateTime.Now + expiresIn );

            return await Task.FromResult( executed );
        }
    }
}