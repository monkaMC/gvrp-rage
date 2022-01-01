namespace GVRP.Module
{
    public abstract class Module<T> : BaseModule where T : Module<T>//, new()
    {
        public static T Instance { get; private set; }

        protected Module()
        {
            Instance = (T) this;
        }
    }
}