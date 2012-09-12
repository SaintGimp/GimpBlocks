using System;
using StructureMap;
using StructureMap.Interceptors;
using StructureMap.TypeRules;

namespace GimpBlocks
{
    public class EventAggregatorTypeInterceptor : TypeInterceptor
    {
        public object Process(object target, IContext context)
        {
            EventAggregator.Instance.AddListener(target);
            return target;
        }

        public bool MatchesType(Type type)
        {
            return type.ImplementsInterfaceTemplate(typeof(IListener<>));
        }
    }
}