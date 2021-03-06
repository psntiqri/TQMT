USE [LocalTime]
GO
/****** Object:  Table [dbo].[Locations]    Script Date: 1/25/2018 1:07:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Locations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DeviceNo] [int] NOT NULL,
	[Floor] [nvarchar](max) NULL,
	[DeviceIP] [nvarchar](max) NULL,
	[DeviceName] [nvarchar](max) NULL,
	[DeviceNumber] [int] NOT NULL DEFAULT ((0)),
	[Port] [int] NOT NULL DEFAULT ((0)),
	[CommKey] [int] NOT NULL DEFAULT ((0)),
	[ClearLogsAutomatically] [bit] NOT NULL DEFAULT ((0)),
	[OnSiteLocation] [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_dbo.Locations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SyncMachineLogs]    Script Date: 1/25/2018 1:07:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SyncMachineLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SyncSessionLogId] [int] NOT NULL,
	[StartAt] [datetime] NOT NULL,
	[CompletedAt] [datetime] NOT NULL,
	[Status] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[LocationID] [int] NOT NULL,
	[NumberOfEmployeeRecords] [int] NOT NULL,
	[NumberOfVisitorRecords] [int] NOT NULL,
	[NuberOfExternalRecords] [int] NOT NULL,
	[IsSynced] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SyncMachineLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SyncServiceStatusInfoes]    Script Date: 1/25/2018 1:07:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SyncServiceStatusInfoes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ServiceStatus] [nvarchar](max) NULL,
	[SynchronizationStatus] [nvarchar](max) NULL,
	[LastServiceStartedAt] [datetime] NULL,
	[LastServiceStoppedAt] [datetime] NULL,
	[NextServiceScheduledAt] [datetime] NULL,
	[CurrentSynchronizingMessage] [nvarchar](max) NULL,
	[CurrentSynchronizingLocationID] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SyncServiceStatusInfoes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SyncSessionLogs]    Script Date: 1/25/2018 1:07:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SyncSessionLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StartAt] [datetime] NOT NULL,
	[CompletedAt] [datetime] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NULL,
	[NumberOfEmployeeRecords] [int] NOT NULL,
	[NumberOfVisitorRecords] [int] NOT NULL,
	[NuberOfExternalRecords] [int] NOT NULL,
	[IsSynced] [int] NOT NULL,
 CONSTRAINT [PK_dbo.SyncSessionLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TimeRecords]    Script Date: 1/25/2018 1:07:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TimeRecords](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CardNo] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Month] [int] NOT NULL,
	[Day] [int] NOT NULL,
	[Hour] [int] NOT NULL,
	[Minute] [int] NOT NULL,
	[Second] [int] NOT NULL,
	[VerifyMode] [int] NOT NULL,
	[InOutMode] [int] NOT NULL,
	[WorkCode] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[IsSynced] [int] NOT NULL,
 CONSTRAINT [PK_dbo.TimeRecords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[Locations] ON 

GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (8, 2, N'Basement', N'192.168.20.201', N'AC100', 2, 4370, 2, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (9, 3, N'Main Entrance', N'192.168.20.202', N'AC100', 3, 4370, 3, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (10, 5, N'2nd Floor', N'192.168.20.204', N'AC100', 5, 4370, 5, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (11, 6, N'3rd Floor', N'192.168.20.205', N'AC100', 6, 4370, 6, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (12, 7, N'4th Floor', N'192.168.20.206', N'AC100', 7, 4370, 7, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (13, 4, N'1st Floor', N'192.168.20.203', N'AC100', 4, 4370, 4, 1, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (18, 1, N'Server Room', N'192.168.20.200', N'AC100', 1, 4370, 1, 1, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (25, 8, N'City Bank Main Entrance', N'192.168.20.221', N'AC100', 8, 4370, 8, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (26, 9, N'City Bank Exit', N'192.168.20.222', N'AC100', 9, 4370, 9, 1, 0)
GO
SET IDENTITY_INSERT [dbo].[Locations] OFF
GO
ALTER TABLE [dbo].[SyncMachineLogs] ADD  DEFAULT ((0)) FOR [IsSynced]
GO
ALTER TABLE [dbo].[SyncSessionLogs] ADD  DEFAULT ((0)) FOR [IsSynced]
GO
ALTER TABLE [dbo].[SyncMachineLogs]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SyncMachineLogs_dbo.SyncSessionLogs_SyncSessionLogId] FOREIGN KEY([SyncSessionLogId])
REFERENCES [dbo].[SyncSessionLogs] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SyncMachineLogs] CHECK CONSTRAINT [FK_dbo.SyncMachineLogs_dbo.SyncSessionLogs_SyncSessionLogId]
GO
