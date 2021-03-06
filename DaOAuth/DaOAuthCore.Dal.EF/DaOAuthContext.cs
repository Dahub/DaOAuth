﻿using DaOAuthCore.Dal.Interface;
using DaOAuthCore.Domain;
using Microsoft.EntityFrameworkCore;
using System;

namespace DaOAuthCore.Dal.EF
{
    public class DaOAuthContext : DbContext, IContext
    {
        public DaOAuthContext(DbContextOptions options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Code> Codes { get; set; }
        public DbSet<ClientType> ClientsTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserClient> UsersClients { get; set; }
        public DbSet<Scope> Scopes { get; set; }
        public DbSet<ClientScope> ClientsScopes { get; set; }
        public DbSet<RessourceServer> RessourceServers { get; set; }
        public DbSet<ClientReturnUrl> ClientReturnUrl { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("auth");

            modelBuilder.Entity<ClientReturnUrl>().ToTable("ClientReturnUrls");
            modelBuilder.Entity<ClientReturnUrl>().HasKey(rs => rs.Id);
            modelBuilder.Entity<ClientReturnUrl>().Property(p => p.ReturnUrl).HasColumnName("ReturnUrl").HasColumnType("nvarchar(max)").IsRequired();
            modelBuilder.Entity<ClientReturnUrl>().Property(p => p.ClientId).HasColumnName("FK_Client").HasColumnType("int").IsRequired();
            modelBuilder.Entity<ClientReturnUrl>().HasOne<Client>(c => c.Client).WithMany(g => g.ClientReturnUrls).HasForeignKey(c => c.ClientId);

            modelBuilder.Entity<RessourceServer>().ToTable("RessourceServers");
            modelBuilder.Entity<RessourceServer>().HasKey(rs => rs.Id);
            modelBuilder.Entity<RessourceServer>().Property(rs => rs.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            modelBuilder.Entity<RessourceServer>().Property(rs => rs.Description).HasColumnName("Description").HasColumnType("nvarchar(max)");
            modelBuilder.Entity<RessourceServer>().Property(rs => rs.Name).HasColumnName("Name").HasColumnType("nvarchar(256)").HasMaxLength(256).IsRequired();
            modelBuilder.Entity<RessourceServer>().Property(rs => rs.Login).HasColumnName("Login").HasColumnType("nvarchar(256)").HasMaxLength(256).IsRequired();
            modelBuilder.Entity<RessourceServer>().Property(rs => rs.ServerSecret).HasColumnName("ServerSecret").HasColumnType("varbinary(50)").HasMaxLength(50);
            modelBuilder.Entity<RessourceServer>().Property(rs => rs.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(c => c.Id);
            modelBuilder.Entity<User>().Property(p => p.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            modelBuilder.Entity<User>().Property(p => p.BirthDate).HasColumnName("BirthDate").HasColumnType("datetime");
            modelBuilder.Entity<User>().Property(p => p.CreationDate).HasColumnName("CreationDate").HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<User>().Property(p => p.FullName).HasColumnName("FullName").HasColumnType("nvarchar(256)").HasMaxLength(256);
            modelBuilder.Entity<User>().Property(p => p.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            modelBuilder.Entity<User>().Property(p => p.Password).HasColumnName("Password").HasColumnType("varbinary(50)").HasMaxLength(50);
            modelBuilder.Entity<User>().Property(p => p.UserName).HasColumnName("UserName").HasColumnType("nvarchar(32)").HasMaxLength(32).IsRequired();

            modelBuilder.Entity<ClientType>().ToTable("ClientsTypes");
            modelBuilder.Entity<ClientType>().HasKey(c => c.Id);
            modelBuilder.Entity<ClientType>().Property(p => p.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            modelBuilder.Entity<ClientType>().Property(p => p.Wording).HasColumnName("Wording").HasColumnType("nvarchar(256)").HasMaxLength(256).IsRequired();

            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Client>().HasKey(c => c.Id);
            modelBuilder.Entity<Client>().Property(p => p.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.CreationDate).HasColumnName("CreationDate").HasColumnType("datetime2").IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.Description).HasColumnName("Description").HasColumnType("nvarchar(max)");
            modelBuilder.Entity<Client>().Property(p => p.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.Name).HasColumnName("Name").HasColumnType("nvarchar(256)").HasMaxLength(256).IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.PublicId).HasColumnName("PublicId").HasColumnType("nvarchar(256)").HasMaxLength(256).IsRequired();
            modelBuilder.Entity<Client>().Property(p => p.ClientSecret).HasColumnName("ClientSecret").HasColumnType("varbinary(50)").HasMaxLength(50);
            modelBuilder.Entity<Client>().HasMany<Code>(c => c.Codes).WithOne(c => c.Client).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Client>().HasMany<ClientScope>(c => c.ClientsScopes).WithOne(c => c.Client).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Client>().Property(p => p.ClientTypeId).HasColumnName("FK_ClientType").HasColumnType("int").IsRequired();
            modelBuilder.Entity<Client>().HasOne<ClientType>(c => c.ClientType).WithMany(ct => ct.Clients).HasForeignKey(ct => ct.ClientTypeId);
            modelBuilder.Entity<Client>().HasMany<ClientReturnUrl>(c => c.ClientReturnUrls).WithOne(c => c.Client).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Scope>().ToTable("Scopes");
            modelBuilder.Entity<Scope>().HasKey(s => s.Id);
            modelBuilder.Entity<Scope>().Property(p => p.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            modelBuilder.Entity<Scope>().Property(p => p.Wording).HasColumnName("Wording").HasColumnType("nvarchar(max)");
            modelBuilder.Entity<Scope>().Property(p => p.NiceWording).HasColumnName("NiceWording").HasColumnType("nvarchar(512)").HasMaxLength(512);
            modelBuilder.Entity<Scope>().HasMany<ClientScope>(c => c.ClientsScopes).WithOne(c => c.Scope).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Code>().ToTable("Codes");
            modelBuilder.Entity<Code>().HasKey(c => c.Id);
            modelBuilder.Entity<Code>().Property(p => p.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.CodeValue).HasColumnName("CodeValue").HasColumnType("nvarchar(256)").HasMaxLength(256).IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.ExpirationTimeStamp).HasColumnName("ExpirationTimeStamp").HasColumnType("bigint").IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.Scope).HasColumnName("Scope").HasColumnType("nvarchar(max)").HasMaxLength(Int32.MaxValue);
            modelBuilder.Entity<Code>().Property(p => p.UserName).HasColumnName("UserName").HasColumnType("nvarchar(32)").HasMaxLength(32).IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.UserPublicId).HasColumnName("UserPublicId").HasColumnType("uniqueidentifier").IsRequired();
            modelBuilder.Entity<Code>().Property(p => p.ClientId).HasColumnName("FK_Client").HasColumnType("int").IsRequired();
            modelBuilder.Entity<Code>().HasOne<Client>(c => c.Client).WithMany(g => g.Codes).HasForeignKey(c => c.ClientId);

            modelBuilder.Entity<UserClient>().ToTable("UsersClients");
            modelBuilder.Entity<UserClient>().HasKey(c => c.Id);
            modelBuilder.Entity<UserClient>().Property(p => p.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            modelBuilder.Entity<UserClient>().Property(p => p.ClientId).HasColumnName("FK_Client").HasColumnType("int").IsRequired();
            modelBuilder.Entity<UserClient>().HasOne<Client>(c => c.Client).WithMany(g => g.UsersClients).HasForeignKey(c => c.ClientId);
            modelBuilder.Entity<UserClient>().Property(p => p.UserId).HasColumnName("FK_User").HasColumnType("int").IsRequired();
            modelBuilder.Entity<UserClient>().Property(p => p.UserPublicId).HasColumnName("UserPublicId").HasColumnType("uniqueidentifier").IsRequired();
            modelBuilder.Entity<UserClient>().HasOne<User>(c => c.User).WithMany(g => g.UsersClients).HasForeignKey(c => c.UserId);
            modelBuilder.Entity<UserClient>().Property(p => p.CreationDate).HasColumnName("CreationDate").HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<UserClient>().Property(p => p.IsValid).HasColumnName("IsValid").HasColumnType("bit").IsRequired();
            modelBuilder.Entity<UserClient>().Property(p => p.RefreshToken).HasColumnName("RefreshToken").HasColumnType("nvarchar(max)").HasMaxLength(Int32.MaxValue);

            modelBuilder.Entity<ClientScope>().ToTable("ClientsScopes");
            modelBuilder.Entity<ClientScope>().HasKey(c => c.Id);
            modelBuilder.Entity<ClientScope>().Property(p => p.ClientId).HasColumnName("FK_Client").HasColumnType("int").IsRequired();
            modelBuilder.Entity<ClientScope>().HasOne<Client>(c => c.Client).WithMany(g => g.ClientsScopes).HasForeignKey(c => c.ClientId);
            modelBuilder.Entity<ClientScope>().Property(p => p.ScopeId).HasColumnName("FK_Scope").HasColumnType("int").IsRequired();
            modelBuilder.Entity<ClientScope>().HasOne<Scope>(c => c.Scope).WithMany(g => g.ClientsScopes).HasForeignKey(c => c.ScopeId);
        }

        public void Commit()
        {
            this.SaveChanges();
        }

        public async void CommitAsync()
        {
            await SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
