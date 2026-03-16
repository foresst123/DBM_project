using SWD302_Project_HostelManagement.Models;

namespace SWD302_Project_HostelManagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Không seed nếu đã có data
        if (context.Accounts.Any()) return;

        // =============================================
        // 1. ACCOUNTS
        // =============================================
        // Tạo đủ 4 role: Admin, HostelOwner x2, Tenant x3, Guest x1
        var accounts = new List<Account>
        {
            // Admin
            new Account
            {
                Email = "admin@hostel.com",
                PasswordHash = "hashed_password_admin",
                Role = "Admin",
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // Hostel Owner 1
            new Account
            {
                Email = "owner1@hostel.com",
                PasswordHash = "hashed_password_owner1",
                Role = "HostelOwner",
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // Hostel Owner 2
            new Account
            {
                Email = "owner2@hostel.com",
                PasswordHash = "hashed_password_owner2",
                Role = "HostelOwner",
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // Tenant 1
            new Account
            {
                Email = "tenant1@gmail.com",
                PasswordHash = "hashed_password_tenant1",
                Role = "Tenant",
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // Tenant 2
            new Account
            {
                Email = "tenant2@gmail.com",
                PasswordHash = "hashed_password_tenant2",
                Role = "Tenant",
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // Tenant 3
            new Account
            {
                Email = "tenant3@gmail.com",
                PasswordHash = "hashed_password_tenant3",
                Role = "Tenant",
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // Guest
            new Account
            {
                Email = "guest@gmail.com",
                PasswordHash = "hashed_password_guest",
                Role = "Guest",
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            }
        };
        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();

        // =============================================
        // 2. ADMIN PROFILE
        // =============================================
        var admin = new Admin
        {
            AccountId = accounts[0].AccountId,
            Name = "Super Admin"
        };
        await context.Admins.AddAsync(admin);

        // =============================================
        // 3. HOSTEL OWNER PROFILES
        // =============================================
        var owners = new List<HostelOwner>
        {
            new HostelOwner
            {
                AccountId = accounts[1].AccountId,
                Name = "Nguyễn Văn An",
                PhoneNumber = "0901234567",
                BusinessLicense = "BL-2024-001"
            },
            new HostelOwner
            {
                AccountId = accounts[2].AccountId,
                Name = "Trần Thị Bình",
                PhoneNumber = "0912345678",
                BusinessLicense = "BL-2024-002"
            }
        };
        await context.HostelOwners.AddRangeAsync(owners);

        // =============================================
        // 4. TENANT PROFILES
        // =============================================
        var tenants = new List<Tenant>
        {
            new Tenant
            {
                AccountId = accounts[3].AccountId,
                Name = "Lê Văn Cường",
                PhoneNumber = "0923456789",
                IdentityCard = "079200012345"
            },
            new Tenant
            {
                AccountId = accounts[4].AccountId,
                Name = "Phạm Thị Dung",
                PhoneNumber = "0934567890",
                IdentityCard = "079200023456"
            },
            new Tenant
            {
                AccountId = accounts[5].AccountId,
                Name = "Hoàng Văn Em",
                PhoneNumber = "0945678901",
                IdentityCard = "079200034567"
            }
        };
        await context.Tenants.AddRangeAsync(tenants);
        await context.SaveChangesAsync();

        // =============================================
        // 5. HOSTELS
        // =============================================
        var hostels = new List<Hostel>
        {
            new Hostel
            {
                OwnerId = owners[0].OwnerId,
                Name = "Nhà Trọ Ánh Dương",
                Address = "123 Nguyễn Trãi, Quận 1, TP.HCM",
                Description = "Nhà trọ sạch sẽ, an ninh, gần trường đại học",
                Status = "Approved",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                UpdatedDate = DateTime.UtcNow.AddDays(-25)
            },
            new Hostel
            {
                OwnerId = owners[0].OwnerId,
                Name = "Phòng Trọ Bình Minh",
                Address = "456 Lê Lợi, Quận 3, TP.HCM",
                Description = "Phòng trọ tiện nghi, có thang máy",
                Status = "PendingApproval",
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                UpdatedDate = DateTime.UtcNow.AddDays(-5)
            },
            new Hostel
            {
                OwnerId = owners[1].OwnerId,
                Name = "Căn Hộ Mini Hoàng Gia",
                Address = "789 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM",
                Description = "Căn hộ mini full nội thất, view đẹp",
                Status = "Approved",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                UpdatedDate = DateTime.UtcNow.AddDays(-15)
            }
        };
        await context.Hostels.AddRangeAsync(hostels);
        await context.SaveChangesAsync();

        // =============================================
        // 6. ROOMS
        // =============================================
        var rooms = new List<Room>
        {
            // Hostel 1 - Ánh Dương
            new Room
            {
                HostelId = hostels[0].HostelId,
                OwnerId = owners[0].OwnerId,
                RoomNumber = "101",
                RoomType = "Single",
                Capacity = 1,
                PricePerMonth = 2500000,
                Area = 20,
                Floor = 1,
                Status = "Occupied",
                Description = "Phòng đơn, có máy lạnh, WC riêng"
            },
            new Room
            {
                HostelId = hostels[0].HostelId,
                OwnerId = owners[0].OwnerId,
                RoomNumber = "102",
                RoomType = "Double",
                Capacity = 2,
                PricePerMonth = 3500000,
                Area = 25,
                Floor = 1,
                Status = "Available",
                Description = "Phòng đôi, ban công, view đường"
            },
            new Room
            {
                HostelId = hostels[0].HostelId,
                OwnerId = owners[0].OwnerId,
                RoomNumber = "201",
                RoomType = "Single",
                Capacity = 1,
                PricePerMonth = 2800000,
                Area = 22,
                Floor = 2,
                Status = "Reserved",
                Description = "Phòng đơn tầng 2, yên tĩnh"
            },
            // Hostel 3 - Hoàng Gia
            new Room
            {
                HostelId = hostels[2].HostelId,
                OwnerId = owners[1].OwnerId,
                RoomNumber = "A01",
                RoomType = "Studio",
                Capacity = 2,
                PricePerMonth = 5000000,
                Area = 35,
                Floor = 3,
                Status = "Available",
                Description = "Studio full nội thất, bếp riêng"
            },
            new Room
            {
                HostelId = hostels[2].HostelId,
                OwnerId = owners[1].OwnerId,
                RoomNumber = "A02",
                RoomType = "Studio",
                Capacity = 2,
                PricePerMonth = 5500000,
                Area = 40,
                Floor = 4,
                Status = "Occupied",
                Description = "Studio cao cấp, view thành phố"
            }
        };
        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();

        // =============================================
        // 7. BOOKING REQUESTS
        // =============================================
        var bookings = new List<BookingRequest>
        {
            // Approved booking - tenant 1 - room 101
            new BookingRequest
            {
                RoomId = rooms[0].RoomId,
                TenantId = tenants[0].TenantId,
                RequestType = "Booking",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 6, 30),
                Status = "Approved",
                CreatedDate = DateTime.UtcNow.AddDays(-60),
                UpdatedDate = DateTime.UtcNow.AddDays(-55)
            },
            // Pending booking - tenant 2 - room 102
            new BookingRequest
            {
                RoomId = rooms[1].RoomId,
                TenantId = tenants[1].TenantId,
                RequestType = "Booking",
                StartDate = new DateOnly(2025, 4, 1),
                EndDate = new DateOnly(2025, 9, 30),
                Status = "Pending",
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                UpdatedDate = DateTime.UtcNow.AddDays(-2)
            },
            // Rejected booking - tenant 3 - room 201
            new BookingRequest
            {
                RoomId = rooms[2].RoomId,
                TenantId = tenants[2].TenantId,
                RequestType = "Viewing",
                StartDate = new DateOnly(2025, 3, 15),
                EndDate = null,
                Status = "Rejected",
                RejectReason = "Phòng đã có người đặt trước",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                UpdatedDate = DateTime.UtcNow.AddDays(-8)
            },
            // DepositPaid booking - tenant 1 - room A01
            new BookingRequest
            {
                RoomId = rooms[3].RoomId,
                TenantId = tenants[0].TenantId,
                RequestType = "Booking",
                StartDate = new DateOnly(2025, 4, 1),
                EndDate = new DateOnly(2025, 9, 30),
                Status = "DepositPaid",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                UpdatedDate = DateTime.UtcNow.AddDays(-10)
            }
        };
        await context.BookingRequests.AddRangeAsync(bookings);
        await context.SaveChangesAsync();

        // =============================================
        // 8. PAYMENT TRANSACTIONS
        // =============================================
        var payments = new List<PaymentTransaction>
        {
            new PaymentTransaction
            {
                BookingId = bookings[3].BookingId,
                TenantId = tenants[0].TenantId,
                Amount = 5000000,
                PaymentMethod = "BankTransfer",
                GatewayRef = "PAY-2025-001",
                Status = "Success",
                PaidAt = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };
        await context.PaymentTransactions.AddRangeAsync(payments);

        // =============================================
        // 9. NOTIFICATIONS
        // =============================================
        var notifications = new List<Notification>
        {
            new Notification
            {
                BookingId = bookings[0].BookingId,
                RecipientEmail = "tenant1@gmail.com",
                Subject = "Booking Approved - Nhà Trọ Ánh Dương Room 101",
                MessageContent = "Yêu cầu đặt phòng của bạn đã được chấp thuận.",
                Type = "BookingApproved",
                Status = "Sent",
                CreatedAt = DateTime.UtcNow.AddDays(-55),
                SentAt = DateTime.UtcNow.AddDays(-55)
            },
            new Notification
            {
                BookingId = bookings[2].BookingId,
                RecipientEmail = "tenant3@gmail.com",
                Subject = "Booking Rejected - Phòng 201",
                MessageContent = "Rất tiếc, yêu cầu đặt phòng của bạn đã bị từ chối.",
                Type = "BookingRejected",
                Status = "Sent",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                SentAt = DateTime.UtcNow.AddDays(-8)
            },
            new Notification
            {
                BookingId = bookings[3].BookingId,
                RecipientEmail = "tenant1@gmail.com",
                Subject = "Payment Successful - Room A01",
                MessageContent = "Thanh toán đặt cọc thành công. Booking đã được xác nhận.",
                Type = "PaymentSuccess",
                Status = "Sent",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                SentAt = DateTime.UtcNow.AddDays(-10)
            }
        };
        await context.Notifications.AddRangeAsync(notifications);

        // =============================================
        // 10. FAVORITES
        // =============================================
        var favorites = new List<Favorite>
        {
            new Favorite
            {
                TenantId = tenants[0].TenantId,
                HostelId = hostels[2].HostelId,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Favorite
            {
                TenantId = tenants[1].TenantId,
                HostelId = hostels[0].HostelId,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Favorite
            {
                TenantId = tenants[2].TenantId,
                HostelId = hostels[0].HostelId,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };
        await context.Favorites.AddRangeAsync(favorites);

        // =============================================
        // 11. REVIEWS
        // =============================================
        var reviews = new List<Review>
        {
            new Review
            {
                TenantId = tenants[0].TenantId,
                HostelId = hostels[0].HostelId,
                BookingId = bookings[0].BookingId,
                Rating = 5,
                Comment = "Phòng sạch sẽ, chủ trọ nhiệt tình, vị trí thuận tiện!",
                OwnerReply = "Cảm ơn bạn đã tin tưởng và ủng hộ nhà trọ chúng tôi!",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-28)
            },
            new Review
            {
                TenantId = tenants[2].TenantId,
                HostelId = hostels[0].HostelId,
                BookingId = null,
                Rating = 4,
                Comment = "Khu vực an ninh, giá hợp lý. Tuy nhiên nên cải thiện thêm chỗ để xe.",
                OwnerReply = null,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };
        await context.Reviews.AddRangeAsync(reviews);

        // =============================================
        // 12. VIOLATION REPORTS
        // =============================================
        var violations = new List<ViolationReport>
        {
            new ViolationReport
            {
                ReporterTenantId = tenants[1].TenantId,
                ReportedAccountId = accounts[1].AccountId,
                HostelId = hostels[0].HostelId,
                Reason = "Thông tin phòng không đúng với thực tế, diện tích nhỏ hơn mô tả",
                Evidence = "photo_evidence_001.jpg",
                Status = "Pending",
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            }
        };
        await context.ViolationReports.AddRangeAsync(violations);

        // =============================================
        // 13. ROOM UPDATE LOGS
        // =============================================
        var logs = new List<RoomUpdateLog>
        {
            new RoomUpdateLog
            {
                RoomId = rooms[0].RoomId,
                BookingId = bookings[0].BookingId,
                ChangedByOwnerId = owners[0].OwnerId,
                StatusBefore = "Available",
                StatusAfter = "Occupied",
                ChangedAt = DateTime.UtcNow.AddDays(-55)
            },
            new RoomUpdateLog
            {
                RoomId = rooms[2].RoomId,
                BookingId = null,
                ChangedByOwnerId = owners[0].OwnerId,
                StatusBefore = "Available",
                StatusAfter = "Reserved",
                ChangedAt = DateTime.UtcNow.AddDays(-10)
            }
        };
        await context.RoomUpdateLogs.AddRangeAsync(logs);

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Seed data completed successfully!");
    }
}
