//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataVault.Core.Helpers.Events
{
    public class CallbackChain
    {
        private class ChainElement
        {
            public global::System.Func<global::System.Boolean> Filter { get; private set; }
            public global::System.Action Listener { get; private set; }
            public global::System.Double Sequence { get; private set; }

            public ChainElement(global::System.Func<global::System.Boolean> filter, global::System.Action listener, global::System.Double sequence)
            {
                Filter = filter;
                Listener = listener;
                Sequence = sequence;
            }
        }

        private class CallbackChainRegistration : global::System.IDisposable
        {
            private bool _isDisposed = false;
            private readonly global::System.Action _unregistrationLogic;

            public CallbackChainRegistration(global::System.Action unregistrationLogic)
            {
                _unregistrationLogic = unregistrationLogic;
            }

            public void Dispose()
            {
               if (!_isDisposed)
               {
                   _isDisposed = true;
                   _unregistrationLogic();
               }
            }
        }

        private readonly global::System.Collections.Generic.List<ChainElement> _chain = new global::System.Collections.Generic.List<ChainElement>();

        public global::System.IDisposable Add(global::System.Func<global::System.Boolean> filter, global::System.Action listener, global::System.Double sequence)
        {
            var element = new ChainElement(filter, listener, sequence);
            _chain.Add(element);
            _chain.Sort((el1, el2) => global::System.Collections.Generic.Comparer<global::System.Double>.Default.Compare(el1.Sequence, el2.Sequence));
            return new CallbackChainRegistration(() => _chain.Remove(element));
        }

        public void Invoke()
        {
            foreach (var el in _chain)
            {
                if (el.Filter()) 
                {
                    el.Listener();
                }
            }
        }
        
        public override global::System.String ToString()
        {
            var hierarchy = global::DataVault.Core.Helpers.Reflection.ReflectionHelper.LookupInheritanceChain(this.GetType());
            var veryThisType = global::System.Linq.Enumerable.ElementAt(global::System.Linq.Enumerable.Reverse(hierarchy), 1);
            var payload = global::DataVault.Core.Helpers.Reflection.GenericsHelper.XGetGenericArguments(veryThisType);
            
            var payloadTypes = global::System.Linq.Enumerable.Select(payload, t => global::DataVault.Core.Helpers.ToStringHelper.GetUnqualifiedCSharpTypeName(t));
            var payloadMacro = global::DataVault.Core.Helpers.EnumerableExtensions.StringJoin(payloadTypes);
            return global::System.String.Format("{{Listeners: {0}, payload schema: [{1}], type: passive listener}}", _chain.Count, payloadMacro);
        }
    }
}

namespace DataVault.Core.Helpers.Events
{
    public class CallbackChain<T>
    {
        private class ChainElement
        {
            public global::System.Func<T, global::System.Boolean> Filter { get; private set; }
            public global::System.Action<T> Listener { get; private set; }
            public global::System.Double Sequence { get; private set; }

            public ChainElement(global::System.Func<T, global::System.Boolean> filter, global::System.Action<T> listener, global::System.Double sequence)
            {
                Filter = filter;
                Listener = listener;
                Sequence = sequence;
            }
        }

        private class CallbackChainRegistration : global::System.IDisposable
        {
            private bool _isDisposed = false;
            private readonly global::System.Action _unregistrationLogic;

            public CallbackChainRegistration(global::System.Action unregistrationLogic)
            {
                _unregistrationLogic = unregistrationLogic;
            }

            public void Dispose()
            {
               if (!_isDisposed)
               {
                   _isDisposed = true;
                   _unregistrationLogic();
               }
            }
        }

        private readonly global::System.Collections.Generic.List<ChainElement> _chain = new global::System.Collections.Generic.List<ChainElement>();

        public global::System.IDisposable Add(global::System.Func<T, global::System.Boolean> filter, global::System.Action<T> listener, global::System.Double sequence)
        {
            var element = new ChainElement(filter, listener, sequence);
            _chain.Add(element);
            _chain.Sort((el1, el2) => global::System.Collections.Generic.Comparer<global::System.Double>.Default.Compare(el1.Sequence, el2.Sequence));
            return new CallbackChainRegistration(() => _chain.Remove(element));
        }

        public void Invoke(T arg0)
        {
            foreach (var el in _chain)
            {
                if (el.Filter(arg0)) 
                {
                    el.Listener(arg0);
                }
            }
        }
        
