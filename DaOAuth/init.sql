use master
go

if exists(select * from sys.databases where name='DaOAuth_Dev')
	drop database [DaOAuth_Dev]
go

create database  [DaOAuth_Dev]
go

use [DaOAuth_Dev]
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
	Description nvarchar(max) null,
	DefautRedirectUri nvarchar(max) null,
	CreationDate datetime not null,
	IsValid bit not null,
	FK_ClientType integer not null foreign key references auth.ClientsTypes(Id)
) 
go

create table auth.RessourceServers
(
	Id integer not null primary key identity(1,1),
	Login nvarchar(256) not null unique,
	ServerSecret varbinary(50) null,
	IsValid bit not null,
	Name nvarchar(256) not null,
	Description nvarchar(max) null
)
go

create table auth.Codes
(
	Id integer not null primary key identity(1,1),
	CodeValue nvarchar(256) not null unique,
	ExpirationTimeStamp bigint not null,
	IsValid bit not null,
	Scope nvarchar(max) null,
	UserName nvarchar(32) not null,
	UserPublicId uniqueidentifier not null,
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
	RefreshToken nvarchar(max) null,
	IsValid bit not null
)
go

create table auth.Scopes
(
	Id integer not null primary key identity(1,1),
	Wording nvarchar(max) not null,
	NiceWording nvarchar(512) not null
)
go

create table auth.ClientsScopes
(
	Id integer not null primary key identity(1,1),
	FK_Client integer not null foreign key references auth.Clients(Id),
	FK_Scope integer not null foreign key references auth.Scopes(Id)
)
go

/* données d'initialisation */
insert into auth.ClientsTypes(wording) values ('public')
insert into auth.ClientsTypes(wording) values ('confidential')
go



/* données de test */
insert into auth.Clients (publicId, ClientSecret, Name, DefautRedirectUri, CreationDate, IsValid, FK_ClientType, Description) 
values ('G7H8q4yBhpinNo6H', HASHBYTES('SHA1', 'abc123456789'), 'test', 'http://perdu.com', getdate(), 1, 2, 'un client de type confidential pour tester')
go
insert into auth.Clients (publicId, ClientSecret, Name, DefautRedirectUri, CreationDate, IsValid, FK_ClientType, Description) 
values ('5EDsd2EU642NVq7D', HASHBYTES('SHA1', 'def123456789'), 'test spa', 'http://perdu.com', getdate(), 1, 1, 'un autre client pour tester, mais de type publique')
go
/* client daget */
insert into auth.Clients (publicId, ClientSecret, Name, DefautRedirectUri, CreationDate, IsValid, FK_ClientType, Description) 
values ('dEx5f12sPLEN5S09', HASHBYTES('SHA1', 'p-#d556cmzZSEDgvg'), 'DaGet Client', 'http://localhost:1234', getdate(), 1, 1, 'Client permettant d''utiliser l''API DaGet')
go
/* API Daget (ressource server) */
insert into auth.RessourceServers(Login, ServerSecret, IsValid, Name, Description)
values ('_kZ2#412#Edcm-5f',  HASHBYTES('SHA1', 'og3Rkf--red###2'), true, 'API DaGet', 'Ressource server daget')
go

insert into auth.Scopes(wording, NiceWording) values ('account:read:write', 'plop')
insert into auth.Scopes(wording, NiceWording) values ('operation:read:write', 'plip')
go

insert into auth.Scopes(wording, NiceWording) values ('account:read', 'plap')
go

insert into auth.Scopes(wording, NiceWording) values ('daget:bankaccount:rw', 'Gestion des comptes (Lecture et écriture)')
insert into auth.Scopes(wording, NiceWording) values ('daget:operation:rw', 'Saisir des opérations (Lecture et écriture)')
go

insert into auth.ClientsScopes(FK_Client, FK_Scope) values (1, 1)
insert into auth.ClientsScopes(FK_Client, FK_Scope) values (1, 2)
insert into auth.ClientsScopes(FK_Client, FK_Scope) values (1, 3)
insert into auth.ClientsScopes(FK_Client, FK_Scope) values (1, 4)
insert into auth.ClientsScopes(FK_Client, FK_Scope) values (1, 5)
insert into auth.ClientsScopes(FK_Client, FK_Scope) values (2, 3)
insert into auth.ClientsScopes(FK_Client, FK_Scope) values (3, 4)
insert into auth.ClientsScopes(FK_Client, FK_Scope) values (3, 5)
go

select * from auth.Scopes
select * from auth.Clients
select * from auth.Codes
select * from auth.Users
select * from auth.UsersClients
select * from auth.ClientsScopes


--delete from auth.UsersClients

-- select HASHBYTES('SHA1', 'abc123456789')

-- 1K__-Az1mK15xxeyCzNxWDw3jU6QPMro2IBBUUF8YyAWSuvn2cAlYEmieGklaYxGjt89XqqhobiTawvmLQhSHN6WdamUIlc0Kr_cMRTIysQCH-lrLOKTbGPx87rCY6TCC3ymrBqN2aQZz53mm-WV2FZUBnTmWOWvAhxomMkK_qqfedxpDqHGHT2U-YUepGq0KKgDdusk4Jr0TwgEjXt8-IB1Nh5Yrtn1vb58Q04Y7rU