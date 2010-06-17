using System;

namespace DataVault.UI.Api.Commands.WithHistory
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ResetsHistoryAttribute : GhostableInHistoryAttribute
    {
    }
}