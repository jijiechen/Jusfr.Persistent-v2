IF DB_ID('TestDb') IS NULL
    CREATE DATABASE TestDb
GO

USE TestDB
GO

IF OBJECT_ID('Person') IS NOT NULL
    DROP TABLE Person

GO

CREATE TABLE Person
    (
      [Id] INT IDENTITY
               PRIMARY KEY ,
      [Name] VARCHAR(10) NOT NULL
                         UNIQUE ,
      [Birth] DATETIME NOT NULL
                       DEFAULT ( GETDATE() ) ,
      [Address] VARCHAR(255) NULL ,
      [JobId] INT NULL
    )
GO

INSERT  Person
        ( [Name], [Address], [JobId] )
VALUES  ( 'Rattz', 'Beijing', 2 ),
        ( 'Mike', 'Tokyo', 3 )
GO

IF OBJECT_ID('Job') IS NOT	NULL
    DROP TABLE Job
GO

CREATE TABLE Job
    (
      [Id] INT IDENTITY
               PRIMARY KEY ,
      [Title] VARCHAR(50) NOT NULL ,
      [Salary] DECIMAL(12, 2) NOT NULL
                              DEFAULT ( 0 )
    )
go

INSERT  Job
        ( [Title], [Salary] )
VALUES  ( 'C#', 5000 ),
        ( 'Java', 6000 ),
        ( 'Python', 5500 )
go