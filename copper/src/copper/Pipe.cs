namespace copper
{
    public interface Pipe<out TP, in TC> : Produces<TP>, Consumes<TC>
    {
        
    }
}