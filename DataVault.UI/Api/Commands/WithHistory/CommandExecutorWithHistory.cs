using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Reflection;
using DataVault.UI.Api.Commands.WithExecutor;

namespace DataVault.UI.Api.Commands.WithHistory
{
    public class CommandExecutorWithHistory : CommandExecutor, ICommandExecutorWithHistory<ICommand>
    {
        protected override bool AfterExecute(ICommand command)
        {
            if (command.GetType().HasAttr<ResetsHistoryAttribute>())
            {
                ResetHistory();
            }

            if (!command.GetType().HasAttr<GhostableInHistoryAttribute>())
            {
                if (InRedo)
                {
                    ++_nextIndex;
                }
                else
                {
                    if (_savedIndex > _nextIndex) _savedIndex = -1;
                    _history = new List<ICommand>(_history.Take(_nextIndex));
                    _history.Add(command);
                    _nextIndex = _history.Count;

                    if (_history.Count > MaxHistory)
                    {
                        var delta = _history.Count - MaxHistory;
                        _history = new List<ICommand>(_history.Skip(delta));

                        _nextIndex -= delta;
                        _savedIndex -= delta;
                        if (_savedIndex < 0) _savedIndex = -1;
                    }
                }
            }

            return true;
        }

        protected override bool BeforeUnexecute(ICommand command)
        {
            return command == _history[_nextIndex - 1];
        }

        private List<ICommand> _history = new List<ICommand>();
        private int _nextIndex;

        public ReadOnlyCollection<ICommand> UndoableUnits
        {
            get { return new ReadOnlyCollection<ICommand>(_history.Take(_nextIndex).Reverse().ToList()); }
        }

        public ReadOnlyCollection<ICommand> RedoableUnits
        {
            get { return new ReadOnlyCollection<ICommand>(_history.Skip(_nextIndex).ToList()); }
        }

        private int _savedIndex;
        public bool IsDirty { get { return _history.Count > 0 && _nextIndex != _savedIndex; } }
        public void MarkNotDirty() { _savedIndex = _nextIndex; }

        private static readonly int DefaultMaxHistory = 10;
        private int _maxHistory = DefaultMaxHistory;

        public int MaxHistory
        {
            get { return _maxHistory; } 
            set
            {
                var delta = _maxHistory - value;
                if (delta > 0)
                {
                    _history = new List<ICommand>(_history.Skip(delta));

                    _nextIndex -= delta;
                    _savedIndex -= delta;
                    if (_savedIndex < 0) _savedIndex = -1;
                }

                _maxHistory = value;
            }
        }

        public bool IsHistoryEnabled
        {
            get { return _maxHistory != 0; } 
            set
            {
                if (IsHistoryEnabled != value)
                {
                    if (value)
                    {
                        _maxHistory = DefaultMaxHistory;
                    }
                    else
                    {
                        ResetHistory();
                        _maxHistory = 0;
                    }
                }
            }
        }

        public void ResetHistory()
        {
            lock (this)
            {
                _nextIndex = 0;
                _savedIndex = 0;
                _history.Clear();
            }
        }

        public bool CanUndo()
        {
            return _nextIndex > 0;
        }

        public bool InUndo { get; private set; }

        public void Undo()
        {
            lock (this)
            {
                CanUndo().AssertTrue();

                try
                {
                    InUndo = true;
                    if (Unexecute(_history[_nextIndex - 1])) --_nextIndex;
                }
                finally
                {
                    InUndo = false;
                }
            }
        }

        public bool CanRedo()
        {
            return _nextIndex < _history.Count;
        }

        public bool InRedo { get; private set; }

        public void Redo()
        {
            lock (this)
            {
                CanRedo().AssertTrue();

                try
                {
                    InRedo = true;
                    Execute(_history[_nextIndex]);
                }
                finally
                {
                    InRedo = false;
                }
            }
        }
    }
}