USE [TimeObj]
GO
/****** Object:  Table [dbo].[Attendances]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Attendances](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[CardNo] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Month] [int] NOT NULL,
	[Day] [int] NOT NULL,
	[Hour] [int] NOT NULL,
	[Minute] [int] NOT NULL,
	[Second] [int] NOT NULL,
	[VerifyMode] [int] NOT NULL,
	[InOutMode] [nvarchar](max) NULL,
	[WorkCode] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Attendances] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CardAccessLevels]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CardAccessLevels](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.CardAccessLevels] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CardCategories]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CardCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.CardCategories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Cards]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cards](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CardCategoryId] [int] NULL,
	[CardAccessLevelId] [int] NULL,
 CONSTRAINT [PK_dbo.Cards] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EmployeeEnrollments]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeEnrollments](
	[EmployeeId] [int] NOT NULL,
	[CardNo] [int] NOT NULL,
	[UserName] [nvarchar](10) NULL,
	[Privillage] [int] NOT NULL,
	[IsEnable] [bit] NOT NULL,
	[MobileId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK__Employee__7AD04F11789EE131] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EmployeeExternals]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeExternals](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CardNo] [int] NOT NULL,
 CONSTRAINT [PK_dbo.EmployeeExternals] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EmployeeOnSites]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeOnSites](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[FromDate] [datetime] NOT NULL,
	[ToDate] [datetime] NOT NULL,
	[LocationId] [int] NULL,
	[MobileNumber] [nvarchar](max) NULL,
	[IsPermanant] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.EmployeeOnSites] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Locations]    Script Date: 1/25/2018 1:34:25 PM ******/
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
/****** Object:  Table [dbo].[Privileges]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Privileges](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Privileges] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SpecialEvents]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SpecialEvents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventName] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[EventFromDate] [datetime] NULL,
	[EventToDate] [datetime] NULL,
 CONSTRAINT [PK_dbo.SpecialEvents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TimeRecords]    Script Date: 1/25/2018 1:34:25 PM ******/
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
/****** Object:  Table [dbo].[UserTeams]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserTeams](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeamName] [nvarchar](max) NULL,
	[TeamMembersIdString] [nvarchar](max) NULL,
	[DateCreated] [datetime] NOT NULL,
	[CreatedBy_EmployeeId] [int] NULL,
	[TeamSharedEmpIdString] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.UserTeams] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VisitInformations]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VisitInformations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[VisitPurpose] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NULL,
	[FomDate] [datetime] NULL,
	[ToDate] [datetime] NULL,
	[CardAccessLevelId] [int] NULL,
	[EmployeeId] [int] NULL,
	[EnteredBy] [int] NULL,
	[EntityId] [int] NOT NULL,
	[EntityType] [int] NOT NULL,
	[AppointmentTime] [datetime] NULL,
 CONSTRAINT [PK_dbo.VisitInformations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VisitorAttendances]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VisitorAttendances](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CardNo] [int] NOT NULL,
	[DateTime] [datetime] NOT NULL,
	[VerifyMode] [int] NOT NULL,
	[InOutMode] [nvarchar](max) NULL,
	[WorkCode] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[isTransferred] [bit] NOT NULL,
	[TransferredDate] [datetime] NULL,
	[Note] [nvarchar](max) NULL,
	[TransferredBy_EmployeeId] [int] NULL,
	[VisitorPassAllocationId] [int] NULL,
	[VisitInformationId] [int] NULL,
	[EmployeeId] [int] NULL,
 CONSTRAINT [PK_dbo.VisitorAttendances] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VisitorPassAllocations]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VisitorPassAllocations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[CardNo] [int] NOT NULL,
	[AssignDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[IsCardReturned] [bit] NOT NULL,
	[CardIssuedBy] [int] NULL,
	[VisitInformationId] [int] NULL,
	[DeallocateDate] [datetime] NULL,
 CONSTRAINT [PK_dbo.VisitorPassAllocations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Visitors]    Script Date: 1/25/2018 1:34:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Visitors](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[MobileNo] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[IdentificationType] [nvarchar](max) NULL,
	[IdentificationNo] [nvarchar](max) NULL,
	[EnteredBy] [int] NULL,
	[Company] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Visitors] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[CardAccessLevels] ON 

GO
INSERT [dbo].[CardAccessLevels] ([Id], [Description]) VALUES (1, N'All Floors')
GO
INSERT [dbo].[CardAccessLevels] ([Id], [Description]) VALUES (2, N'4th Floor only')
GO
INSERT [dbo].[CardAccessLevels] ([Id], [Description]) VALUES (3, N'3rd Floor only')
GO
INSERT [dbo].[CardAccessLevels] ([Id], [Description]) VALUES (4, N'2nd Floor only')
GO
SET IDENTITY_INSERT [dbo].[CardAccessLevels] OFF
GO
SET IDENTITY_INSERT [dbo].[CardCategories] ON 

GO
INSERT [dbo].[CardCategories] ([Id], [Description]) VALUES (1, N'Employee')
GO
SET IDENTITY_INSERT [dbo].[CardCategories] OFF
GO
SET IDENTITY_INSERT [dbo].[Cards] ON 

GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1069, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1070, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1088, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1089, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1093, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1094, 1, 2)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1095, 1, 3)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1096, 1, 4)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1097, 1, 2)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1098, 1, 2)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1100, 1, 3)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1101, 1, 3)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1108, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1164, 1, NULL)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1165, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1166, 1, NULL)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1167, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1168, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1169, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1170, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1176, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1177, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1178, 1, NULL)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1179, 1, NULL)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1180, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1181, 1, 1)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1182, 1, NULL)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1183, 1, NULL)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1184, 1, NULL)
GO
INSERT [dbo].[Cards] ([Id], [CardCategoryId], [CardAccessLevelId]) VALUES (1185, 1, 1)
GO
SET IDENTITY_INSERT [dbo].[Cards] OFF
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
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (13, 4, N'1st Floor', N'192.168.20.203', N'AC100', 4, 4370, 4, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (14, 0, N'Tiqri-Australia', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (15, 0, N'Tiqri-Norway', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (16, 0, N'Tiqri-Sweden', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (17, 0, N'Tiqri-Singapore', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (18, 1, N'Server Room', N'192.168.20.200', N'AC100', 1, 4370, 1, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (19, 0, N'Tiqri-Sri Lanka', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (20, 0, N'USA', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (21, 0, N'UK', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (22, 0, N'Ireland', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (23, 0, N'Vietnam', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (24, 0, N'India', NULL, NULL, 0, 0, 0, 0, 1)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (25, 8, N'City Bank Main Entrance', N'192.168.20.221', N'AC100', 8, 4370, 8, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (26, 9, N'City Bank Exit', N'192.168.20.222', N'AC100', 9, 4370, 9, 1, 0)
GO
INSERT [dbo].[Locations] ([Id], [DeviceNo], [Floor], [DeviceIP], [DeviceName], [DeviceNumber], [Port], [CommKey], [ClearLogsAutomatically], [OnSiteLocation]) VALUES (27, 0, N'Home', NULL, NULL, 0, 0, 0, 0, 1)
GO
SET IDENTITY_INSERT [dbo].[Locations] OFF
GO
INSERT [dbo].[Privileges] ([Id], [Name]) VALUES (-1, N'System Admin')
GO
INSERT [dbo].[Privileges] ([Id], [Name]) VALUES (0, N'Employee')
GO
INSERT [dbo].[Privileges] ([Id], [Name]) VALUES (1, N'Manager')
GO
INSERT [dbo].[Privileges] ([Id], [Name]) VALUES (2, N'Top Manager')
GO
INSERT [dbo].[Privileges] ([Id], [Name]) VALUES (3, N'Admin')
GO
ALTER TABLE [dbo].[EmployeeOnSites] ADD  DEFAULT ((0)) FOR [IsPermanant]
GO
ALTER TABLE [dbo].[VisitorPassAllocations] ADD  DEFAULT ((0)) FOR [IsCardReturned]
GO
ALTER TABLE [dbo].[Attendances]  WITH CHECK ADD  CONSTRAINT [FK_Attendances_EmployeeEnrollment] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId])
GO
ALTER TABLE [dbo].[Attendances] CHECK CONSTRAINT [FK_Attendances_EmployeeEnrollment]
GO
ALTER TABLE [dbo].[Attendances]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Attendances_dbo.Locations_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Locations] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Attendances] CHECK CONSTRAINT [FK_dbo.Attendances_dbo.Locations_LocationId]
GO
ALTER TABLE [dbo].[Cards]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Cards_dbo.CardAccessLevels_CardAccessLevelId] FOREIGN KEY([CardAccessLevelId])
REFERENCES [dbo].[CardAccessLevels] ([Id])
GO
ALTER TABLE [dbo].[Cards] CHECK CONSTRAINT [FK_dbo.Cards_dbo.CardAccessLevels_CardAccessLevelId]
GO
ALTER TABLE [dbo].[Cards]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Cards_dbo.CardCategories_CardCategoryId] FOREIGN KEY([CardCategoryId])
REFERENCES [dbo].[CardCategories] ([Id])
GO
ALTER TABLE [dbo].[Cards] CHECK CONSTRAINT [FK_dbo.Cards_dbo.CardCategories_CardCategoryId]
GO
ALTER TABLE [dbo].[EmployeeOnSites]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EmployeeOnSites_dbo.Locations_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Locations] ([Id])
GO
ALTER TABLE [dbo].[EmployeeOnSites] CHECK CONSTRAINT [FK_dbo.EmployeeOnSites_dbo.Locations_LocationId]
GO
ALTER TABLE [dbo].[EmployeeOnSites]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeOnSites_EmployeeEnrollment] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId])
GO
ALTER TABLE [dbo].[EmployeeOnSites] CHECK CONSTRAINT [FK_EmployeeOnSites_EmployeeEnrollment]
GO
ALTER TABLE [dbo].[UserTeams]  WITH CHECK ADD  CONSTRAINT [FK_UserTeams_EmployeeEnrollment] FOREIGN KEY([CreatedBy_EmployeeId])
REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId])
GO
ALTER TABLE [dbo].[UserTeams] CHECK CONSTRAINT [FK_UserTeams_EmployeeEnrollment]
GO
ALTER TABLE [dbo].[VisitInformations]  WITH CHECK ADD  CONSTRAINT [FK_dbo.VisitInformations_dbo.CardAccessLevels_CardAccessLevelId] FOREIGN KEY([CardAccessLevelId])
REFERENCES [dbo].[CardAccessLevels] ([Id])
GO
ALTER TABLE [dbo].[VisitInformations] CHECK CONSTRAINT [FK_dbo.VisitInformations_dbo.CardAccessLevels_CardAccessLevelId]
GO
ALTER TABLE [dbo].[VisitInformations]  WITH CHECK ADD  CONSTRAINT [FK_dbo.VisitInformations_dbo.Visitors_EntityId] FOREIGN KEY([EntityId])
REFERENCES [dbo].[Visitors] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[VisitInformations] CHECK CONSTRAINT [FK_dbo.VisitInformations_dbo.Visitors_EntityId]
GO
ALTER TABLE [dbo].[VisitorAttendances]  WITH CHECK ADD  CONSTRAINT [FK_dbo.VisitorAttendances_dbo.EmployeeEnrollments_EmployeeId] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId])
GO
ALTER TABLE [dbo].[VisitorAttendances] CHECK CONSTRAINT [FK_dbo.VisitorAttendances_dbo.EmployeeEnrollments_EmployeeId]
GO
ALTER TABLE [dbo].[VisitorAttendances]  WITH CHECK ADD  CONSTRAINT [FK_dbo.VisitorAttendances_dbo.Locations_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Locations] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[VisitorAttendances] CHECK CONSTRAINT [FK_dbo.VisitorAttendances_dbo.Locations_LocationId]
GO
ALTER TABLE [dbo].[VisitorAttendances]  WITH CHECK ADD  CONSTRAINT [FK_dbo.VisitorAttendances_dbo.VisitInformations_VisitInformationId] FOREIGN KEY([VisitInformationId])
REFERENCES [dbo].[VisitInformations] ([Id])
GO
ALTER TABLE [dbo].[VisitorAttendances] CHECK CONSTRAINT [FK_dbo.VisitorAttendances_dbo.VisitInformations_VisitInformationId]
GO
ALTER TABLE [dbo].[VisitorAttendances]  WITH CHECK ADD  CONSTRAINT [FK_VisitorAttendances_EmployeeEnrollment] FOREIGN KEY([TransferredBy_EmployeeId])
REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId])
GO
ALTER TABLE [dbo].[VisitorAttendances] CHECK CONSTRAINT [FK_VisitorAttendances_EmployeeEnrollment]
GO
ALTER TABLE [dbo].[VisitorPassAllocations]  WITH CHECK ADD  CONSTRAINT [FK_dbo.VisitorPassAllocations_dbo.EmployeeEnrollments_EmployeeId] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId])
GO
ALTER TABLE [dbo].[VisitorPassAllocations] CHECK CONSTRAINT [FK_dbo.VisitorPassAllocations_dbo.EmployeeEnrollments_EmployeeId]
GO
ALTER TABLE [dbo].[VisitorPassAllocations]  WITH CHECK ADD  CONSTRAINT [FK_dbo.VisitorPassAllocations_dbo.VisitInformations_VisitInformationId] FOREIGN KEY([VisitInformationId])
REFERENCES [dbo].[VisitInformations] ([Id])
GO
ALTER TABLE [dbo].[VisitorPassAllocations] CHECK CONSTRAINT [FK_dbo.VisitorPassAllocations_dbo.VisitInformations_VisitInformationId]
GO
ALTER TABLE [dbo].[VisitorPassAllocations]  WITH CHECK ADD  CONSTRAINT [FK_VisitorPassAllocations_EmployeeEnrollment] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId])
GO
ALTER TABLE [dbo].[VisitorPassAllocations] CHECK CONSTRAINT [FK_VisitorPassAllocations_EmployeeEnrollment]
GO
