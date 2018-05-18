﻿// <auto-generated />
using DaOAuthCore.Dal.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace DaOAuthCore.WebServer.Migrations
{
    [DbContext(typeof(DaOAuthContext))]
    [Migration("20180518201800_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("auth")
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DaOAuthCore.Domain.Client", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("ClientSecret")
                        .HasColumnName("ClientSecret")
                        .HasColumnType("varbinary(50)")
                        .HasMaxLength(50);

                    b.Property<int>("ClientTypeId")
                        .HasColumnName("FK_ClientType")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnName("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DefautRedirectUri")
                        .IsRequired()
                        .HasColumnName("DefautRedirectUri")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnName("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsValid")
                        .HasColumnName("IsValid")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("PublicId")
                        .IsRequired()
                        .HasColumnName("PublicId")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("ClientTypeId");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("DaOAuthCore.Domain.ClientType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("int");

                    b.Property<string>("Wording")
                        .IsRequired()
                        .HasColumnName("Wording")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("ClientsTypes");
                });

            modelBuilder.Entity("DaOAuthCore.Domain.Code", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("int");

                    b.Property<int>("ClientId")
                        .HasColumnName("FK_Client")
                        .HasColumnType("int");

                    b.Property<string>("CodeValue")
                        .IsRequired()
                        .HasColumnName("CodeValue")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<long>("ExpirationTimeStamp")
                        .HasColumnName("ExpirationTimeStamp")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsValid")
                        .HasColumnName("IsValid")
                        .HasColumnType("bit");

                    b.Property<string>("Scope")
                        .HasColumnName("Scope")
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(2147483647);

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnName("UserName")
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<Guid>("UserPublicId")
                        .HasColumnName("UserPublicId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("Codes");
                });

            modelBuilder.Entity("DaOAuthCore.Domain.Scope", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("int");

                    b.Property<int>("ClientId")
                        .HasColumnName("FK_Client")
                        .HasColumnType("int");

                    b.Property<string>("NiceWording")
                        .HasColumnName("NiceWording")
                        .HasColumnType("nvarchar(512)")
                        .HasMaxLength(512);

                    b.Property<string>("Wording")
                        .HasColumnName("Wording")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("Scopes");
                });

            modelBuilder.Entity("DaOAuthCore.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("int");

                    b.Property<DateTime?>("BirthDate")
                        .HasColumnName("BirthDate")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("CreationDate")
                        .IsRequired()
                        .HasColumnName("CreationDate")
                        .HasColumnType("datetime");

                    b.Property<string>("FullName")
                        .HasColumnName("FullName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<bool>("IsValid")
                        .HasColumnName("IsValid")
                        .HasColumnType("bit");

                    b.Property<byte[]>("Password")
                        .HasColumnName("Password")
                        .HasColumnType("varbinary(50)")
                        .HasMaxLength(50);

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnName("UserName")
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DaOAuthCore.Domain.UserClient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("int");

                    b.Property<int>("ClientId")
                        .HasColumnName("FK_Client")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnName("CreationDate")
                        .HasColumnType("datetime");

                    b.Property<bool>("IsValid")
                        .HasColumnName("IsValid")
                        .HasColumnType("bit");

                    b.Property<string>("RefreshToken")
                        .HasColumnName("RefreshToken")
                        .HasColumnType("nvarchar(512)")
                        .HasMaxLength(512);

                    b.Property<int>("UserId")
                        .HasColumnName("FK_User")
                        .HasColumnType("int");

                    b.Property<Guid>("UserPublicId")
                        .HasColumnName("UserPublicId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("UserId");

                    b.ToTable("UsersClients");
                });

            modelBuilder.Entity("DaOAuthCore.Domain.Client", b =>
                {
                    b.HasOne("DaOAuthCore.Domain.ClientType", "ClientType")
                        .WithMany("Clients")
                        .HasForeignKey("ClientTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DaOAuthCore.Domain.Code", b =>
                {
                    b.HasOne("DaOAuthCore.Domain.Client", "Client")
                        .WithMany("Codes")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DaOAuthCore.Domain.Scope", b =>
                {
                    b.HasOne("DaOAuthCore.Domain.Client", "Client")
                        .WithMany("Scopes")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DaOAuthCore.Domain.UserClient", b =>
                {
                    b.HasOne("DaOAuthCore.Domain.Client", "Client")
                        .WithMany("UsersClients")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DaOAuthCore.Domain.User", "User")
                        .WithMany("UsersClients")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
