CREATE TABLE [dbo].[AUTH_UsersMasterRecord] (
    [HashCode] NVARCHAR (44) NOT NULL,
    [UserId]   NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_AUTH_UsersMasterRecord] PRIMARY KEY CLUSTERED ([HashCode] ASC)
);


GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190321-184819]
    ON [dbo].[AUTH_UsersMasterRecord]([UserId] ASC);

