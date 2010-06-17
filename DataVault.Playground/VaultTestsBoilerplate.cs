using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataVault.Core.Api;
using DataVault.Core.Api.Events;
using DataVault.Core.Helpers;
using DataVault.Core.Impl;
using NUnit.Framework;

namespace DataVault.Playground
{
    public abstract class VaultTestsBoilerplate
    {
        protected abstract IVault OpenVaultImpl(String uri);
        protected abstract IVault LoadVaultBlueprintImpl(String uri);
        protected RevisionHistory _history;

        protected bool _trackingEvents = false;
        protected List<ElementChangingEventArgs> _changing;
        protected List<ElementChangedEventArgs> _changed;

        protected void ShutdownEvents()
        {
            _trackingEvents = false;
        }

        protected void StartupEvents()
        {
            _trackingEvents = true;
        }

        protected IVault OpenVault(String uri)
        {
            return OpenVault(uri, _changing, _changed);
        }

        protected IVault OpenVault(String uri, List<ElementChangingEventArgs> changing, List<ElementChangedEventArgs> changed)
        {
            var vault = OpenVaultImpl(uri);
            if (_trackingEvents) RouteVaultEvents(vault, changing, changed);
            return vault;
        }

        protected IVault LoadVaultBlueprint(String uri)
        {
            return LoadVaultBlueprint(uri, _changing, _changed);
        }

        protected IVault LoadVaultBlueprint(String uri, List<ElementChangingEventArgs> changing, List<ElementChangedEventArgs> changed)
        {
            var vault = LoadVaultBlueprintImpl(uri);
            if (_trackingEvents) RouteVaultEvents(vault, changing, changed);
            return vault;
        }

        private void RouteVaultEvents(IVault vault, List<ElementChangingEventArgs> changing, List<ElementChangedEventArgs> changed)
        {
            vault.Changing(e =>
            {
                changing.Add(e);

                if (e.OldValue is Stream)
                {
                    var s = ((Stream)e.OldValue).AsString();
                    var setter = typeof(ElementEventArgs).GetProperty("OldValue").GetSetMethod(true);
                    setter.Invoke(e, s.MkArray());
                }

                if (e.NewValue is Stream)
                {
                    var s = ((Stream)e.NewValue).AsString();
                    var setter = typeof(ElementEventArgs).GetProperty("NewValue").GetSetMethod(true);
                    setter.Invoke(e, s.MkArray());
                }
            });

            vault.Changed(e =>
            {
                changed.Add(e);

                if (e.OldValue is Stream)
                {
                    var s = ((Stream)e.OldValue).AsString();
                    var setter = typeof(ElementEventArgs).GetProperty("OldValue").GetSetMethod(true);
                    setter.Invoke(e, s.MkArray());
                }

                if (e.NewValue is Stream)
                {
                    var s = ((Stream)e.NewValue).AsString();
                    var setter = typeof(ElementEventArgs).GetProperty("NewValue").GetSetMethod(true);
                    setter.Invoke(e, s.MkArray());
                }
            });
        }

