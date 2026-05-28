using System;

namespace Crashmania.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InjectAttribute : Attribute
    {
    }
}
