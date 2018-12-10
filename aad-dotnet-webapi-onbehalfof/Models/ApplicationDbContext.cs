using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace aad_dotnet_webapi_onbehalfof.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<ApiUserTokenCache> ApiUserTokenCacheList { get; set; }
    }

    public class ApiUserTokenCache
    {
        [Key]
        public int UserTokenCacheId { get; set; }
        public string webUserUniqueId { get; set; }
        public byte[] cacheBits { get; set; }
        public DateTime LastWrite { get; set; }
    }
}