using System;
using System.Collections.Generic;
using DI_ContainerLib;

namespace DI_ContainerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (DIContainer root = new DIContainer())
            {
                root.RegistryType<Root, IRoot>(LifetimeType.Self);
                using (DIContainer child1 = new DIContainer(root))
                {
                    child1.RegistryType<Child, IChild>(LifetimeType.Self);

                    var childObj1 = child1.Resolve<IChild>();
                    childObj1.Val++;
                    Console.WriteLine($"inner childObj1.Val:{childObj1.Val}");

                    child1.Resolve<IRoot>().Val = 100;
                    Console.WriteLine(child1.Resolve<IRoot>().Val);
                }

                Console.WriteLine($"root rootObj1.Val:{root.Resolve<IRoot>().Val}");
                Console.WriteLine($"root childObj1.Val: {root.Resolve<IChild>().Val}");
            }

            Console.ReadKey();
        }
    }


    public class Child : IChild
    {
        public int Val { get; set; }
    }

    public interface IChild
    {
        int Val { get; set; }
    }

    public class Root : IRoot
    {
        public int Val { get; set; }
    }
    public interface IRoot
    {
        int Val { get; set; }
    }

}
