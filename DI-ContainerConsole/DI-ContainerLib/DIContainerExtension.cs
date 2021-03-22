using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DI_ContainerLib
{
    public static class DIContainerExtension
    {
        public static DIContainer RegistryType(this DIContainer container,LifetimeType lifetime, Type instanceType, Type registryType,Func<DIContainer,Type[],object> factory = null)
        {
            factory = factory ?? ((_, args) => Create(_, instanceType, args));
            container.Register(new ServiceRegistry(registryType, lifetime, factory));
            return container;
        }

        public static DIContainer RegistryType<TInstance, TRegistry>(this DIContainer container, LifetimeType lifetime)
        {
            return RegistryType(container, lifetime, typeof(TInstance), typeof(TRegistry));
        }

        public static DIContainer RegistryType<TInstance>(this DIContainer container, LifetimeType lifetime)
        {
            return RegistryType(container, lifetime, typeof(TInstance), typeof(TInstance));
        }

        public static DIContainer RegistryType<TInstance>(this DIContainer container, LifetimeType lifetime, TInstance instance)
        {
            return RegistryType(container, lifetime, typeof(TInstance), typeof(TInstance),(_,args)=> instance);
        }

        public static T Resolve<T>(this DIContainer container)
        {
            return (T)container.GetService(typeof(T));
        }
        public static IEnumerable<T> Resolves<T>(this DIContainer container)
        {
            return container.Resolve<IEnumerable<T>>();
        }

        private static object Create(DIContainer container, Type type, Type[] genericArguments)
        {
            if (genericArguments.Length > 0)
            {
                type = type.MakeGenericType();
            }

            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"Can't create instance of {type}, which didn't have public constructor.");
            }

            var constructor = constructors.FirstOrDefault();
            var parameterInfos = constructor.GetParameters();
            if (parameterInfos.Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            var argsObject = parameterInfos.Select(x => container.GetService(x.ParameterType)).ToArray();

            return constructor.Invoke(constructor,argsObject);
        }
    }
}
