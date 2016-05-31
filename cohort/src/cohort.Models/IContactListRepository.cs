using System.Collections.Generic;

namespace cohort.Models
{
    public interface IContactListRepository
    {
        IEnumerable<Contact> GetAll();
    }
}