        public override global::System.String ToString()
        {
            var hierarchy = global::DataVault.Core.Helpers.Reflection.ReflectionHelper.LookupInheritanceChain(this.GetType());
            var veryThisType = global::System.Linq.Enumerable.ElementAt(global::System.Linq.Enumerable.Reverse(hierarchy), 1);
            var payload = global::DataVault.Core.Helpers.Reflection.GenericsHelper.XGetGenericArguments(veryThisType);
            
            var payloadTypes = global::System.Linq.Enumerable.Select(payload, t => global::DataVault.Core.Helpers.ToStringHelper.GetUnqualifiedCSharpTypeName(t));
            var payloadMacro = global::DataVault.Core.Helpers.EnumerableExtensions.StringJoin(payloadTypes);
            return global::System.String.Format("{{Listeners: {0}, payload schema: [{1}], type: passive listener}}", _chain.Count, payloadMacro);
        }
    }
}

namespace DataVault.Core.Helpers.Events
{
    public class CallbackChain<T1, T2>
    {
        private class ChainElement
        {
            public global::System.Func<T1, T2, global::System.Boolean> Filter { get; private set; }
            public global::System.Action<T1, T2> Listener { get; private set; }
            public global::System.Double Sequence { get; private set; }

            public ChainElement(global::System.Func<T1, T2, global::System.Boolean> filter, global::System.Action<T1, T2> listener, global::System.Double sequence)
            {
                Filter = filter;
                Listener = listener;
                Sequence = sequence;
            }
        }

        private class CallbackChainRegistration : global::System.IDisposable
        {
            private bool _isDisposed = false;
            private readonly global::System.Action _unregistrationLogic;

            public CallbackChainRegistration(global::System.Action unregistrationLogic)
            {
                _unregistrationLogic = unregistrationLogic;
            }

            public void Dispose()
            {
               if (!_isDisposed)
               {
                   _isDisposed = true;
                   _unregistrationLogic();
               }
            }
        }

        private readonly global::System.Collections.Generic.List<ChainElement> _chain = new global::System.Collections.Generic.List<ChainElement>();

        public global::System.IDisposable Add(global::System.Func<T1, T2, global::System.Boolean> filter, global::System.Action<T1, T2> listener, global::System.Double sequence)
        {
            var element = new ChainElement(filter, listener, sequence);
            _chain.Add(element);
            _chain.Sort((el1, el2) => global::System.Collections.Generic.Comparer<global::System.Double>.Default.Compare(el1.Sequence, el2.Sequence));
            return new CallbackChainRegistration(() => _chain.Remove(element));
        }

        public void Invoke(T1 arg0, T2 arg1)
        {
            foreach (var el in _chain)
            {
                if (el.Filter(arg0, arg1)) 
                {
                    el.Listener(arg0, arg1);
                }
            }
        }
        
        public override global::System.String ToString()
        {
            var hierarchy = global::DataVault.Core.Helpers.Reflection.ReflectionHelper.LookupInheritanceChain(this.GetType());
            var veryThisType = global::System.Linq.Enumerable.ElementAt(global::System.Linq.Enumerable.Reverse(hierarchy), 1);
            var payload = global::DataVault.Core.Helpers.Reflection.GenericsHelper.XGetGenericArguments(veryThisType);
            
            var payloadTypes = global::System.Linq.Enumerable.Select(payload, t => global::DataVault.Core.Helpers.ToStringHelper.GetUnqualifiedCSharpTypeName(t));
            var payloadMacro = global::DataVault.Core.Helpers.EnumerableExtensions.StringJoin(payloadTypes);
            return global::System.String.Format("{{Listeners: {0}, payload schema: [{1}], type: passive listener}}", _chain.Count, payloadMacro);
        }
    }
}

namespace DataVault.Core.Helpers.Events
{
    public class CallbackChain<T1, T2, T3>
    {
        private class ChainElement
        {
            public global::System.Func<T1, T2, T3, global::System.Boolean> Filter { get; private set; }
            public global::System.Action<T1, T2, T3> Listener { get; private set; }
            public global::System.Double Sequence { get; private set; }

            public ChainElement(global::System.Func<T1, T2, T3, global::System.Boolean> filter, global::System.Action<T1, T2, T3> listener, global::System.Double sequence)
            {
                Filter = filter;
                Listener = listener;
                Sequence = sequence;
            }
        }

        private class CallbackChainRegistration : global::System.IDisposable
        {
            private bool _isDisposed = false;
            private readonly global::System.Action _unregistrationLogic;

            public CallbackChainRegistration(global::System.Action unregistrationLogic)
            {
                _unregistrationLogic = unregistrationLogic;
            }

