-- Create Payments table
CREATE TABLE IF NOT EXISTS `Payments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PropertyId` int NOT NULL,
    `UserId` int NOT NULL,
    `Amount` decimal(18,2) NOT NULL,
    `DueDate` datetime(6) NOT NULL,
    `PaymentDate` datetime(6) NULL,
    `ReferenceNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Status` int NOT NULL DEFAULT 0,
    `Type` int NOT NULL,
    `Notes` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `NotificationSent` tinyint(1) NOT NULL DEFAULT 0,
    `NotificationSentAt` datetime(6) NULL,
    `LastReminderSent` datetime(6) NULL,
    `ReminderCount` int NOT NULL DEFAULT 0,
    CONSTRAINT `PK_Payments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Payments_Properties_PropertyId` FOREIGN KEY (`PropertyId`) REFERENCES `Properties` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Payments_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- Create PaymentHistory table for audit trail
CREATE TABLE IF NOT EXISTS `PaymentHistory` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PaymentId` int NOT NULL,
    `UserId` int NOT NULL,
    `Action` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `PreviousStatus` int NULL,
    `NewStatus` int NULL,
    `PreviousAmount` decimal(18,2) NULL,
    `NewAmount` decimal(18,2) NULL,
    `Notes` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_PaymentHistory` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PaymentHistory_Payments_PaymentId` FOREIGN KEY (`PaymentId`) REFERENCES `Payments` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PaymentHistory_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`)
) CHARACTER SET=utf8mb4;

-- Create BillingSettings table for configurable billing parameters
CREATE TABLE IF NOT EXISTS `BillingSettings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Key` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(255) CHARACTER SET utf8mb4 NULL,
    `Type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_BillingSettings` PRIMARY KEY (`Id`),
    CONSTRAINT `UK_BillingSettings_Key` UNIQUE (`Key`)
) CHARACTER SET=utf8mb4;

-- Create indices
CREATE INDEX `IX_Payments_PropertyId` ON `Payments` (`PropertyId`);
CREATE INDEX `IX_Payments_UserId` ON `Payments` (`UserId`);
CREATE INDEX `IX_Payments_Status` ON `Payments` (`Status`);
CREATE INDEX `IX_Payments_DueDate` ON `Payments` (`DueDate`);
CREATE INDEX `IX_Payments_Type` ON `Payments` (`Type`);
CREATE INDEX `IX_PaymentHistory_PaymentId` ON `PaymentHistory` (`PaymentId`);
CREATE INDEX `IX_PaymentHistory_UserId` ON `PaymentHistory` (`UserId`);
CREATE INDEX `IX_PaymentHistory_CreatedAt` ON `PaymentHistory` (`CreatedAt`);

-- Insert default billing settings
INSERT INTO `BillingSettings` 
(`Key`, `Value`, `Description`, `Type`, `CreatedAt`) 
VALUES 
('LatePaymentFeePercentage', '5', 'Late payment fee percentage', 'Decimal', NOW()),
('GracePeriodDays', '5', 'Number of days after due date before late fee applies', 'Integer', NOW()),
('ReminderDaysBeforeDue', '7,3,1', 'Days before due date to send reminders (comma-separated)', 'String', NOW()),
('MaxReminderCount', '3', 'Maximum number of reminders to send per bill', 'Integer', NOW()),
('NotificationMethods', 'Email,SMS', 'Available notification methods (comma-separated)', 'String', NOW()),
('DefaultDueDateDays', '30', 'Default number of days for payment due date from bill creation', 'Integer', NOW()); 