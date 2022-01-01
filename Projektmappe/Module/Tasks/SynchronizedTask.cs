namespace GVRP.Module.Tasks
{
    public abstract class SynchronizedTask
    {
        public abstract void Execute();

        public virtual bool CanExecute()
        {
            return true;
        }
    }
}