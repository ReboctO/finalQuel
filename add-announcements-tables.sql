-- Create Announcements table
CREATE TABLE IF NOT EXISTS `Announcements` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Content` longtext CHARACTER SET utf8mb4 NOT NULL,
    `UrgencyLevel` int NOT NULL,
    `PublishedDate` datetime(6) NOT NULL,
    `ExpiryDate` datetime(6) NULL,
    `IsActive` tinyint(1) NOT NULL,
    `NotificationMethod` int NOT NULL,
    `EmailSent` tinyint(1) NOT NULL,
    `SmsSent` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedById` int NOT NULL,
    CONSTRAINT `PK_Announcements` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Announcements_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

-- Create AnnouncementRecipients table
CREATE TABLE IF NOT EXISTS `AnnouncementRecipients` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `AnnouncementId` int NOT NULL,
    `UserId` int NOT NULL,
    `IsRead` tinyint(1) NOT NULL,
    `ReadAt` datetime(6) NULL,
    CONSTRAINT `PK_AnnouncementRecipients` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AnnouncementRecipients_Announcements_AnnouncementId` FOREIGN KEY (`AnnouncementId`) REFERENCES `Announcements` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AnnouncementRecipients_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

-- Create indices
CREATE INDEX `IX_Announcements_CreatedById` ON `Announcements` (`CreatedById`);
CREATE INDEX `IX_AnnouncementRecipients_AnnouncementId` ON `AnnouncementRecipients` (`AnnouncementId`);
CREATE INDEX `IX_AnnouncementRecipients_UserId` ON `AnnouncementRecipients` (`UserId`); 