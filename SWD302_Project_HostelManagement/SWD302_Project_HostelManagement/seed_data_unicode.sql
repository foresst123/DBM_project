-- =============================================
-- SEED DATA FOR HOSTEL MANAGEMENT SYSTEM
-- WITH PROPER UNICODE (N PREFIX)
-- =============================================

USE HostelManagementDB;

SET IDENTITY_INSERT [dbo].[Account] ON;

-- 1. ACCOUNTS
INSERT INTO [Account] ([account_id], [email], [password_hash], [role], [status], [avatar_url], [created_date])
VALUES
-- Admin
(1, 'admin@hostel.com', 'hashed_password_admin', 'Admin', 'Active', NULL, GETUTCDATE()),
-- Hostel Owner 1
(2, 'owner1@hostel.com', 'hashed_password_owner1', 'HostelOwner', 'Active', NULL, GETUTCDATE()),
-- Hostel Owner 2
(3, 'owner2@hostel.com', 'hashed_password_owner2', 'HostelOwner', 'Active', NULL, GETUTCDATE()),
-- Tenant 1
(4, 'tenant1@gmail.com', 'hashed_password_tenant1', 'Tenant', 'Active', NULL, GETUTCDATE()),
-- Tenant 2
(5, 'tenant2@gmail.com', 'hashed_password_tenant2', 'Tenant', 'Active', NULL, GETUTCDATE()),
-- Tenant 3
(6, 'tenant3@gmail.com', 'hashed_password_tenant3', 'Tenant', 'Active', NULL, GETUTCDATE()),
-- Guest
(7, 'guest@gmail.com', 'hashed_password_guest', 'Guest', 'Active', NULL, GETUTCDATE());

SET IDENTITY_INSERT [dbo].[Account] OFF;

-- 2. ADMIN PROFILE
SET IDENTITY_INSERT [dbo].[Admin] ON;
INSERT INTO [Admin] ([admin_id], [account_id], [name])
VALUES (1, 1, N'Super Admin');
SET IDENTITY_INSERT [dbo].[Admin] OFF;

-- 3. HOSTEL OWNER PROFILES
SET IDENTITY_INSERT [dbo].[HostelOwner] ON;
INSERT INTO [HostelOwner] ([owner_id], [account_id], [name], [phone_number], [business_license])
VALUES
(1, 2, N'Nguyễn Văn An', '0901234567', 'BL-2024-001'),
(2, 3, N'Trần Thị Bình', '0912345678', 'BL-2024-002');
SET IDENTITY_INSERT [dbo].[HostelOwner] OFF;

-- 4. TENANT PROFILES
SET IDENTITY_INSERT [dbo].[Tenant] ON;
INSERT INTO [Tenant] ([tenant_id], [account_id], [name], [phone_number], [identity_card])
VALUES
(1, 4, N'Lê Văn Cường', '0923456789', '079200012345'),
(2, 5, N'Phạm Thị Dung', '0934567890', '079200023456'),
(3, 6, N'Hoàng Văn Em', '0945678901', '079200034567');
SET IDENTITY_INSERT [dbo].[Tenant] OFF;

-- 5. HOSTELS
SET IDENTITY_INSERT [dbo].[Hostel] ON;
INSERT INTO [Hostel] ([hostel_id], [owner_id], [name], [address], [description], [status], [created_date], [updated_date])
VALUES
(1, 1, N'Nhà Trọ Ánh Dương', N'123 Nguyễn Trãi, Quận 1, TP.HCM', N'Nhà trọ sạch sẽ, an ninh, gần trường đại học', 'Approved', DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -25, GETUTCDATE())),
(2, 1, N'Phòng Trọ Bình Minh', N'456 Lê Lợi, Quận 3, TP.HCM', N'Phòng trọ tiện nghi, có thang máy', 'PendingApproval', DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, -5, GETUTCDATE())),
(3, 2, N'Căn Hộ Mini Hoàng Gia', N'789 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM', N'Căn hộ mini full nội thất, view đẹp', 'Approved', DATEADD(DAY, -20, GETUTCDATE()), DATEADD(DAY, -15, GETUTCDATE()));
SET IDENTITY_INSERT [dbo].[Hostel] OFF;

