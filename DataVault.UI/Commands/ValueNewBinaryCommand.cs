using System;
using DataVault.Core.Api;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class ValueNewBinaryCommand : ValueNewCommand
    {
        public ValueNewBinaryCommand(DataVaultUIContext context) 
            : base(context)
        {
        }

        protected override IValue CreateValue()
        {
            var createdValue = Branch.CreateValue(CalculateFirstUnusedName(), String.Empty);
            createdValue.SetTypeToken2("binary");
            return createdValue;
        }
    }
}