        protected void GenerateEventTests()
        {
            if (_trackingEvents)
            {
                // generates only checks for 'ed events since 'ings will be verified by symmetry test in teardown
                var name = new StackTrace().GetFrame(1).GetMethod().Name;
                using (var sw = new StreamWriter(@"d:\" + name))
                {
                    sw.WriteLine("if (_trackingEvents)");
                    sw.WriteLine("{");
                    sw.WriteLine("    Assert.AreEqual({0}, _changed.Count);", _changed.Count);

                    for (var i = 0; i < _changed.Count; i++)
                    {
                        var e = _changed[i];
                        var verbatimCs = ToVerbatimCSharpCopy(e.ToString());

                        var guidRegex = @"\{?[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\}?";
                        if (new Regex(guidRegex).IsMatch(verbatimCs))
                        {
                            sw.WriteLine("    AssertAreEqualExceptGuids(\"{0}\", _changed[{1}].ToStringThatsFriendlyToUnitTests());", verbatimCs, i);
                        }
                        else
                        {
                            sw.WriteLine("    Assert.AreEqual(\"{0}\", _changed[{1}].ToStringThatsFriendlyToUnitTests());", verbatimCs, i);
                        }
                    }

                    sw.WriteLine("}");
                }
            }
        }

        protected String ToVerbatimCSharpCopy(String s)
        {
            return s.Select(c => (c != '\\' && c != '\"') ? new String(c, 1) : "\\" + c)
                .StringJoin(String.Empty);
        }

        internal static void AssertAreEqualExceptGuids(String s1, String s2)
        {
            // http://www.regexlib.com/REDetails.aspx?regexp_id=212
            var guidRegex = @"\{?[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\}?";
            var replaceRegex = new Regex("Id: " + guidRegex);
            s1 = replaceRegex.Replace(s1, m => "Id: <GUID>");
            s2 = replaceRegex.Replace(s1, m => "Id: <GUID>");

            Assert.AreEqual(s1, s2);
        }

        [SetUp]
        public void SetUp()
        {
            _history = new RevisionHistory();
            _changing = new List<ElementChangingEventArgs>();
            _changed = new List<ElementChangedEventArgs>();
            EnsureErased("vault");
            EnsureErased("vault.zip");
            EnsureErased("testvault");
            EnsureErased("testvault2");
            EnsureErased("testvault3");
            EnsureErased("blueprint");
            EnsureErased("blueprint.zip");
            EnsureErased("blueprint.bak");
        }

        [TearDown]
        public void TearDown()
        {
            EnsureErased("vault");
            EnsureErased("vault.zip");
            EnsureErased("testvault");
            EnsureErased("testvault2");
            EnsureErased("testvault3");
            EnsureErased("blueprint");
            EnsureErased("blueprint.zip");
            EnsureErased("blueprint.bak");
            if (_trackingEvents) VerifyEventSymmetry();
            _changed = null;
            _changing = null;
            _history = null;
        }

        private void VerifyEventSymmetry()
        {
            Assert.AreEqual(_changing.Count, _changed.Count);

            var count = _changing.Count;
            for (var i = 0; i < count; i++)
            {
                var ing = _changing[i];
                var ed = _changed[i];

                Assert.AreEqual(ing.CorrelationId, ed.CorrelationId);
                Assert.AreEqual(ing.Reason, ed.Reason);
                Assert.AreEqual(ing.Subject, ed.Subject);
                Assert.AreEqual(ing.OldValue, ed.OldValue);
                Assert.AreEqual(ing.NewValue, ed.NewValue);
            }
        }

        private void EnsureErased(String uri)
        {
            if (File.Exists(uri)) File.Delete(uri);
            if (Directory.Exists(uri)) Directory.Delete(uri, true);
        }

        protected void AssertState(IValue v, String mdata, String content, bool umdata, bool ucontent)
        {
            Assert.AreEqual(mdata, (String)v.BeforeInv(_history).Metadata);
            Assert.AreEqual(content, v.BeforeInv(_history).ContentString);
            Assert.AreEqual(umdata, ((Value)v.BeforeInv(_history)).SaveMyMetadataPlease);
            Assert.AreEqual(ucontent, ((Value)v.BeforeInv(_history)).SaveMyContentPlease);
        }

        protected void AssertState(IValue v, String mdata, String content)
        {
            Assert.AreEqual(mdata, (String)v.BeforeInv(_history).Metadata);
            Assert.AreEqual(content, v.BeforeInv(_history).ContentString);
        }

        protected void AssertState(IBranch b, String mdata, bool umdata)
        {
            Assert.AreEqual(mdata, (String)b.BeforeInv(_history).Metadata);
            Assert.AreEqual(umdata, ((Branch)b.BeforeInv(_history)).SaveMyMetadataPlease);
        }

        protected void AssertState(IBranch b, String mdata)
        {
            Assert.AreEqual(mdata, (String)b.BeforeInv(_history).Metadata);
        }
    }
}