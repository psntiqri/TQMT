USE [MyTimeDev]
GO

UPDATE [dbo].[Cards]
   SET [CardAccessLevelId] = NULL 

--update [CardAccessLevelId] = 1 for "All Floors"
UPDATE [dbo].[Cards]
   SET [CardAccessLevelId] = 1
 WHERE Id in (1108,1165,1167,1168,1169,1170,1176,1177,1180,1181,1185)

 --update [CardAccessLevelId] = 2 for "4th Floor only"
 UPDATE [dbo].[Cards]
   SET [CardAccessLevelId] = 2
 WHERE Id in (1094,1097,1098)

  --update [CardAccessLevelId] = 3 for "3rd Floor only"
 UPDATE [dbo].[Cards]
   SET [CardAccessLevelId] = 3
 WHERE Id in (1095,1100,1101)

  --update [CardAccessLevelId] = 4 for "2nd Floor only"
 UPDATE [dbo].[Cards]
   SET [CardAccessLevelId] = 4
 WHERE Id in (1096)

GO


