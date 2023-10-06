using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using stDataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stDataAccess.DbInitializer
{
    internal class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }
        public void Initialise()
        {
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }catch (Exception ex) { }


/*            if (!_roleManager.RoleExistsAsync(SD.role_cust).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.role_cust)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.role_comp)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.role_emp)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.role_admin)).GetAwaiter().GetResult();

            }*/
        }
    }
}
