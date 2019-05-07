CREATE TABLE [dbo].[AUTH_AuthorizationObject] (
    [AuthorizationObjectId]      NVARCHAR (50)  NOT NULL,
    [AuthorizationObjectGroupId] NVARCHAR (50)  NOT NULL,
    [FieldList]                  NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_AUTH_AuthorizationObject] PRIMARY KEY CLUSTERED ([AuthorizationObjectId] ASC),
    CONSTRAINT [FK_AUTH_AuthorizationObject_AUTH_AuthorizationGroup] FOREIGN KEY ([AuthorizationObjectGroupId]) REFERENCES [dbo].[AUTH_AuthorizationGroup] ([AuthorizationObjectGroupId])
);