-- 6. ROOMS
SET IDENTITY_INSERT [dbo].[Room] ON;
INSERT INTO [Room] ([room_id], [hostel_id], [owner_id], [room_number], [room_type], [capacity], [price_per_month], [area], [floor], [status], [description], [created_at], [updated_at])
VALUES
-- Hostel 1 - Ánh Dương
(1, 1, 1, '101', 'Single', 1, 2500000, 20, 1, 'Occupied', N'Phòng đơn, có máy lạnh, WC riêng', GETUTCDATE(), GETUTCDATE()),
(2, 1, 1, '102', 'Double', 2, 3500000, 25, 1, 'Available', N'Phòng đôi, ban công, view đường', GETUTCDATE(), GETUTCDATE()),
(3, 1, 1, '201', 'Single', 1, 2800000, 22, 2, 'Reserved', N'Phòng đơn tầng 2, yên tĩnh', GETUTCDATE(), GETUTCDATE()),
-- Hostel 3 - Hoàng Gia
(4, 3, 2, 'A01', 'Studio', 2, 5000000, 35, 3, 'Available', N'Studio full nội thất, bếp riêng', GETUTCDATE(), GETUTCDATE()),
(5, 3, 2, 'A02', 'Studio', 2, 5500000, 40, 4, 'Occupied', N'Studio cao cấp, view thành phố', GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT [dbo].[Room] OFF;

-- 7. BOOKING REQUESTS
SET IDENTITY_INSERT [dbo].[BookingRequest] ON;
INSERT INTO [BookingRequest] ([booking_id], [room_id], [tenant_id], [request_type], [start_date], [end_date], [status], [reject_reason], [created_date], [updated_date])
VALUES
(1, 1, 1, 'Booking', CONVERT(DATE, '2025-01-01'), CONVERT(DATE, '2025-06-30'), 'Approved', NULL, DATEADD(DAY, -60, GETUTCDATE()), DATEADD(DAY, -55, GETUTCDATE())),
(2, 2, 2, 'Booking', CONVERT(DATE, '2025-04-01'), CONVERT(DATE, '2025-09-30'), 'Pending', NULL, DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE())),
(3, 3, 3, 'Viewing', CONVERT(DATE, '2025-03-15'), NULL, 'Rejected', N'Phòng đã có người đặt trước', DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -8, GETUTCDATE())),
(4, 4, 1, 'Booking', CONVERT(DATE, '2025-04-01'), CONVERT(DATE, '2025-09-30'), 'DepositPaid', NULL, DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()));
SET IDENTITY_INSERT [dbo].[BookingRequest] OFF;

-- 8. PAYMENT TRANSACTIONS
SET IDENTITY_INSERT [dbo].[PaymentTransaction] ON;
INSERT INTO [PaymentTransaction] ([transaction_id], [booking_id], [tenant_id], [amount], [payment_method], [gateway_ref], [status], [paid_at], [created_at])
VALUES
(1, 4, 1, 5000000, 'BankTransfer', 'PAY-2025-001', 'Success', DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()));
SET IDENTITY_INSERT [dbo].[PaymentTransaction] OFF;

