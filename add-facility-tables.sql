-- Create Facilities table
CREATE TABLE IF NOT EXISTS `Facilities` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Type` int NOT NULL,
    `Capacity` int NOT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `Location` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ImageUrl` varchar(255) CHARACTER SET utf8mb4 NULL,
    `HourlyRate` decimal(18,2) NULL,
    `MinimumReservationTime` time(6) NULL,
    `MaximumReservationTime` time(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `OpeningTime` time(6) NOT NULL,
    `ClosingTime` time(6) NOT NULL,
    `MaxDaysInAdvance` int NOT NULL DEFAULT 30,
    `MaxReservationsPerUser` int NOT NULL DEFAULT 2,
    `RequiresAdminApproval` tinyint(1) NOT NULL DEFAULT 1,
    CONSTRAINT `PK_Facilities` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

-- Create FacilityReservations table
CREATE TABLE IF NOT EXISTS `FacilityReservations` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FacilityId` int NOT NULL,
    `UserId` int NOT NULL,
    `ReservationDate` datetime(6) NOT NULL,
    `StartTime` time(6) NOT NULL,
    `EndTime` time(6) NOT NULL,
    `Purpose` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `ExpectedAttendees` int NULL,
    `Notes` longtext CHARACTER SET utf8mb4 NULL,
    `Status` int NOT NULL DEFAULT 0,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `ApprovedAt` datetime(6) NULL,
    `RejectedAt` datetime(6) NULL,
    `AdminRemarks` longtext CHARACTER SET utf8mb4 NULL,
    `ReviewedByUserId` int NULL,
    `NotificationSent` tinyint(1) NOT NULL DEFAULT 0,
    `NotificationSentAt` datetime(6) NULL,
    CONSTRAINT `PK_FacilityReservations` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_FacilityReservations_Facilities_FacilityId` FOREIGN KEY (`FacilityId`) REFERENCES `Facilities` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_FacilityReservations_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_FacilityReservations_Users_ReviewedByUserId` FOREIGN KEY (`ReviewedByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

-- Create indices
CREATE INDEX `IX_Facilities_Name` ON `Facilities` (`Name`);
CREATE INDEX `IX_FacilityReservations_FacilityId` ON `FacilityReservations` (`FacilityId`);
CREATE INDEX `IX_FacilityReservations_UserId` ON `FacilityReservations` (`UserId`);
CREATE INDEX `IX_FacilityReservations_ReviewedByUserId` ON `FacilityReservations` (`ReviewedByUserId`);
CREATE INDEX `IX_FacilityReservations_ReservationDate` ON `FacilityReservations` (`ReservationDate`);
CREATE INDEX `IX_FacilityReservations_Status` ON `FacilityReservations` (`Status`);

-- Insert sample facility data
INSERT INTO `Facilities` 
(`Name`, `Description`, `Type`, `Capacity`, `Location`, `ImageUrl`, `OpeningTime`, `ClosingTime`, `CreatedAt`) 
VALUES 
('Function Hall', 'Large hall suitable for parties and gatherings', 0, 100, 'Building A, Ground Floor', '/images/facilities/function-hall.jpg', '08:00:00', '22:00:00', NOW()),
('Basketball Court', 'Full-size basketball court with seating', 1, 30, 'Sports Complex, East Wing', '/images/facilities/basketball-court.jpg', '06:00:00', '21:00:00', NOW()),
('Swimming Pool', 'Olympic-size swimming pool with lanes', 2, 50, 'Sports Complex, West Wing', '/images/facilities/swimming-pool.jpg', '06:00:00', '20:00:00', NOW()),
('Gym', 'Fully equipped gym with cardio and weight machines', 3, 25, 'Building B, 2nd Floor', '/images/facilities/gym.jpg', '05:00:00', '23:00:00', NOW()),
('Tennis Court', 'Professional tennis court with lighting', 4, 4, 'Sports Complex, South Area', '/images/facilities/tennis-court.jpg', '07:00:00', '21:00:00', NOW()),
('Meeting Room', 'Conference room with presentation equipment', 5, 20, 'Building A, 3rd Floor', '/images/facilities/meeting-room.jpg', '08:00:00', '20:00:00', NOW()),
('BBQ Area', 'Outdoor BBQ area with tables and grills', 6, 30, 'Garden Area, North Side', '/images/facilities/bbq-area.jpg', '10:00:00', '22:00:00', NOW()); 