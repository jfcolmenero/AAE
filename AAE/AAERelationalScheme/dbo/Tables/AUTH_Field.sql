CREATE TABLE [dbo].[AUTH_Field] (
    [FieldId]     NVARCHAR (50)  NOT NULL,
    [FieldValues] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_AUTH_Field] PRIMARY KEY CLUSTERED ([FieldId] ASC)
);

