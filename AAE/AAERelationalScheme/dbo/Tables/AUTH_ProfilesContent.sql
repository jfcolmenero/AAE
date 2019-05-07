CREATE TABLE [dbo].[AUTH_ProfilesContent] (
    [ProfileId]       NVARCHAR (100)   NOT NULL,
    [AuthorizationId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_AUTH_ProfilesContent] PRIMARY KEY CLUSTERED ([ProfileId] ASC, [AuthorizationId] ASC),
    CONSTRAINT [FK_AUTH_ProfilesContent_AUTH_Authorization] FOREIGN KEY ([AuthorizationId]) REFERENCES [dbo].[AUTH_Authorization] ([AuthorizationId]),
    CONSTRAINT [FK_AUTH_ProfilesContent_AUTH_Profile] FOREIGN KEY ([ProfileId]) REFERENCES [dbo].[AUTH_Profile] ([ProfileId])
);