            public void Dispose()
            {
               if (!_isDisposed)
               {
                   _isDisposed = true;
                   _unregistrationLogic();
               }
            }
        }

        private readonly global::System.Collections.Generic.List<ChainElement> _chain = new global::System.Collections.Generic.List<ChainElement>();

        public global::System.IDisposable Add(global::System.Func<T1, T2, T3, global::System.Boolean> filter, global::System.Action<T1, T2, T3> listener, global::System.Double sequence)
        {
            var element = new ChainElement(filter, listener, sequence);
            _chain.Add(element);
            _chain.Sort((el1, el2) => global::System.Collections.Generic.Comparer<global::System.Double>.Default.Compare(el1.Sequence, el2.Sequence));
            return new CallbackChainRegistration(() => _chain.Remove(element));
        }

        public void Invoke(T1 arg0, T2 arg1, T3 arg2)
        {
            foreach (var el in _chain)
            {
                if (el.Filter(arg0, arg1, arg2)) 
                {
                    el.Listener(arg0, arg1, arg2);
                }
            }
        }
        
        public override global::System.String ToString()
        {
            var hierarchy = global::DataVault.Core.Helpers.Reflection.ReflectionHelper.LookupInheritanceChain(this.GetType());
            var veryThisType = global::System.Linq.Enumerable.ElementAt(global::System.Linq.Enumerable.Reverse(hierarchy), 1);
            var payload = global::DataVault.Core.Helpers.Reflection.GenericsHelper.XGetGenericArguments(veryThisType);
            
            var payloadTypes = global::System.Linq.Enumerable.Select(payload, t => global::DataVault.Core.Helpers.ToStringHelper.GetUnqualifiedCSharpTypeName(t));
            var payloadMacro = global::DataVault.Core.Helpers.EnumerableExtensions.StringJoin(payloadTypes);
            return global::System.String.Format("{{Listeners: {0}, payload schema: [{1}], type: passive listener}}", _chain.Count, payloadMacro);
        }
    }
}

namespace DataVault.Core.Helpers.Events
{
    public class CallbackChain<T1, T2, T3, T4>
    {
        private class ChainElement
        {
            public global::System.Func<T1, T2, T3, T4, global::System.Boolean> Filter { get; private set; }
            public global::System.Action<T1, T2, T3, T4> Listener { get; private set; }
            public global::System.Double Sequence { get; private set; }

            public ChainElement(global::System.Func<T1, T2, T3, T4, global::System.Boolean> filter, global::System.Action<T1, T2, T3, T4> listener, global::System.Double sequence)
            {
                Filter = filter;
                Listener = listener;
                Sequence = sequence;
            }
        }

        private class CallbackChainRegistration : global::System.IDisposable
        {
            private bool _isDisposed = false;
            private readonly global::System.Action _unregistrationLogic;

            public CallbackChainRegistration(global::System.Action unregistrationLogic)
            {
                _unregistrationLogic = unregistrationLogic;
            }

            public void Dispose()
            {
               if (!_isDisposed)
               {
                   _isDisposed = true;
                   _unregistrationLogic();
               }
            }
        }

        private readonly global::System.Collections.Generic.List<ChainElement> _chain = new global::System.Collections.Generic.List<ChainElement>();

        public global::System.IDisposable Add(global::System.Func<T1, T2, T3, T4, global::System.Boolean> filter, global::System.Action<T1, T2, T3, T4> listener, global::System.Double sequence)
        {
            var element = new ChainElement(filter, listener, sequence);
            _chain.Add(element);
            _chain.Sort((el1, el2) => global::System.Collections.Generic.Comparer<global::System.Double>.Default.Compare(el1.Sequence, el2.Sequence));
            return new CallbackChainRegistration(() => _chain.Remove(element));
        }

        public void Invoke(T1 arg0, T2 arg1, T3 arg2, T4 arg3)
        {
            foreach (var el in _chain)
            {
                if (el.Filter(arg0, arg1, arg2, arg3)) 
                {
                    el.Listener(arg0, arg1, arg2, arg3);
                }
            }
        }
        
