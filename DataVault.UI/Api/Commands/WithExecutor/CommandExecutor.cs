using System;

namespace DataVault.UI.Api.Commands.WithExecutor
{
    // todo. implement something to check command dependencies
    //
    // currently we have to manually implement protection against simultaneous execution
    // (and by the way in a very crude way - one command at a time, and no commands even on CommandXXXed)
    //
    // we could do better: e.g. heuristics+hint to detect dependencies and allow parallel or
    // at least sequential (in XXXed events) execution of certain commands

    public class CommandExecutor : ICommandExecutor
    {
        public event EventHandler<CommandExecutingEventArgs> CommandExecuting;
        public event EventHandler<CommandExecutedEventArgs> CommandExecutionCancelled;
        public event EventHandler<CommandExecutedEventArgs> CommandExecuted;
        public event EventHandler<CommandExecutedEventArgs> CommandUnexecutionCancelled;
        public event EventHandler<CommandExecutingEventArgs> CommandUnexecuting;
        public event EventHandler<CommandExecutedEventArgs> CommandUnexecuted;

        public bool InExecute { get; private set; }
        public bool InUnexecute { get; private set; }
        public ICommand ActiveCommand { get; private set; }

        public virtual bool Execute(ICommand command)
        {
            lock (this)
            {
                try
                {
                    InExecute = true;
                    ActiveCommand = command;

                    if (CommandExecuting != null)
                    {
                        var e = new CommandExecutingEventArgs(command);
                        CommandExecuting(this, e);

                        if (e.Cancel)
                        {
                            if (CommandExecutionCancelled != null)
                                CommandExecutionCancelled(this, new CommandExecutedEventArgs(command));

                            return false;
                        }
                    }

                    if (!BeforeExecute(command))
                    {
                        if (CommandExecutionCancelled != null)
                            CommandExecutionCancelled(this, new CommandExecutedEventArgs(command));

                        return false;
                    }

                    if (!DoExecute(command))
                    {
                        if (CommandExecutionCancelled != null)
                            CommandExecutionCancelled(this, new CommandExecutedEventArgs(command));

                        return false;
                    }

                    if (!AfterExecute(command))
                    {
                        if (CommandExecutionCancelled != null)
                            CommandExecutionCancelled(this, new CommandExecutedEventArgs(command));

                        return false;
                    }

                    if (CommandExecuted != null)
                        CommandExecuted(this, new CommandExecutedEventArgs(command));

                    return true;
                }
                catch (Exception)
                {
                    if (CommandExecutionCancelled != null)
                        CommandExecutionCancelled(this, new CommandExecutedEventArgs(command));

                    throw;
                }
                finally
                {
                    InExecute = false;
                    ActiveCommand = null;
                }
            }
        }

        protected virtual bool BeforeExecute(ICommand command)
        {
            return true;
        }

        protected virtual bool DoExecute(ICommand command)
        {
            try
            {
                command.Do();
                return true;
            }
            catch (CommandExecutionCancelledException)
            {
                return false;
            }
        }

        protected virtual bool AfterExecute(ICommand command)
        {
            return true;
        }

        public virtual bool Unexecute(ICommand command)
        {
            lock (this)
            {
                try
                {
                    InUnexecute = true;
                    ActiveCommand = command;

                    if (CommandUnexecuting != null)
                    {
                        var e = new CommandExecutingEventArgs(command);
                        CommandUnexecuting(this, e);

                        if (e.Cancel)
                        {
                            if (CommandUnexecutionCancelled != null)
                                CommandUnexecutionCancelled(this, new CommandExecutedEventArgs(command));

                            return false;
                        }
                    }

                    if (!BeforeUnexecute(command))
                    {
                        if (CommandUnexecutionCancelled != null)
                            CommandUnexecutionCancelled(this, new CommandExecutedEventArgs(command));

                        return false;
                    }

                    if (!DoUnexecute(command))
                    {
                        if (CommandUnexecutionCancelled != null)
                            CommandUnexecutionCancelled(this, new CommandExecutedEventArgs(command));

                        return false;
                    }

                    if (!AfterUnexecute(command))
                    {
                        if (CommandUnexecutionCancelled != null)
                            CommandUnexecutionCancelled(this, new CommandExecutedEventArgs(command));

                        return false;
                    }

                    if (CommandUnexecuted != null)
                        CommandUnexecuted(this, new CommandExecutedEventArgs(command));

                    return true;
                }
                catch (Exception)
                {
                    if (CommandUnexecutionCancelled != null)
                        CommandUnexecutionCancelled(this, new CommandExecutedEventArgs(command));

                    throw;
                }
                finally
                {
                    InUnexecute = false;
                    ActiveCommand = null;
                }
            }
        }

        protected virtual bool BeforeUnexecute(ICommand command)
        {
            return true;
        }

        protected virtual bool DoUnexecute(ICommand command)
        {
            try
            {
                command.Undo();
                return true;
            }
            catch (CommandExecutionCancelledException)
            {
                return false;
            }
        }

        protected virtual bool AfterUnexecute(ICommand command)
        {
            return true;
        }
    }
}