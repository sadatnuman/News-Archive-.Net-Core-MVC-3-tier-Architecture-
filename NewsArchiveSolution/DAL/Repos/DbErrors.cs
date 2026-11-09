using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal static class DbErrors
    {
        public static bool IsMissingReference(DbUpdateException ex) => ex.InnerException is SqlException { Number: 547 };

        public static bool IsDuplicateKey(DbUpdateException ex) => ex.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
