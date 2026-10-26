using System.Reflection;

namespace NewsArchive.Tests.Support
{
    public class Fake<T> : DispatchProxy where T : class
    {
        private Dictionary<string, Func<object?[], object?>> handlers = new();

        public List<(string Method, object?[] Args)> Calls { get; } = new();

        public static (T Instance, Fake<T> Control) Create()
        {
            var instance = Create<T, Fake<T>>();
            return (instance, (Fake<T>)(object)instance);
        }

        public Fake<T> On(string method, Func<object?[], object?> handler)
        {
            handlers[method] = handler;
            return this;
        }

        public int CountOf(string method) => Calls.Count(c => c.Method == method);

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var name = targetMethod!.Name;
            var arguments = args ?? Array.Empty<object?>();
            Calls.Add((name, arguments));
            if (handlers.TryGetValue(name, out var handler)) return handler(arguments);

            var returnType = targetMethod.ReturnType;
            if (returnType == typeof(void)) return null;
            if (returnType == typeof(bool)) return false;
            if (returnType.IsValueType) return Activator.CreateInstance(returnType);
            throw new NotSupportedException($"{typeof(T).Name}.{name} was called but the test did not set it up.");
        }
    }
}
