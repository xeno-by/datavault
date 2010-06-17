using System;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.UI.Api.ContentTypez
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ContentTypeAttribute : Attribute
    {
        public String Token { get; private set; }

        public ContentTypeAttribute(String token) 
        {
            (token != "binary").AssertTrue();
            Token = token;
        }
    }
}