using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Crashmania.Core
{
    public sealed class DependencyContainer
    {
        private static readonly BindingFlags InjectionFlags =
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private static DependencyContainer instance;
        private readonly Dictionary<Type, object> dependencies = new();

        public static DependencyContainer Instance => instance ??= new DependencyContainer();

        public IReadOnlyDictionary<Type, object> Dependencies => dependencies;

        public void Register<T>(object instanceToRegister)
        {
            if (instanceToRegister == null)
            {
                throw new ArgumentNullException(nameof(instanceToRegister));
            }

            var key = typeof(T);
            if (dependencies.ContainsKey(key))
            {
                Debug.LogWarning($"[DependencyContainer] Replacing dependency registered for {key}.");
            }

            dependencies[key] = instanceToRegister;
        }

        public void Bind<T>(object instanceToRegister)
        {
            Register<T>(instanceToRegister);
        }

        public T Resolve<T>()
        {
            if (!dependencies.TryGetValue(typeof(T), out var value))
            {
                throw new InvalidOperationException($"Dependency not found: {typeof(T)}");
            }

            return (T)value;
        }

        public T Find<T>()
        {
            return Resolve<T>();
        }

        public void Inject(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var fields = target.GetType()
                .GetFields(InjectionFlags)
                .Where(field => field.IsDefined(typeof(InjectAttribute), false));

            foreach (var field in fields)
            {
                if (!dependencies.TryGetValue(field.FieldType, out var value))
                {
                    throw new InvalidOperationException($"Dependency not found: {field.FieldType}");
                }

                field.SetValue(target, value);
            }
        }

        public void Clear()
        {
            dependencies.Clear();
        }
    }
}
