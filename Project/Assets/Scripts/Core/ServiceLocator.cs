namespace Crashmania.Core
{
    public static class ServiceLocator
    {
        public static void Register<T>(object instance)
        {
            DependencyContainer.Instance.Register<T>(instance);
        }

        public static T Resolve<T>()
        {
            return DependencyContainer.Instance.Resolve<T>();
        }

        public static void Inject(object target)
        {
            DependencyContainer.Instance.Inject(target);
        }

        public static void Clear()
        {
            DependencyContainer.Instance.Clear();
        }
    }
}
