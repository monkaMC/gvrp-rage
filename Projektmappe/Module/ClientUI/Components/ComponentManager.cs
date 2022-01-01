using System;
using System.Collections.Generic;

namespace GVRP.Module.ClientUI.Components
{
    public class ComponentManager
    {
        public static ComponentManager Instance { get; } = new ComponentManager();

        private readonly Dictionary<Type, object> components = new Dictionary<Type, object>();

        private ComponentManager()
        {
        }

        public void Register(Component component)
        {
            components[component.GetType()] = component;
        }

        public static T Get<T>() where T : Component
        {
            if (!Instance.components.TryGetValue(typeof(T), out var component)) return null;
            return component as T;
        }
    }
}