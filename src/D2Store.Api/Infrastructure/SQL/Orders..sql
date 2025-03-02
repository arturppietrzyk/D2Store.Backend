USE [D2Store]
GO

/****** Object:  Table [dbo].[Orders]    Script Date: 02/03/2025 14:00:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Orders](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[OrderDate] [datetime] NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Orders] ADD  DEFAULT (getutcdate()) FOR [OrderDate]
GO

ALTER TABLE [dbo].[Orders]  WITH CHECK ADD CHECK  (([Status]='Cancelled' OR [Status]='Paid' OR [Status]='Pending'))
GO

ALTER TABLE [dbo].[Orders]  WITH CHECK ADD CHECK  (([TotalAmount]>(0)))
GO


