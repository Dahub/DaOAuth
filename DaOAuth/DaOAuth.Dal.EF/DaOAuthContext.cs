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
        public DbSet<User> Users { get; set; }
        public DbSet<UserClient> UsersClients { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("auth");

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey<int>(c => c.Id);
            modelBuilder.Entity<User>().Property(p => p.BirthDate).HasColumnName("BirthDate").HasColumnType("datetime").IsOptional();
            modelBuilder.Entity<User>().Property(p => p.CreationDate).HasColumnName("CreationDate").HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<User>().Property(p => p.FullName).HasColumnName("FullName").HasColumnType("nvarchar").HasMaxLength(256).IsOptional();
            modelBuilder.Entity<User>().Property(p => p.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            modelBuilder.Entity<User>().Property(p => p.Password).HasColumnName("Password").HasColumnType("varbinary").HasMaxLength(50);
            modelBuilder.Entity<User>().Property(p => p.UserName).HasColumnName("UserName").HasColumnType("nvarchar").HasMaxLength(32).IsRequired();
          
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

            modelBuilder.Entity<UserClient>().ToTable("UsersClients");
            modelBuilder.Entity<UserClient>().HasKey<int>(c => c.Id);
            modelBuilder.Entity<UserClient>().Property(p => p.ClientId).HasColumnName("FK_Client").HasColumnType("int").IsRequired();
            modelBuilder.Entity<UserClient>().HasRequired<Client>(c => c.Client).WithMany(g => g.UsersClients).HasForeignKey<int>(c => c.ClientId);
            modelBuilder.Entity<UserClient>().Property(p => p.UserId).HasColumnName("FK_User").HasColumnType("int").IsRequired();
            modelBuilder.Entity<UserClient>().HasRequired<User>(c => c.User).WithMany(g => g.UsersClients).HasForeignKey<int>(c => c.UserId);
            modelBuilder.Entity<UserClient>().Property(p => p.CreationDate).HasColumnName("CreationDate").HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<UserClient>().Property(p => p.UserPublicId).HasColumnName("UserPublicId").HasColumnType("uniqueidentifier").IsRequired();
            modelBuilder.Entity<UserClient>().Property(p => p.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            modelBuilder.Entity<UserClient>().Property(p => p.RefreshToken).HasColumnName("RefreshToken").HasColumnType("nvarchar").HasMaxLength(512);
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
