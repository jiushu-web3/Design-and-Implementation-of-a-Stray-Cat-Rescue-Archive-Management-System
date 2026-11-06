USE StrayCatArchive;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SysUser WHERE UserName = N'admin')
BEGIN
    INSERT INTO dbo.SysUser (UserName, PasswordHash, Salt, RealName, Role, IsEnabled)
    VALUES (
        N'admin',
        N'9ddc3e471e45a5eab820bdecae8f62255d38a83619b4ce81caa8456909af3fa1',
        N'0123456789ABCDEF0123456789ABCDEF',
        N'系统管理员',
        1,
        1);
END
GO
