namespace tuxedo
{
    public interface Dialect
    {
        char StartIdentifier { get; }
        char EndIdentifier { get; }
        char Separator { get; }
        int ParametersPerQuery { get; }
        string Identity { get; }
    }
}
