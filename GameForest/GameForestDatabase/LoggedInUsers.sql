CREATE TABLE [dbo].[LoggedInUsers]
(
    [SessionId] NCHAR(10) NOT NULL , 
    [UserId] UNIQUEIDENTIFIER NOT NULL , 
    [LastHeartbeat] DATETIME NOT NULL, 
    PRIMARY KEY ([SessionId]), 
    CONSTRAINT [UserIdFromRegisteredUsers] FOREIGN KEY ([UserId]) REFERENCES [RegisteredUsers]([UserId])
)
