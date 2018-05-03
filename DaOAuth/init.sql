use master
go

if exists(select * from sys.databases where name='DaOAuth')
	drop database [DaOAuth]
go

create database  [DaOAuth]
go

use [DaOAuth]
go

begin
	exec sp_executesql N'CREATE SCHEMA auth'
end
go

create table auth.ClientsTypes
(
	Id integer not null primary key identity(1,1),
	Wording nvarchar(256) not null
)
go

create table auth.Clients
(
	Id integer not null primary key identity(1,1),
	PublicId nvarchar(256) not null unique,
	ClientSecret varbinary(50) null,
	Name nvarchar(256) not null,
	DefautRedirectUri nvarchar(max) null,
	CreationDate datetime not null,
	IsValid bit not null,
	FK_ClientType integer not null foreign key references auth.ClientsTypes(Id)
) 
go

create table auth.Codes
(
	Id integer not null primary key identity(1,1),
	CodeValue nvarchar(256) not null unique,
	ExpirationTimeStamp bigint not null,
	IsValid bit not null,
	FK_Client integer not null foreign key references auth.Clients(Id)
)
go

create table auth.Users
(
	Id integer not null primary key identity(1,1),
	UserName nvarchar(32) not null unique,
	Password varbinary(50) null,
	FullName nvarchar(256) null,
	BirthDate datetime null,
	CreationDate datetime not null,
	IsValid bit not null
)
go

create table auth.UsersClients
(
	Id integer not null primary key identity(1,1),
	FK_User integer not null foreign key references auth.Users(Id),
	FK_Client integer not null foreign key references auth.Clients(Id),
	CreationDate datetime not null,
	UserPublicId uniqueidentifier not null unique,
	RefreshToken nvarchar(512) null,
	IsValid bit not null
)
go

/* données d'initialisation */
insert into auth.ClientsTypes(wording) values ('public')
insert into auth.ClientsTypes(wording) values ('confidential')
go

/* données de test */
insert into auth.Clients (publicId, ClientSecret, Name, DefautRedirectUri, CreationDate, IsValid, FK_ClientType) 
values ('G7H8q4yBhpinNo6H', HASHBYTES('SHA1', 'abc123456789'), 'test', 'http://perdu.com', getdate(), 1, 2)
go
insert into auth.Clients (publicId, ClientSecret, Name, DefautRedirectUri, CreationDate, IsValid, FK_ClientType) 
values ('5EDsd2EU642NVq7D', HASHBYTES('SHA1', 'def123456789'), 'test spa', 'http://perdu.com', getdate(), 1, 1)
go

select * from auth.Clients
select * from auth.Codes
select * from auth.Users
select * from auth.UsersClients

--delete from auth.UsersClients

-- select HASHBYTES('SHA1', 'abc123456789')

-- 1K__-Az1mK15xxeyCzNxWDw3jU6QPMro2IBBUUF8YyAWSuvn2cAlYEmieGklaYxGjt89XqqhobiTawvmLQhSHN6WdamUIlc0Kr_cMRTIysQCH-lrLOKTbGPx87rCY6TCC3ymrBqN2aQZz53mm-WV2FZUBnTmWOWvAhxomMkK_qqfedxpDqHGHT2U-YUepGq0KKgDdusk4Jr0TwgEjXt8-IB1Nh5Yrtn1vb58Q04Y7rU