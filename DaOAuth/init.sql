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

/* données d'initialisation */
insert into auth.ClientsTypes(wording) values ('public')
insert into auth.ClientsTypes(wording) values ('confidential')
go

/* données de test */
insert into auth.Clients (publicId, ClientSecret, Name, DefautRedirectUri, CreationDate, IsValid, FK_ClientType) 
values ('G7H8q4yBhpinNo6H', HASHBYTES('SHA1', 'abc123456789'), 'test', 'http://www.google.fr', getdate(), 1, 2)
go

select * from auth.Clients
select * from auth.Codes

select HASHBYTES('SHA1', 'abc123456789')