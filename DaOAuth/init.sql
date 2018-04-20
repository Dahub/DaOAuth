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

create table auth.Clients
(
	Id integer not null primary key identity(1,1),
	PublicId nvarchar(256) not null unique,
	Name nvarchar(256) not null,
	DefautRedirectUri nvarchar(max) null,
	CreationDate datetime not null default(getdate()),
	IsValid bit not null default(1)
) 
go

create table auth.Codes
(
	Id integer not null primary key identity(1,1),
	CodeValue nvarchar(256) not null,

)
go