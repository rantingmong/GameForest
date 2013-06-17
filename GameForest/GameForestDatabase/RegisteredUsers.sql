CREATE TABLE [dbo].[RegisteredUsers]
(
    [UserId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [UserName] VARBINARY(50) NOT NULL, 
    [Password] VARCHAR(50) NOT NULL, 
    [FirstName] VARCHAR(50) NULL, 
    [LastName] VARCHAR(50) NULL, 
    [Description] VARCHAR(MAX) NULL
)