        public override global::System.String ToString()
        {
            var hierarchy = global::DataVault.Core.Helpers.Reflection.ReflectionHelper.LookupInheritanceChain(this.GetType());
            var veryThisType = global::System.Linq.Enumerable.ElementAt(global::System.Linq.Enumerable.Reverse(hierarchy), 1);
            var payload = global::DataVault.Core.Helpers.Reflection.GenericsHelper.XGetGenericArguments(veryThisType);
            
            var payloadTypes = global::System.Linq.Enumerable.Select(payload, t => global::DataVault.Core.Helpers.ToStringHelper.GetUnqualifiedCSharpTypeName(t));
            var payloadMacro = global::DataVault.Core.Helpers.EnumerableExtensions.StringJoin(payloadTypes);
            return global::System.String.Format("{{Listeners: {0}, payload schema: [{1}], type: passive listener}}", _chain.Count, payloadMacro);
        }
    }
}

namespace DataVault.Core.Helpers.Events
{
    public class CallbackChain<T1, T2, T3, T4, T5>
    {
        private class ChainElement
        {
            public global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, global::System.Boolean> Filter { get; private set; }
            public global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5> Listener { get; private set; }
            public global::System.Double Sequence { get; private set; }

            public ChainElement(global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, global::System.Boolean> filter, global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5> listener, global::System.Double sequence)
            {
                Filter = filter;
                Listener = listener;
                Sequence = sequence;
            }
        }

        private class CallbackChainRegistration : global::System.IDisposable
        {
            private bool _isDisposed = false;
            private readonly global::System.Action _unregistrationLogic;

            public CallbackChainRegistration(global::System.Action unregistrationLogic)
            {
                _unregistrationLogic = unregistrationLogic;
            }

            public void Dispose()
            {
               if (!_isDisposed)
               {
                   _isDisposed = true;
                   _unregistrationLogic();
               }
            }
        }

        private readonly global::System.Collections.Generic.List<ChainElement> _chain = new global::System.Collections.Generic.List<ChainElement>();

        public global::System.IDisposable Add(global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, global::System.Boolean> filter, global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5> listener, global::System.Double sequence)
        {
            var element = new ChainElement(filter, listener, sequence);
            _chain.Add(element);
            _chain.Sort((el1, el2) => global::System.Collections.Generic.Comparer<global::System.Double>.Default.Compare(el1.Sequence, el2.Sequence));
            return new CallbackChainRegistration(() => _chain.Remove(element));
        }

        public void Invoke(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4)
        {
            foreach (var el in _chain)
            {
                if (el.Filter(arg0, arg1, arg2, arg3, arg4)) 
                {
                    el.Listener(arg0, arg1, arg2, arg3, arg4);
                }
            }
        }
        
        public override global::System.String ToString()
        {
            var hierarchy = global::DataVault.Core.Helpers.Reflection.ReflectionHelper.LookupInheritanceChain(this.GetType());
            var veryThisType = global::System.Linq.Enumerable.ElementAt(global::System.Linq.Enumerable.Reverse(hierarchy), 1);
            var payload = global::DataVault.Core.Helpers.Reflection.GenericsHelper.XGetGenericArguments(veryThisType);
            
            var payloadTypes = global::System.Linq.Enumerable.Select(payload, t => global::DataVault.Core.Helpers.ToStringHelper.GetUnqualifiedCSharpTypeName(t));
            var payloadMacro = global::DataVault.Core.Helpers.EnumerableExtensions.StringJoin(payloadTypes);
            return global::System.String.Format("{{Listeners: {0}, payload schema: [{1}], type: passive listener}}", _chain.Count, payloadMacro);
        }
    }
}

namespace DataVault.Core.Helpers.Events
{
    public class CallbackChain<T1, T2, T3, T4, T5, T6>
    {
        private class ChainElement
        {
            public global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, T6, global::System.Boolean> Filter { get; private set; }
            public global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5, T6> Listener { get; private set; }
            public global::System.Double Sequence { get; private set; }

            public ChainElement(global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, T6, global::System.Boolean> filter, global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5, T6> listener, global::System.Double sequence)
            {
                Filter = filter;
                Listener = listener;
                Sequence = sequence;
            }
        }

        private class CallbackChainRegistration : global::System.IDisposable
        {
            private bool _isDisposed = false;
            private readonly global::System.Action _unregistrationLogic;

            public CallbackChainRegistration(global::System.Action unregistrationLogic)
            {
                _unregistrationLogic = unregistrationLogic;
            }

            public void Dispose()
            {
               if (!_isDisposed)
               {
                   _isDisposed = true;
                   _unregistrationLogic();
               }
            }
        }

        private readonly global::System.Collections.Generic.List<ChainElement> _chain = new global::System.Collections.Generic.List<ChainElement>();

