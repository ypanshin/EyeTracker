﻿-- =============================================
-- Script initilizate database with development data
-- =============================================

INSERT INTO [usr].[Users] ([UserTypeID], [Email], [Password], [PasswordSalt], [CreateDate], [Activated], [FirstName], [LastName])
VALUES (2/*Staff*/, 'dev@mobillify.com', 'XW0mA5DzfN4XL851H/i1xNFFbMOdtjVAL6fjBN5monE='/*111111*/, '/WCjbQ==', '20120101', 1, 'Development', 'Mobillify');

GO

INSERT INTO [dbo].[Portfolio] ([Description] ,[TimeZone] ,[CreateDate] ,[UserId])
VALUES ('Demo Portfolio', 0, '20120525', (SELECT ID FROM [usr].[Users] WHERE Email = 'dev@mobillify.com'));
GO

INSERT INTO [dbo].[Application] ([Description] ,[CreateDate] ,[Type] ,[PortfolioId])
VALUES ('Demo Application', '20120525', 3 /*3 stands for android, see EyeTracker.Domain.Model.ApplicationType*/, (SELECT ID FROM [dbo].[Portfolio] WHERE [Description] = 'Demo Portfolio'));

GO
INSERT INTO [dbo].[OperationSystem] ([Name])
VALUES ('2.3.3');
GO