using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System.Data.Entity;

namespace DaOAuth.Dal.EF
{
    internal class DaOAuthContext : DbContext, IContext
    {
        public DaOAuthContext(string cs) : base(cs)
        {
            Database.SetInitializer<DaOAuthContext>(null);
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Code> Codes { get; set; }
        public DbSet<ClientType> ClientsTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("auth");

            modelBuilder.Entity<ClientType>().ToTable("ClientsTypes");
            modelBuilder.Entity<ClientType>().HasKey<int>(c => c.Id);
            modelBuilder.Entity<ClientType>().Property(p => p.Wording).HasColumnName("Wording").HasColumnType("nvarchar").HasMaxLength(256).IsRequired();

            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Client>().HasKey<int>(c => c.Id);
            modelBuilder.Entity<Client>().Property(p => p.CreationDate).HasColumnName("CreationDate").HasColumnType("datetime2").IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.DefautRedirectUri).HasColumnName("DefautRedirectUri").HasColumnType("nvarchar(max)").IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.Name).HasColumnName("Name").HasColumnType("nvarchar").HasMaxLength(256).IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.PublicId).HasColumnName("PublicId").HasColumnType("nvarchar").HasMaxLength(256).IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.ClientSecret).HasColumnName("ClientSecret").HasColumnType("varbinary").HasMaxLength(50);
            
            modelBuilder.Entity<Client>().HasMany<Code>(c => c.Codes).WithRequired(c => c.Client).WillCascadeOnDelete();
            modelBuilder.Entity<Client>().Property(p => p.ClientTypeId).HasColumnName("FK_ClientType").HasColumnType("int").IsRequired();
            modelBuilder.Entity<Client>().HasRequired<ClientType>(c => c.ClientType).WithMany(ct => ct.Clients).HasForeignKey<int>(ct => ct.ClientTypeId);

            modelBuilder.Entity<Code>().ToTable("Codes");
            modelBuilder.Entity<Code>().HasKey<int>(c => c.Id);
            modelBuilder.Entity<Code>().Property(p => p.CodeValue).HasColumnName("CodeValue").HasColumnType("nvarchar").HasMaxLength(256).IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.ExpirationTimeStamp).HasColumnName("ExpirationTimeStamp").HasColumnType("bigint").IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.ClientId).HasColumnName("FK_Client").HasColumnType("int").IsRequired();
            modelBuilder.Entity<Code>().HasRequired<Client>(c => c.Client).WithMany(g => g.Codes).HasForeignKey<int>(c => c.ClientId);
        }

        public void Commit()
        {            
            this.SaveChanges();
        }

        public async void CommitAsync()
        {
            await this.SaveChangesAsync();
        }
    }
}
