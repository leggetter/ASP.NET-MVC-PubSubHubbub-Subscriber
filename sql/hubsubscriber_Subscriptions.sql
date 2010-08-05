USE [CloudWebsite]
GO

/****** Object:  Table [dbo].[hubsubscriber_Subscriptions]    Script Date: 08/05/2010 22:58:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[hubsubscriber_Subscriptions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Mode] [varchar](11) NOT NULL,
	[Verify] [varchar](5) NOT NULL,
	[Topic] [nvarchar](300) NOT NULL,
	[Callback] [nvarchar](300) NOT NULL,
	[Digest] [bit] NOT NULL,
	[Hub] [nvarchar](300) NULL,
	[Verified] [bit] NOT NULL,
	[LastUpdated] [smalldatetime] NULL,
	[PendingDeletion] [bit] NOT NULL,
	[PubSubHubUser] [varchar](100) NOT NULL,
 CONSTRAINT [PK_hubsubscriber_Subscriptions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[hubsubscriber_Subscriptions]  WITH CHECK ADD  CONSTRAINT [FK_hubsubscriber_Subscriptions_hubsubscriber_Users] FOREIGN KEY([PubSubHubUser])
REFERENCES [dbo].[hubsubscriber_Users] ([PubSubHubUser])
GO

ALTER TABLE [dbo].[hubsubscriber_Subscriptions] CHECK CONSTRAINT [FK_hubsubscriber_Subscriptions_hubsubscriber_Users]
GO

ALTER TABLE [dbo].[hubsubscriber_Subscriptions] ADD  CONSTRAINT [DF_hubsubscriber_Subscriptions_Verified]  DEFAULT ((0)) FOR [Verified]
GO

ALTER TABLE [dbo].[hubsubscriber_Subscriptions] ADD  CONSTRAINT [DF_hubsubscriber_Subscriptions_PendingDeletion]  DEFAULT ((0)) FOR [PendingDeletion]
GO


