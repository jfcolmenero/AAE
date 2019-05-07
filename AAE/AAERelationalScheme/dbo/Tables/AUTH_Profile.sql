CREATE TABLE [dbo].[AUTH_Profile] (
    [ProfileId]   NVARCHAR (100) NOT NULL,
    [Description] TEXT           NOT NULL,
    CONSTRAINT [PK_AUTH_Profile] PRIMARY KEY CLUSTERED ([ProfileId] ASC)
);

