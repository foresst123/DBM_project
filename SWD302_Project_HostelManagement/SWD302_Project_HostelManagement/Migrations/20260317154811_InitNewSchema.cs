using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWD302_Project_HostelManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitNewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    admin_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'Active'"),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.admin_id);
                    table.CheckConstraint("CK_Admin_Status", "[status] IN ('Active', 'Inactive', 'Banned')");
                });

            migrationBuilder.CreateTable(
                name: "HostelOwner",
                columns: table => new
                {
                    owner_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'Active'"),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    business_license = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostelOwner", x => x.owner_id);
                    table.CheckConstraint("CK_HostelOwner_Status", "[status] IN ('Active', 'Inactive', 'Banned')");
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    tenant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'Active'"),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    identity_card = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.tenant_id);
                    table.CheckConstraint("CK_Tenant_Status", "[status] IN ('Active', 'Inactive', 'Banned')");
                });

            migrationBuilder.CreateTable(
                name: "Hostel",
                columns: table => new
                {
                    hostel_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValueSql: "'PendingApproval'"),
                    reject_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hostel", x => x.hostel_id);
                    table.CheckConstraint("CK_Hostel_Status", "[status] IN ('PendingApproval', 'Approved', 'Rejected', 'Deleted')");
                    table.ForeignKey(
                        name: "FK_Hostel_HostelOwner",
                        column: x => x.owner_id,
                        principalTable: "HostelOwner",
                        principalColumn: "owner_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favorite",
                columns: table => new
                {
                    favorite_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    hostel_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorite", x => x.favorite_id);
                    table.ForeignKey(
                        name: "FK_Favorite_Hostel",
                        column: x => x.hostel_id,
                        principalTable: "Hostel",
                        principalColumn: "hostel_id");
                    table.ForeignKey(
                        name: "FK_Favorite_Tenant",
                        column: x => x.tenant_id,
                        principalTable: "Tenant",
                        principalColumn: "tenant_id");
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    room_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hostel_id = table.Column<int>(type: "int", nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    room_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    room_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    capacity = table.Column<int>(type: "int", nullable: true),
                    price_per_month = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    area = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    floor = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValueSql: "'Available'"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.room_id);
                    table.CheckConstraint("CK_Room_Status", "[status] IN ('Available', 'Reserved', 'Occupied', 'Maintenance', 'Inactive')");
                    table.ForeignKey(
                        name: "FK_Room_Hostel",
                        column: x => x.hostel_id,
                        principalTable: "Hostel",
                        principalColumn: "hostel_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Room_HostelOwner",
                        column: x => x.owner_id,
                        principalTable: "HostelOwner",
                        principalColumn: "owner_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ViolationReport",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    reporter_tenant_id = table.Column<int>(type: "int", nullable: false),
                    reported_tenant_id = table.Column<int>(type: "int", nullable: true),
                    hostel_id = table.Column<int>(type: "int", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    evidence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'Pending'"),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    resolved_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationReport", x => x.report_id);
                    table.CheckConstraint("CK_ViolationReport_Status", "[status] IN ('Pending', 'Resolved', 'Dismissed')");
                    table.ForeignKey(
                        name: "FK_ViolationReport_Hostel",
                        column: x => x.hostel_id,
                        principalTable: "Hostel",
                        principalColumn: "hostel_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ViolationReport_ReportedTenant",
                        column: x => x.reported_tenant_id,
                        principalTable: "Tenant",
                        principalColumn: "tenant_id");
                    table.ForeignKey(
                        name: "FK_ViolationReport_Reporter",
                        column: x => x.reporter_tenant_id,
                        principalTable: "Tenant",
                        principalColumn: "tenant_id");
                });

            migrationBuilder.CreateTable(
                name: "BookingRequest",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    request_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'Booking'"),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValueSql: "'Pending'"),
                    reject_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRequest", x => x.booking_id);
                    table.CheckConstraint("CK_BookingRequest_Status", "[status] IN ('Pending', 'Approved', 'Rejected', 'Cancelled', 'PendingPayment', 'DepositPaid', 'Confirmed')");
                    table.CheckConstraint("CK_BookingRequest_Type", "[request_type] IN ('Booking', 'Viewing')");
                    table.ForeignKey(
                        name: "FK_BookingRequest_Room",
                        column: x => x.room_id,
                        principalTable: "Room",
                        principalColumn: "room_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingRequest_Tenant",
                        column: x => x.tenant_id,
                        principalTable: "Tenant",
                        principalColumn: "tenant_id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: true),
                    recipient_email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    message_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'Pending'"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.notification_id);
                    table.CheckConstraint("CK_Notification_Status", "[status] IN ('Pending', 'Sent', 'Failed')");
                    table.ForeignKey(
                        name: "FK_Notification_BookingRequest",
                        column: x => x.booking_id,
                        principalTable: "BookingRequest",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransaction",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gateway_ref = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValueSql: "'Pending'"),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransaction", x => x.transaction_id);
                    table.CheckConstraint("CK_PaymentTransaction_Status", "[status] IN ('Pending', 'Success', 'Failed', 'Cancelled', 'VerificationFailed')");
                    table.ForeignKey(
                        name: "FK_PaymentTransaction_BookingRequest",
                        column: x => x.booking_id,
                        principalTable: "BookingRequest",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTransaction_Tenant",
                        column: x => x.tenant_id,
                        principalTable: "Tenant",
                        principalColumn: "tenant_id");
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    hostel_id = table.Column<int>(type: "int", nullable: false),
                    booking_id = table.Column<int>(type: "int", nullable: true),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    owner_reply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_Review_BookingRequest",
                        column: x => x.booking_id,
                        principalTable: "BookingRequest",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Review_Hostel",
                        column: x => x.hostel_id,
                        principalTable: "Hostel",
                        principalColumn: "hostel_id");
                    table.ForeignKey(
                        name: "FK_Review_Tenant",
                        column: x => x.tenant_id,
                        principalTable: "Tenant",
                        principalColumn: "tenant_id");
                });

            migrationBuilder.CreateTable(
                name: "RoomUpdateLog",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    booking_id = table.Column<int>(type: "int", nullable: true),
                    changed_by_owner_id = table.Column<int>(type: "int", nullable: true),
                    status_before = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    status_after = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomUpdateLog", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_RoomUpdateLog_BookingRequest",
                        column: x => x.booking_id,
                        principalTable: "BookingRequest",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RoomUpdateLog_ChangedBy",
                        column: x => x.changed_by_owner_id,
                        principalTable: "HostelOwner",
                        principalColumn: "owner_id");
                    table.ForeignKey(
                        name: "FK_RoomUpdateLog_Room",
                        column: x => x.room_id,
                        principalTable: "Room",
                        principalColumn: "room_id");
                });

            migrationBuilder.CreateIndex(
                name: "UX_Admin_Email",
                table: "Admin",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequest_RoomId",
                table: "BookingRequest",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequest_Status",
                table: "BookingRequest",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequest_TenantId",
                table: "BookingRequest",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Favorite_hostel_id",
                table: "Favorite",
                column: "hostel_id");

            migrationBuilder.CreateIndex(
                name: "IX_Favorite_TenantId",
                table: "Favorite",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "UX_Favorite_TenantId_HostelId",
                table: "Favorite",
                columns: new[] { "tenant_id", "hostel_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hostel_OwnerId",
                table: "Hostel",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_Hostel_Status",
                table: "Hostel",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UX_HostelOwner_Email",
                table: "HostelOwner",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_BookingId",
                table: "Notification",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Status",
                table: "Notification",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransaction_BookingId",
                table: "PaymentTransaction",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransaction_tenant_id",
                table: "PaymentTransaction",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Review_booking_id",
                table: "Review",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_Review_HostelId",
                table: "Review",
                column: "hostel_id");

            migrationBuilder.CreateIndex(
                name: "IX_Review_tenant_id",
                table: "Review",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Room_HostelId",
                table: "Room",
                column: "hostel_id");

            migrationBuilder.CreateIndex(
                name: "IX_Room_OwnerId",
                table: "Room",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_Room_Status",
                table: "Room",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UX_Room_HostelId_RoomNumber",
                table: "Room",
                columns: new[] { "hostel_id", "room_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomUpdateLog_booking_id",
                table: "RoomUpdateLog",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_RoomUpdateLog_changed_by_owner_id",
                table: "RoomUpdateLog",
                column: "changed_by_owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_RoomUpdateLog_room_id",
                table: "RoomUpdateLog",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "UX_Tenant_Email",
                table: "Tenant",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReport_hostel_id",
                table: "ViolationReport",
                column: "hostel_id");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReport_reported_tenant_id",
                table: "ViolationReport",
                column: "reported_tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReport_reporter_tenant_id",
                table: "ViolationReport",
                column: "reporter_tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "Favorite");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "PaymentTransaction");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "RoomUpdateLog");

            migrationBuilder.DropTable(
                name: "ViolationReport");

            migrationBuilder.DropTable(
                name: "BookingRequest");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Tenant");

            migrationBuilder.DropTable(
                name: "Hostel");

            migrationBuilder.DropTable(
                name: "HostelOwner");
        }
    }
}
