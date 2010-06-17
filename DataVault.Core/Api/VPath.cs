using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DataVault.Core.Helpers;
using System.Linq;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Api
{
    public class VPath
    {
        private readonly String _path;

        private static Object _cacheSyncRoot = new Object();
        private static readonly Dictionary<String, bool> _isValidCache = new Dictionary<String, bool>();
        private static readonly Dictionary<String, String[]> _stepsCache = new Dictionary<String, String[]>();

        // it's important for the name not to end with "$". 
        // todo. correctly implement the regex to express this
        public static readonly String NameFormat = @"[\w \(\)$\.-]*[\w \(\)\.-]?";

        // windows filename regex. didn't enable it since hadn't enough time to test it thoroughly
        // source: http://regexlib.com/REDetails.aspx?regexp_id=965
//            @"[^\\\./:\*\?\" + "\"" + @"<>\|]{1}[^\\/:\*\?\" + "\"" +@"<>\|]{0,254}";

        public static readonly String StepFormat = @"\\(?<step>" + NameFormat + ")";
        public static readonly String PathFormat = "^(" + StepFormat + ")+$";

        private static readonly Regex _stepRegex = new Regex(StepFormat, RegexOptions.Compiled);
        private static readonly Regex _pathRegex = new Regex(PathFormat, RegexOptions.Compiled);

        private static VPath _empty = new VPath(String.Empty);
        public static VPath Empty { get { return _empty; } }

        public VPath(String path) 
        {
            _path = Normalize(path);
            Validate();
        }

        private String Normalize(String path)
        {
            if (path == null) path = String.Empty;
            path = path.Replace("/", @"\");
            if (!path.StartsWith(@"\")) path = @"\" + path;
            if (path.EndsWith(@"\")) path = path.Substring(0, path.Length - 1);
            return String.IsNullOrEmpty(path) ? @"\" : path;
        }

        private void Validate()
        {
            lock (_cacheSyncRoot)
            {
                if (!_isValidCache.ContainsKey(_path))
                {
                    _isValidCache.Add(_path, _pathRegex.IsMatch(_path));
                }

                if (!_isValidCache[_path])
                {
                    throw new ArgumentException(String.Format(
                        "VPath '{0}' is of invalid format (expected '{1}')", _path, PathFormat));
                }
            }
        }

        public static implicit operator String(VPath vpath)
        {
            return vpath.Path;
        }

        public static implicit operator VPath(String path)
        {
            return new VPath(path);
        }

        public static implicit operator VPath(String[] steps)
        {
            return new VPath(steps.StringJoin(@"\"));
        }

        public override String ToString()
        {
            return (String)this;
        }

        public static bool operator ==(VPath left, VPath right) { return Equals(left, right); }
        public static bool operator !=(VPath left, VPath right) { return !Equals(left, right); }

        public bool Equals(VPath obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            return Equals(obj._path, _path);
        }

        public override bool Equals(Object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != typeof(VPath)) return false;
            return Equals((VPath)obj);
        }

        public override int GetHashCode()
        {
            return (_path != null ? _path.GetHashCode() : 0);
        }

        public String Name
        {
            get
            {
                return _path.Substring(_path.LastIndexOf(@"\"));
            }
        }

        public String Path
        {
            get
            {
                return _path;
            }
        }

        public String[] Steps
        {
            get
            {
                lock (_cacheSyncRoot)
                {
                    if (!_stepsCache.ContainsKey(_path))
                    {
                        _stepsCache.Add(_path, StepsImpl.ToArray());
                    }

                    return _stepsCache[_path];
                }
            }
        }

        private IEnumerable<String> StepsImpl
        {
            get
            {
                if (this != Empty)
                {
                    for (var current = _stepRegex.Match(Path); current.Success; current = current.NextMatch())
                        yield return current.Result("${step}");
                }
            }
        }

        public VPath Parent
        {
            get
            {
                return this == Empty ? null : _path.Substring(0, _path.LastIndexOf(@"\"));
            }
        }

        public static VPath operator +(VPath p1, VPath p2)
        {
            if (p1 == Empty) return p2;
            if (p2 == Empty) return p1;
            return p1._path + p2._path;
        }

        public static bool operator >(VPath p1, VPath p2)
        {
            return p1.Steps.Zip(p2.Steps, (p1s, p2s) => p1s == p2s).All() && p1.Steps.Count() > p2.Steps.Count();
        }

        public static bool operator <(VPath p1, VPath p2)
        {
            return p2 > p1;
        }

        public static VPath operator -(VPath p1, VPath p2)
        {
            (p1 > p2).AssertTrue();
            return p1.Steps.Skip(p2.Steps.Count()).ToArray();
        }
    }
}