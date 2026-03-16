-- Clear all data and reset IDENTITY
USE HostelManagementDB;

DELETE FROM RoomUpdateLog;
DELETE FROM ViolationReport;
DELETE FROM Review;
DELETE FROM Favorite;
DELETE FROM Notification;
DELETE FROM PaymentTransaction;
DELETE FROM BookingRequest;
DELETE FROM Room;
DELETE FROM Hostel;
DELETE FROM Tenant;
DELETE FROM HostelOwner;
DELETE FROM Admin;
DELETE FROM Account;

-- Reset IDENTITY
DBCC CHECKIDENT ('Account', RESEED, 0);
DBCC CHECKIDENT ('Tenant', RESEED, 0);
DBCC CHECKIDENT ('HostelOwner', RESEED, 0);
DBCC CHECKIDENT ('Admin', RESEED, 0);
DBCC CHECKIDENT ('Hostel', RESEED, 0);
DBCC CHECKIDENT ('Room', RESEED, 0);
DBCC CHECKIDENT ('BookingRequest', RESEED, 0);
DBCC CHECKIDENT ('Notification', RESEED, 0);
DBCC CHECKIDENT ('PaymentTransaction', RESEED, 0);
DBCC CHECKIDENT ('Favorite', RESEED, 0);
DBCC CHECKIDENT ('Review', RESEED, 0);
DBCC CHECKIDENT ('ViolationReport', RESEED, 0);
DBCC CHECKIDENT ('RoomUpdateLog', RESEED, 0);

PRINT '✅ All data cleared and IDENTITY reset!';
