using System.Collections.ObjectModel;
using DataVault.UI.Api.Commands.WithExecutor;

namespace DataVault.UI.Api.Commands.WithHistory
{
    public interface ICommandExecutorWithHistory<TAtom> : ICommandExecutor
    {
        ReadOnlyCollection<TAtom> UndoableUnits { get; }
        ReadOnlyCollection<TAtom> RedoableUnits { get; }

        bool IsDirty { get; }
        void MarkNotDirty();

        bool IsHistoryEnabled { get; set; }
        int MaxHistory { get; set; }
        void ResetHistory();

        bool CanUndo();
        void Undo();
        bool InUndo { get; }

        bool CanRedo();
        void Redo();
        bool InRedo { get; }
    }
}