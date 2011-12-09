using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap.Configuration.DSL;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using StructureMap;
using StructureMap.TypeRules;

namespace GimpBlocks
{
    public class GameRegistry : Registry
    {
        public GameRegistry()
        {
            Scan(x =>
            {
                x.TheCallingAssembly();
                x.WithDefaultConventions();
            });

            MakeSingleton<ICamera>();
            MakeSingleton<ICameraController>();
            MakeSingleton<IEventAggregator>();
            MakeSingleton<IInputMapper>();
            MakeSingleton<ISettings>();
            MakeSingleton<Statistics>();
            
            For<IInputState>().Use<XnaInputState>();

            RegisterInterceptor(new EventAggregatorTypeInterceptor());
        }

        private void MakeSingleton<T>()
        {
            For<T>().Singleton();

            if (typeof(T).IsConcrete())
            {
                For<T>().Use<T>();
            }
        }
    }
}
