using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using tophat;
using tuxedo.Dapper;

namespace cohort.Api.Models
{
    public class ResourceRepository<T> : IResourceRepository<T> where T : class, IResource, new()
    {
        public Expression<Func<T, object>> DefaultSorter;
        public ResourceRepository()
        {
            // Derived classes can provide their own default sorter here    
            DefaultSorter = resource => resource.Id;
        }

        public virtual ResourceCollection<T> Get(int? page, int? count, Expression<Func<T, object>> sortedOn = null)
        {
            throw new NotImplementedException();

            var db = UnitOfWork.Current;
            var sort = GetSorter(sortedOn);
            //var total = db.Count<T>(null);
            var pageIndex = page ?? 1;
            var resultsPerPage = count ?? 10;
            //var paged = db.GetPage<T>(null, sort, pageIndex, resultsPerPage);
            //return new ResourceCollection<T>(paged, pageIndex, resultsPerPage, total);
        }

        private List<Sort> GetSorter(Expression<Func<T, object>> sortedOn)
        {
            var sort = new List<Sort>();
            var propertyName = GetPropertyNameInExpression(sortedOn ?? DefaultSorter);
            if (propertyName != null)
            {
                sort.Add(new Sort {Ascending = true, Column = propertyName});
            }
            return sort;
        }

        public virtual T Get(int id)
        {
            var db = UnitOfWork.Current;
            var funnel = db.Query<T>(string.Format("SELECT * FROM {0} WHERE Id = @Id", typeof(T).Name), new { Id = id }).SingleOrDefault();
            return funnel;
        }

        public virtual void Save(T resource)
        {
            var db = UnitOfWork.Current;
            db.Insert(resource);
        }

        private static string GetPropertyNameInExpression(Expression<Func<T, object>> expression)
        {
            string name = null;
            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                var memberExpression = unaryExpression.Operand as MemberExpression;
                if (memberExpression != null)
                {
                    name = memberExpression.Member.Name;
                }
            }
            return name;
        }
    }

    internal class Sort
    {
        public bool Ascending { get; set; }
        public string Column { get; set; }
    }
}