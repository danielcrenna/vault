namespace cohort
{
    public interface IActivationRepository
    {
        Activation FindByHash(string hash);
        Activation FindById(int id);
        void Delete(Activation activation);
        void Save(Activation activation);
    }
}