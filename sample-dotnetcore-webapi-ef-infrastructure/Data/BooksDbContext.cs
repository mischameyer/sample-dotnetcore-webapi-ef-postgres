using Microsoft.EntityFrameworkCore;
using sample_dotnetcore_webapi_ef_infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace sample_dotnetcore_webapi_ef_infrastructure.Data
{
    public class BooksDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public BooksDbContext(DbContextOptions<BooksDbContext> options)
            : base(options)
        { }

    }
}
