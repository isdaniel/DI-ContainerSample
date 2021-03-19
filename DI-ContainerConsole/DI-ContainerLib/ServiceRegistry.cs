using System;
using System.Reflection;

namespace DI_ContainerLib
{
    public class ServiceRegistry
    {
        public Type ServiceType { get; set; }

        public Func<DIContainer,Type[],object> Factory { get; set; }
        public LifetimeType Lifetime { get; set; }

        public ServiceRegistry Next { get; set; }

        public ServiceRegistry(Type serviceType, LifetimeType lifetime, Func<DIContainer, Type[], object> factory)
        {
            ServiceType = serviceType;
            Lifetime = lifetime;
            Factory = factory;
        }
    }
}