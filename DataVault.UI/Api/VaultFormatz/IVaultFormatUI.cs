using DataVault.UI.Api.VaultFormatz.Dialogs;

namespace DataVault.UI.Api.VaultFormatz
{
    public interface IVaultFormatUI
    {
        void InjectIntoVaultDialog(IVaultDialog dialog);
    }
}