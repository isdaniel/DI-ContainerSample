using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DI_ContainerLib
{
    public class DIContainer : IDisposable
    {
        internal readonly DIContainer _parent;
        internal readonly ConcurrentDictionary<Type, ServiceRegistry> _registries;
        private ConcurrentDictionary<string, object> _services;
        private ConcurrentBag<IDisposable> _disposables;
        private volatile bool _isDisposed;
        public DIContainer(DIContainer parent)
        {
            _parent = parent;
            _registries = parent._registries;
            InitProperty();
        }

        public DIContainer()
        {
            _registries = new ConcurrentDictionary<Type, ServiceRegistry>();
            InitProperty();
        }

        private void InitProperty()
        {
            _services = new ConcurrentDictionary<string, object>();
            _disposables = new ConcurrentBag<IDisposable>();
        }

        public void Register(ServiceRegistry registry)
        {
            if (_registries.TryGetValue(registry.ServiceType, out var existing))
            {
                registry.Next = existing;
                _registries[registry.ServiceType] = registry;
            }
            else
            {
                _registries[registry.ServiceType] = registry;
            }
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(DIContainer))
            {
                return this;
            }

            ServiceRegistry registry;
            //IEnumerable<T>
            if (serviceType.IsGenericType && 
                serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments().FirstOrDefault();
                if (!_registries.TryGetValue(elementType,out registry))
                {
                    return Array.CreateInstance(elementType,0);
                }

                var services = registry.AsEnumerable().Select(x => GetServiceCore(x, Type.EmptyTypes)).ToArray();
                var array= Array.CreateInstance(elementType, services.Length);
                services.CopyTo(array, 0);
                return array;

            }

            //Generic
            if (serviceType.IsGenericType && !_registries.ContainsKey(serviceType))
            {
                var definitionType = serviceType.GetGenericTypeDefinition();
                return _registries.TryGetValue(definitionType, out registry)
                    ? GetServiceCore(registry, serviceType.GetGenericArguments())
                    : null;
            }

            //Normal
            return _registries.TryGetValue(serviceType, out registry) 
                ? GetServiceCore(registry, Type.EmptyTypes) : null;
        }


        private object GetServiceCore(ServiceRegistry registry,Type[] arguments)
        {
            switch (registry.Lifetime)
            {
                case LifetimeType.Root:
                    return GetOrCreate(_parent._services,_parent._disposables);
                case LifetimeType.Self:
                    return GetOrCreate(_services, _disposables);
                default:
                {
                    var instance = registry.Factory(this, arguments);

                    if (instance is IDisposable disposable)
                    {
                        _disposables.Add(disposable);
                    }

                    return instance;
                }
            }

            object GetOrCreate(ConcurrentDictionary<string, object> services,
                ConcurrentBag<IDisposable> disposables)
            {
                string key = registry.GetHashCode().ToString();

                if (!services.TryGetValue(key,out var result))
                {
                    result = registry.Factory(this, arguments);
                    if (result is IDisposable disposable)
                    {
                        disposables.Add(disposable);
                    }
                    //store instance on service collection.
                    services[key] = result;
                }

                return result;
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            
            _services.Clear();
            _disposables = new ConcurrentBag<IDisposable>();
        }
    }
}
