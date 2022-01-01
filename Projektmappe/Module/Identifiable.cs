namespace GVRP.Module
{
    public interface Identifiable<out T>
    {
        T GetIdentifier();
    }
}