        public global::System.IDisposable Add(global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, T6, global::System.Boolean> filter, global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5, T6> listener, global::System.Double sequence)
        {
            var element = new ChainElement(filter, listener, sequence);
            _chain.Add(element);
            _chain.Sort((el1, el2) => global::System.Collections.Generic.Comparer<global::System.Double>.Default.Compare(el1.Sequence, el2.Sequence));
            return new CallbackChainRegistration(() => _chain.Remove(element));
        }

        public void Invoke(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5)
        {
            foreach (var el in _chain)
            {
                if (el.Filter(arg0, arg1, arg2, arg3, arg4, arg5)) 
                {
                    el.Listener(arg0, arg1, arg2, arg3, arg4, arg5);
                }
            }
        }
        
        public override global::System.String ToString()
        {
            var hierarchy = global::DataVault.Core.Helpers.Reflection.ReflectionHelper.LookupInheritanceChain(this.GetType());
            var veryThisType = global::System.Linq.Enumerable.ElementAt(global::System.Linq.Enumerable.Reverse(hierarchy), 1);
            var payload = global::DataVault.Core.Helpers.Reflection.GenericsHelper.XGetGenericArguments(veryThisType);
            
            var payloadTypes = global::System.Linq.Enumerable.Select(payload, t => global::DataVault.Core.Helpers.ToStringHelper.GetUnqualifiedCSharpTypeName(t));
            var payloadMacro = global::DataVault.Core.Helpers.EnumerableExtensions.StringJoin(payloadTypes);
            return global::System.String.Format("{{Listeners: {0}, payload schema: [{1}], type: passive listener}}", _chain.Count, payloadMacro);
        }
    }
}

namespace DataVault.Core.Helpers.Events
{
    public class CallbackChain<T1, T2, T3, T4, T5, T6, T7>
    {
        private class ChainElement
        {
            public global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, T6, T7, global::System.Boolean> Filter { get; private set; }
            public global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5, T6, T7> Listener { get; private set; }
            public global::System.Double Sequence { get; private set; }

            public ChainElement(global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, T6, T7, global::System.Boolean> filter, global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5, T6, T7> listener, global::System.Double sequence)
            {
                Filter = filter;
                Listener = listener;
                Sequence = sequence;
            }
        }

        private class CallbackChainRegistration : global::System.IDisposable
        {
            private bool _isDisposed = false;
            private readonly global::System.Action _unregistrationLogic;

            public CallbackChainRegistration(global::System.Action unregistrationLogic)
            {
                _unregistrationLogic = unregistrationLogic;
            }

            public void Dispose()
            {
               if (!_isDisposed)
               {
                   _isDisposed = true;
                   _unregistrationLogic();
               }
            }
        }

        private readonly global::System.Collections.Generic.List<ChainElement> _chain = new global::System.Collections.Generic.List<ChainElement>();

        public global::System.IDisposable Add(global::DataVault.Core.Helpers.Func<T1, T2, T3, T4, T5, T6, T7, global::System.Boolean> filter, global::DataVault.Core.Helpers.Action<T1, T2, T3, T4, T5, T6, T7> listener, global::System.Double sequence)
        {
            var element = new ChainElement(filter, listener, sequence);
            _chain.Add(element);
            _chain.Sort((el1, el2) => global::System.Collections.Generic.Comparer<global::System.Double>.Default.Compare(el1.Sequence, el2.Sequence));
            return new CallbackChainRegistration(() => _chain.Remove(element));
        }

        public void Invoke(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6)
        {
            foreach (var el in _chain)
            {
                if (el.Filter(arg0, arg1, arg2, arg3, arg4, arg5, arg6)) 
                {
                    el.Listener(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                }
            }
        }
        
        public override global::System.String ToString()
        {
            var hierarchy = global::DataVault.Core.Helpers.Reflection.ReflectionHelper.LookupInheritanceChain(this.GetType());
            var veryThisType = global::System.Linq.Enumerable.ElementAt(global::System.Linq.Enumerable.Reverse(hierarchy), 1);
            var payload = global::DataVault.Core.Helpers.Reflection.GenericsHelper.XGetGenericArguments(veryThisType);
            
            var payloadTypes = global::System.Linq.Enumerable.Select(payload, t => global::DataVault.Core.Helpers.ToStringHelper.GetUnqualifiedCSharpTypeName(t));
            var payloadMacro = global::DataVault.Core.Helpers.EnumerableExtensions.StringJoin(payloadTypes);
            return global::System.String.Format("{{Listeners: {0}, payload schema: [{1}], type: passive listener}}", _chain.Count, payloadMacro);
        }
    }
}

