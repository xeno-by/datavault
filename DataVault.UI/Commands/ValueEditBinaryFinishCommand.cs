using System;
using System.IO;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class ValueEditBinaryFinishCommand : ValueEditFinishCommand
    {
        private Stream Content { get; set; }

        public ValueEditBinaryFinishCommand(DataVaultUIContext context, Stream content)
            : base(context)
        {
            Content = content;
        }

        protected override String NewTypeToken
        {
            get { return "binary"; }
        }

        protected override Stream NewContent
        {
            get { return Content; }
        }
    }
}