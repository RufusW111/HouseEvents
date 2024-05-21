--Create tables

DROP TABLE dbo.House
GO

CREATE TABLE [dbo].[House](
	[HouseID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[HouseName] [varchar](50) NOT NULL,
	[UndermasterFirstName] [varchar](50) NOT NULL,
	[EventsCoordinator] [varchar](50) NULL )
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_House] ON [dbo].[House]
(
	[HouseName] ASC
)