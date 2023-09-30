using stDataAccess.Data;
using stDataAccess.Repository.IRepository;
using stModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stDataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) :base(db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Update(obj);
        }
    }
}
