using DataVault.Core.Api;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class BranchEditMetadataFinishCommand : ContextBoundCommand
    {
        private IMetadata OldMetadata { get; set; }
        private IMetadata NewMetadata { get; set; }

        public BranchEditMetadataFinishCommand(DataVaultUIContext context, IMetadata newMetadata)
            : base(context)
        {
            NewMetadata = newMetadata;
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null;
        }

        public override void DoImpl()
        {
            OldMetadata = Branch.Metadata.Clone();
            Branch.SetEntireMetadata(NewMetadata);
        }

        public override void UndoImpl()
        {
            Branch.SetEntireMetadata(OldMetadata);
        }
    }
}