using System;
using System.Linq;
using DataVault.Core.Helpers.Reflection.Shortcuts;

namespace DataVault.UI.Api.ContentTypez
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ContentTypeLocAttribute : Attribute
    {
        public Type ResourcesClass { get; private set; }

        private String TypeNameKey { get; set; }
        public String TypeName
        {
            get
            {
                return (String)ResourcesClass.GetProperties(BF.All)
                    .Single(p => p.Name == TypeNameKey).GetValue(null, null);
            }
        }

        private String NewValueKey { get; set; }
        public String NewValue
        {
            get
            {
                return (String)ResourcesClass.GetProperties(BF.All)
                    .Single(p => p.Name == NewValueKey).GetValue(null, null);
            }
        }

        private String ValidationFailedKey { get; set; }
        public String ValidationFailed
        {
            get
            {
                return (String)ResourcesClass.GetProperties(BF.All)
                    .Single(p => p.Name == ValidationFailedKey).GetValue(null, null);
            }
        }

        public ContentTypeLocAttribute(Type resourcesClass, String typeNameKey, String newValueKey, String validationFailedKey)
        {
            ResourcesClass = resourcesClass;
            TypeNameKey = typeNameKey;
            NewValueKey = newValueKey;
            ValidationFailedKey = validationFailedKey;
        }
    }
}