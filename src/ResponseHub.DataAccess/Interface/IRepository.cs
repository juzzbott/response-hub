using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IRepository<T> where T : IEntity
	{

		Task<T> Add(T entity);

		Task Remove(T entity);

		Task<T> Save(T entity);

		Task<IList<T>> GetAll();

		Task<IList<T>> Find(Expression<Func<T, bool>> predicate);

		Task<T> FindOne(Expression<Func<T, bool>> predicate);

		Task<T> GetById(Guid id);

	}
}
