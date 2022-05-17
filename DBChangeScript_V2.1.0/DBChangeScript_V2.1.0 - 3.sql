begin
update [TM].[dbo].[VisitorPassAllocations]	set EntityId=EmployeeId,  EntityType=1;
drop TABLE [TM].[dbo].[EmployeeDatas];
end