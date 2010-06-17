using DataVault.Core.Api;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class ValueEditMetadataFinishCommand : ValueRelatedContextBoundCommand
    {
        private IMetadata OldMetadata { get; set; }
        private IMetadata NewMetadata { get; set; }

        public ValueEditMetadataFinishCommand(DataVaultUIContext context, IMetadata newMetadata)
            : base(context)
        {
            NewMetadata = newMetadata;
        }

        public override void DoImpl()
        {
            OldMetadata = Value.Metadata.Clone();

            Value.SetEntireMetadata(NewMetadata);
            RefreshListAndThenSelect(Value);
        }

        public override void UndoImpl()
        {
            Value.SetEntireMetadata(OldMetadata);
            RefreshListAndThenSelect(Value);
        }
    }
}