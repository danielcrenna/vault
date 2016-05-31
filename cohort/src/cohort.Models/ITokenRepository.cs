using System;

namespace cohort.Models
{
    public interface ITokenRepository
    {
        void Save(Token token);
        Token GetByValue(Guid value);
    }
}