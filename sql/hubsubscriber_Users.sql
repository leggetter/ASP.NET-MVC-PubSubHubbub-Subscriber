USE [CloudWebsite]
GO

/****** Object:  Table [dbo].[hubsubscriber_Users]    Script Date: 08/05/2010 22:56:38 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hubsubscriber_Users]') AND type in (N'U'))
DROP TABLE [dbo].[hubsubscriber_Users]
GO

USE [CloudWebsite]
GO

/****** Object:  Table [dbo].[hubsubscriber_Users]    Script Date: 08/05/2010 22:56:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[hubsubscriber_Users](
	[PubSubHubUser] [varchar](100) NOT NULL,
	[KwwikaTopic] [varchar](300) NOT NULL,
	[MaxHubSubscriptions] [smallint] NOT NULL,
 CONSTRAINT [PK_hubsubscriber_Users] PRIMARY KEY CLUSTERED 
(
	[PubSubHubUser] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


