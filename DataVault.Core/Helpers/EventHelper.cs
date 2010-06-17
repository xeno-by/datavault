using System;
using System.Collections.Generic;
using System.Reflection;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Reflection.Shortcuts;
using System.Linq;

namespace DataVault.Core.Helpers
{
    public static class EventHelper
    {
        private static FieldInfo GetUnderlyingField(this EventInfo evt)
        {
            var f = evt.ReflectedType.GetField(evt.Name, BF.All);
            if (f != null)
            {
                return f;
            }
            else
            {
                // winforms case
                f = evt.ReflectedType.GetField("on" + evt.Name, BF.All);
                if (f != null)
                {
                    return f;
                }
                else
                {
                    throw AssertionHelper.Fail();
                }
            }
        }

        public static IEnumerable<Delegate> GetInvocationList(this EventInfo evt, Object host)
        {
            var f = evt.GetUnderlyingField().AssertNotNull();
            var d = f.GetValue(host).AssertCast<Delegate>();
            return d == null ? new Delegate[0] : d.GetInvocationList();
        }

        public static void SetInvocationList(this EventInfo evt, Object host, params Delegate[] chain)
        {
            evt.SetInvocationList(host, (IEnumerable<Delegate>)chain);
        }

        public static void SetInvocationList(this EventInfo evt, Object host, IEnumerable<Delegate> chain)
        {
            var combo = chain.IsNullOrEmpty() ? null : 
                chain.Aggregate((agg, curr) => agg == null ? curr : Delegate.Combine(agg, curr));

            var f = evt.GetUnderlyingField().AssertNotNull();
            f.SetValue(host, combo);
        }

        public static void ClearInvocationList(this EventInfo evt, Object host)
        {
            var f = evt.GetUnderlyingField().AssertNotNull();
            f.SetValue(host, null);
        }
    }
}
