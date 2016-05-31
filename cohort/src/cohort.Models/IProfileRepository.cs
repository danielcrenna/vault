namespace cohort
{
    public interface IProfileRepository
    {
        void Save(Profile profile);
        Profile GetByKey(string key);
    }
}