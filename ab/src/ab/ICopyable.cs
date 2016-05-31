namespace ab
{
    public interface ICopyable<out T>
    {
        T Copy { get; }
    }
}