-- 9. NOTIFICATIONS
SET IDENTITY_INSERT [dbo].[Notification] ON;
INSERT INTO [Notification] ([notification_id], [booking_id], [recipient_email], [subject], [message_content], [type], [status], [created_at], [sent_at])
VALUES
(1, 1, 'tenant1@gmail.com', N'Booking Approved - Nhà Trọ Ánh Dương Room 101', N'Yêu cầu đặt phòng của bạn đã được chấp thuận.', 'BookingApproved', 'Sent', DATEADD(DAY, -55, GETUTCDATE()), DATEADD(DAY, -55, GETUTCDATE())),
(2, 3, 'tenant3@gmail.com', N'Booking Rejected - Phòng 201', N'Rất tiếc, yêu cầu đặt phòng của bạn đã bị từ chối.', 'BookingRejected', 'Sent', DATEADD(DAY, -8, GETUTCDATE()), DATEADD(DAY, -8, GETUTCDATE())),
(3, 4, 'tenant1@gmail.com', N'Payment Successful - Room A01', N'Thanh toán đặt cọc thành công. Booking đã được xác nhận.', 'PaymentSuccess', 'Sent', DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()));
SET IDENTITY_INSERT [dbo].[Notification] OFF;

-- 10. FAVORITES
SET IDENTITY_INSERT [dbo].[Favorite] ON;
INSERT INTO [Favorite] ([favorite_id], [tenant_id], [hostel_id], [created_at])
VALUES
(1, 1, 3, DATEADD(DAY, -20, GETUTCDATE())),
(2, 2, 1, DATEADD(DAY, -15, GETUTCDATE())),
(3, 3, 1, DATEADD(DAY, -10, GETUTCDATE()));
SET IDENTITY_INSERT [dbo].[Favorite] OFF;

-- 11. REVIEWS
SET IDENTITY_INSERT [dbo].[Review] ON;
INSERT INTO [Review] ([review_id], [tenant_id], [hostel_id], [booking_id], [rating], [comment], [owner_reply], [created_at], [updated_at])
VALUES
(1, 1, 1, 1, 5, N'Phòng sạch sẽ, chủ trọ nhiệt tình, vị trí thuận tiện!', N'Cảm ơn bạn đã tin tưởng và ủng hộ nhà trọ chúng tôi!', DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -28, GETUTCDATE())),
(2, 3, 1, NULL, 4, N'Khu vực an ninh, giá hợp lý. Tuy nhiên nên cải thiện thêm chỗ để xe.', NULL, DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, -15, GETUTCDATE()));
SET IDENTITY_INSERT [dbo].[Review] OFF;

-- 12. VIOLATION REPORTS
SET IDENTITY_INSERT [dbo].[ViolationReport] ON;
INSERT INTO [ViolationReport] ([report_id], [reporter_tenant_id], [reported_account_id], [hostel_id], [reason], [evidence], [status], [created_date], [resolved_date])
VALUES
(1, 2, 2, 1, N'Thông tin phòng không đúng với thực tế, diện tích nhỏ hơn mô tả', 'photo_evidence_001.jpg', 'Pending', DATEADD(DAY, -5, GETUTCDATE()), NULL);
SET IDENTITY_INSERT [dbo].[ViolationReport] OFF;

-- 13. ROOM UPDATE LOGS
SET IDENTITY_INSERT [dbo].[RoomUpdateLog] ON;
INSERT INTO [RoomUpdateLog] ([log_id], [room_id], [booking_id], [changed_by_owner_id], [status_before], [status_after], [changed_at])
VALUES
(1, 1, 1, 1, 'Available', 'Occupied', DATEADD(DAY, -55, GETUTCDATE())),
(2, 3, NULL, 1, 'Available', 'Reserved', DATEADD(DAY, -10, GETUTCDATE()));
SET IDENTITY_INSERT [dbo].[RoomUpdateLog] OFF;

PRINT '✅ Seed data with Unicode inserted successfully!';

-- Verify Vietnamese data
PRINT '';
PRINT '=== VERIFICATION ===';
SELECT N'Tenant Names:' as 'Table', [name] FROM Tenant;
SELECT N'Owner Names:' as 'Table', [name] FROM HostelOwner;
SELECT N'Hostel Names:' as 'Table', [name] FROM Hostel;
SELECT N'Room Descriptions:' as 'Table', [description] FROM Room;
