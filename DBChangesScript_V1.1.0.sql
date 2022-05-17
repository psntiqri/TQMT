
select [Id] as EmployeeId, [EnrollNo],[Username] as UserName,[Priv] as Privillage,
[Enable] as IsEnable,[MobileId] into EmployeeEnrollments from Employees

Go

ALTER TABLE EmployeeEnrollments
ADD PRIMARY KEY (EmployeeId)
Go

PRINT N'EmployeeEnrollments Created';
Go

DROP TABLE EmployeeDetails

Go

PRINT N'EmployeeDetails Table drop';
Go

PRINT N'Dropping FK_dbo.Attendances_dbo.Employees_EmployeeId...';


GO
ALTER TABLE [dbo].[Attendances] DROP CONSTRAINT [FK_dbo.Attendances_dbo.Employees_EmployeeId];


GO
PRINT N'Dropping FK_dbo.EmployeeOnSites_dbo.Employees_EmployeeId...';


GO
ALTER TABLE [dbo].[EmployeeOnSites] DROP CONSTRAINT [FK_dbo.EmployeeOnSites_dbo.Employees_EmployeeId];


GO
PRINT N'Dropping FK_dbo.TeamAllocations_dbo.Employees_Employee_Id...';


GO
ALTER TABLE [dbo].[TeamAllocations] DROP CONSTRAINT [FK_dbo.TeamAllocations_dbo.Employees_Employee_Id];


GO
PRINT N'Dropping FK_dbo.UserTeams_dbo.Employees_CreatedBy_Id...';


GO
ALTER TABLE [dbo].[UserTeams] DROP CONSTRAINT [FK_dbo.UserTeams_dbo.Employees_CreatedBy_Id];


GO
PRINT N'Dropping FK_dbo.VisitorAttendances_dbo.Employees_TransferredBy_Id...';


GO
ALTER TABLE [dbo].[VisitorAttendances] DROP CONSTRAINT [FK_dbo.VisitorAttendances_dbo.Employees_TransferredBy_Id];


GO
PRINT N'Dropping FK_dbo.VisitorPassAllocations_dbo.Employees_EmployeeId...';


GO
ALTER TABLE [dbo].[VisitorPassAllocations] DROP CONSTRAINT [FK_dbo.VisitorPassAllocations_dbo.Employees_EmployeeId];


GO
PRINT N'Altering [dbo].[Employees]...';


GO
ALTER TABLE [dbo].[Employees] DROP COLUMN [EnrollNo], COLUMN [Password], COLUMN [Priv];

GO
PRINT N'Creating FK_Attendances_EmployeeEnrollment...';


GO
ALTER TABLE [dbo].[Attendances] WITH NOCHECK
    ADD CONSTRAINT [FK_Attendances_EmployeeEnrollment] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId]);


GO
PRINT N'Creating FK_EmployeeOnSites_EmployeeEnrollment...';


GO
ALTER TABLE [dbo].[EmployeeOnSites] WITH NOCHECK
    ADD CONSTRAINT [FK_EmployeeOnSites_EmployeeEnrollment] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId]);


GO
PRINT N'Creating FK_TeamAllocations_EmployeeEnrollment...';


GO
ALTER TABLE [dbo].[TeamAllocations] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamAllocations_EmployeeEnrollment] FOREIGN KEY ([Employee_Id]) REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId]);


GO
PRINT N'Creating FK_UserTeams_EmployeeEnrollment...';


GO
ALTER TABLE [dbo].[UserTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_UserTeams_EmployeeEnrollment] FOREIGN KEY ([CreatedBy_Id]) REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId]);


GO
PRINT N'Creating FK_VisitorAttendances_EmployeeEnrollment...';


GO
ALTER TABLE [dbo].[VisitorAttendances] WITH NOCHECK
    ADD CONSTRAINT [FK_VisitorAttendances_EmployeeEnrollment] FOREIGN KEY ([TransferredBy_Id]) REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId]);


GO
PRINT N'Creating FK_VisitorPassAllocations_EmployeeEnrollment...';


GO
ALTER TABLE [dbo].[VisitorPassAllocations] WITH NOCHECK
    ADD CONSTRAINT [FK_VisitorPassAllocations_EmployeeEnrollment] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EmployeeEnrollments] ([EmployeeId]);


GO
PRINT N'Checking existing data against newly created constraints';

GO
ALTER TABLE [dbo].[Attendances] WITH CHECK CHECK CONSTRAINT [FK_Attendances_EmployeeEnrollment];

ALTER TABLE [dbo].[EmployeeOnSites] WITH CHECK CHECK CONSTRAINT [FK_EmployeeOnSites_EmployeeEnrollment];

ALTER TABLE [dbo].[TeamAllocations] WITH CHECK CHECK CONSTRAINT [FK_TeamAllocations_EmployeeEnrollment];

ALTER TABLE [dbo].[UserTeams] WITH CHECK CHECK CONSTRAINT [FK_UserTeams_EmployeeEnrollment];

ALTER TABLE [dbo].[VisitorAttendances] WITH CHECK CHECK CONSTRAINT [FK_VisitorAttendances_EmployeeEnrollment];

ALTER TABLE [dbo].[VisitorPassAllocations] WITH CHECK CHECK CONSTRAINT [FK_VisitorPassAllocations_EmployeeEnrollment];


GO
PRINT N'Update complete.';


--Change the column name of the UserTeams table
GO
 EXEC sp_RENAME  '[dbo].[UserTeams].CreatedBy_Id' , 'CreatedBy_EmployeeId', 'COLUMN'  


 --Change the column name TransferBy_Id to TransferBy_EmployeeId of table [VisitorAttendances]

 GO
 EXEC sp_RENAME  '[dbo].[VisitorAttendances].TransferredBy_Id' , 'TransferredBy_EmployeeId', 'COLUMN'  