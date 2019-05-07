CREATE TABLE [dbo].[AUTH_Authorization] (
    [AuthorizationId]       UNIQUEIDENTIFIER NOT NULL,
    [AuthorizationObjectId] NVARCHAR (50)    NOT NULL,
    [ValueList]             NVARCHAR (MAX)   NOT NULL,
    [Hash]                  NVARCHAR (44)    NOT NULL,
    CONSTRAINT [PK_AUTH_Authorization] PRIMARY KEY CLUSTERED ([AuthorizationId] ASC),
    CONSTRAINT [FK_AUTH_Authorization_AUTH_AuthorizationObject] FOREIGN KEY ([AuthorizationObjectId]) REFERENCES [dbo].[AUTH_AuthorizationObject] ([AuthorizationObjectId])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AUTH_Authorization]
    ON [dbo].[AUTH_Authorization]([Hash] ASC);

