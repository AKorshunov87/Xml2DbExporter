USE [master]
CREATE DATABASE [testdb]; 
ALTER DATABASE [testdb] SET COMPATIBILITY_LEVEL = 90
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin 
	EXEC [testdb].[dbo].[sp_fulltext_database] @action = 'disable'
end;
ALTER DATABASE [testdb] SET ANSI_NULL_DEFAULT OFF 
ALTER DATABASE [testdb] SET ANSI_NULLS OFF 
ALTER DATABASE [testdb] SET ANSI_PADDING OFF 
ALTER DATABASE [testdb] SET ANSI_WARNINGS OFF 
ALTER DATABASE [testdb] SET ARITHABORT OFF 
ALTER DATABASE [testdb] SET AUTO_CLOSE ON 
ALTER DATABASE [testdb] SET AUTO_CREATE_STATISTICS ON 
ALTER DATABASE [testdb] SET AUTO_SHRINK OFF 
ALTER DATABASE [testdb] SET AUTO_UPDATE_STATISTICS ON 
ALTER DATABASE [testdb] SET CURSOR_CLOSE_ON_COMMIT OFF 
ALTER DATABASE [testdb] SET CURSOR_DEFAULT  GLOBAL 
ALTER DATABASE [testdb] SET CONCAT_NULL_YIELDS_NULL OFF 
ALTER DATABASE [testdb] SET NUMERIC_ROUNDABORT OFF 
ALTER DATABASE [testdb] SET QUOTED_IDENTIFIER OFF 
ALTER DATABASE [testdb] SET RECURSIVE_TRIGGERS OFF 
ALTER DATABASE [testdb] SET  DISABLE_BROKER 
ALTER DATABASE [testdb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
ALTER DATABASE [testdb] SET DATE_CORRELATION_OPTIMIZATION OFF 
ALTER DATABASE [testdb] SET TRUSTWORTHY OFF 
ALTER DATABASE [testdb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
ALTER DATABASE [testdb] SET PARAMETERIZATION SIMPLE 
ALTER DATABASE [testdb] SET READ_COMMITTED_SNAPSHOT OFF 
ALTER DATABASE [testdb] SET HONOR_BROKER_PRIORITY OFF 
ALTER DATABASE [testdb] SET  READ_WRITE 
ALTER DATABASE [testdb] SET RECOVERY FULL 
ALTER DATABASE [testdb] SET  MULTI_USER 
ALTER DATABASE [testdb] SET PAGE_VERIFY CHECKSUM  
ALTER DATABASE [testdb] SET DB_CHAINING OFF 
GO

USE [testdb]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON

CREATE TABLE [dbo].[Orders](
	[OrderID] [bigint] IDENTITY(0,1) NOT NULL,
	[CustomerID] [int] NOT NULL,
	[OrderDate] [date] NULL,
	[OrderValue] [varchar](34) NULL,
	[OrderStatus] [int] NOT NULL,
	[DateTimeAdded] [datetime] NULL,
	[DateTimeUpdated] [datetime] NULL,
	[OrderType] [int] NOT NULL,
 CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
SET ANSI_PADDING OFF
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ((0)) FOR [CustomerID]
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ((0)) FOR [OrderStatus]
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Order_DateTimeAdded]  DEFAULT (getdate()) FOR [DateTimeAdded]
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Order_DateTimeUpdated]  DEFAULT (getdate()) FOR [DateTimeUpdated]
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ((0)) FOR [OrderType]
GO


