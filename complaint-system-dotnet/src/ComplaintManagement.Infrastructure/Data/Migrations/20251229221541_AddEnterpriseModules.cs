using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssetId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerContactId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerFeedback",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerLocationId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerSatisfactionRating",
                table: "Complaints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderContract",
                table: "Complaints",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderWarranty",
                table: "Complaints",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PrimaryEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BillingAddressLine1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BillingAddressLine2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BillingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PanNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Segment = table.Column<int>(type: "int", nullable: true),
                    EmployeeCount = table.Column<int>(type: "int", nullable: true),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    CustomerSince = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryAccountManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreditTerms = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CustomerRating = table.Column<int>(type: "int", nullable: true),
                    PortalEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PreferredTimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AuthenticationProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalCustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StatusReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_AuthenticationProviders_AuthenticationProviderId",
                        column: x => x.AuthenticationProviderId,
                        principalTable: "AuthenticationProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Customers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_AccountManagerId",
                        column: x => x.AccountManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Customers_Users_SecondaryAccountManagerId",
                        column: x => x.SecondaryAccountManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicenseKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GracePeriodDays = table.Column<int>(type: "int", nullable: false),
                    MaxUsers = table.Column<int>(type: "int", nullable: true),
                    MaxCustomers = table.Column<int>(type: "int", nullable: true),
                    MaxProducts = table.Column<int>(type: "int", nullable: true),
                    MaxAssets = table.Column<int>(type: "int", nullable: true),
                    MaxContracts = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Signature = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MachineId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidationFailures = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Licenses_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    PrimaryEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PanNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IndustrySegment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PartnerSince = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PortalEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CanManageCustomers = table.Column<bool>(type: "bit", nullable: false),
                    CanViewAllCustomerTickets = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateCustomerTickets = table.Column<bool>(type: "bit", nullable: false),
                    PortalBranding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthenticationProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalPartnerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccountManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryAccountManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partners_AuthenticationProviders_AuthenticationProviderId",
                        column: x => x.AuthenticationProviderId,
                        principalTable: "AuthenticationProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Partners_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partners_Users_AccountManagerId",
                        column: x => x.AccountManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Partners_Users_SecondaryAccountManagerId",
                        column: x => x.SecondaryAccountManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FullPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultWarrantyMonths = table.Column<int>(type: "int", nullable: true),
                    DefaultResponseTimeHours = table.Column<int>(type: "int", nullable: true),
                    DefaultResolutionTimeHours = table.Column<int>(type: "int", nullable: true),
                    DefaultSLASettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultTaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DefaultHSNCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DefaultSACCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RequireSerialNumber = table.Column<bool>(type: "bit", nullable: false),
                    TrackInventory = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    AllowProducts = table.Column<bool>(type: "bit", nullable: false),
                    AllowSubCategories = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExternalCategoryId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CustomAttributes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductCategories_SLASettings_DefaultSLASettingsId",
                        column: x => x.DefaultSLASettingsId,
                        principalTable: "SLASettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CustomerLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsBillingAddress = table.Column<bool>(type: "bit", nullable: false),
                    IsShippingAddress = table.Column<bool>(type: "bit", nullable: false),
                    OperatingHours = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AccessInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalLocationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerLocations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenseModuleActivations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicenseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EnabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModuleConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModuleLimits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseModuleActivations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseModuleActivations_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExternalContractId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PONumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActivatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    RenewalPeriodMonths = table.Column<int>(type: "int", nullable: true),
                    RenewalNoticeDays = table.Column<int>(type: "int", nullable: true),
                    LastRenewalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRenewalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RenewalCount = table.Column<int>(type: "int", nullable: false),
                    MaxRenewals = table.Column<int>(type: "int", nullable: true),
                    ContractValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AnnualValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MonthlyValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    BillingFrequency = table.Column<int>(type: "int", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CoverageType = table.Column<int>(type: "int", nullable: false),
                    IncludesOnsite = table.Column<bool>(type: "bit", nullable: false),
                    IncludesRemote = table.Column<bool>(type: "bit", nullable: false),
                    IncludesParts = table.Column<bool>(type: "bit", nullable: false),
                    IncludesLabor = table.Column<bool>(type: "bit", nullable: false),
                    IncludesTravel = table.Column<bool>(type: "bit", nullable: false),
                    IncludesConsumables = table.Column<bool>(type: "bit", nullable: false),
                    IncludedHours = table.Column<int>(type: "int", nullable: true),
                    UsedHours = table.Column<int>(type: "int", nullable: true),
                    IncludedIncidents = table.Column<int>(type: "int", nullable: true),
                    UsedIncidents = table.Column<int>(type: "int", nullable: true),
                    IncludedPMVisits = table.Column<int>(type: "int", nullable: true),
                    CompletedPMVisits = table.Column<int>(type: "int", nullable: true),
                    ExcludedItems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncludedItems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SLASettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponseTimeHours = table.Column<int>(type: "int", nullable: true),
                    ResolutionTimeHours = table.Column<int>(type: "int", nullable: true),
                    SupportHoursType = table.Column<int>(type: "int", nullable: true),
                    SupportHoursDefinition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UptimeGuarantee = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TerminationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TerminatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalDocuments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialConditions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AccountManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TechnicalManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contracts_SLASettings_SLASettingsId",
                        column: x => x.SLASettingsId,
                        principalTable: "SLASettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PartnerContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsPortalUser = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PortalRole = table.Column<int>(type: "int", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PasswordExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPasswordChangeAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthType = table.Column<int>(type: "int", nullable: false),
                    ExternalUserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PreferredTimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReceiveEmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    ReceiveSmsNotifications = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryContact = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerContacts_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartnerCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimaryPartner = table.Column<bool>(type: "bit", nullable: false),
                    RelationshipStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelationshipEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CanCreateTickets = table.Column<bool>(type: "bit", nullable: false),
                    CanViewAllTickets = table.Column<bool>(type: "bit", nullable: false),
                    CanManageContacts = table.Column<bool>(type: "bit", nullable: false),
                    CanViewContracts = table.Column<bool>(type: "bit", nullable: false),
                    CanViewAssets = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EndReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerCustomers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerCustomers_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PartNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UPC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ISBN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HSNCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SACCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CostPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MSRP = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MinimumPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    TaxType = table.Column<int>(type: "int", nullable: false),
                    PriceBook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitOfMeasure = table.Column<int>(type: "int", nullable: false),
                    CustomUnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinOrderQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxOrderQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    QuantityIncrement = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    QuantityPerPack = table.Column<int>(type: "int", nullable: true),
                    TrackInventory = table.Column<bool>(type: "bit", nullable: false),
                    QuantityInStock = table.Column<int>(type: "int", nullable: true),
                    QuantityReserved = table.Column<int>(type: "int", nullable: true),
                    ReorderLevel = table.Column<int>(type: "int", nullable: true),
                    ReorderQuantity = table.Column<int>(type: "int", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    AllowBackorder = table.Column<bool>(type: "bit", nullable: false),
                    DefaultWarehouse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Specifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dimensions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Videos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultWarrantyMonths = table.Column<int>(type: "int", nullable: true),
                    ExtendedWarrantyMonths = table.Column<int>(type: "int", nullable: true),
                    WarrantyTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DefaultSLASettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupportUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsServiceable = table.Column<bool>(type: "bit", nullable: false),
                    RequiresInstallation = table.Column<bool>(type: "bit", nullable: false),
                    IsSubscription = table.Column<bool>(type: "bit", nullable: false),
                    BillingFrequency = table.Column<int>(type: "int", nullable: true),
                    TrialPeriodDays = table.Column<int>(type: "int", nullable: true),
                    SetupFee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    CancellationPolicy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LaunchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndOfSaleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndOfLifeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndOfSupportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsConfigurable = table.Column<bool>(type: "bit", nullable: false),
                    RequireSerialNumber = table.Column<bool>(type: "bit", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExternalProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VendorProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RelatedProductIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CrossSellProductIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpSellProductIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessoryProductIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacementProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Products_Products_ParentProductId",
                        column: x => x.ParentProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Products_ReplacementProductId",
                        column: x => x.ReplacementProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_SLASettings_DefaultSLASettingsId",
                        column: x => x.DefaultSLASettingsId,
                        principalTable: "SLASettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CustomerContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsPortalUser = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PortalRole = table.Column<int>(type: "int", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PasswordExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPasswordChangeAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthType = table.Column<int>(type: "int", nullable: false),
                    ExternalUserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CanCreateTickets = table.Column<bool>(type: "bit", nullable: false),
                    CanViewAllLocationTickets = table.Column<bool>(type: "bit", nullable: false),
                    CanViewAllCustomerTickets = table.Column<bool>(type: "bit", nullable: false),
                    CanManageContacts = table.Column<bool>(type: "bit", nullable: false),
                    CanViewContracts = table.Column<bool>(type: "bit", nullable: false),
                    CanViewAssets = table.Column<bool>(type: "bit", nullable: false),
                    CanApproveEscalations = table.Column<bool>(type: "bit", nullable: false),
                    CanDownloadReports = table.Column<bool>(type: "bit", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PreferredTimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReceiveEmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    ReceiveSmsNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NotificationPreferences = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsPrimaryContact = table.Column<bool>(type: "bit", nullable: false),
                    IsBillingContact = table.Column<bool>(type: "bit", nullable: false),
                    IsTechnicalContact = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContacts_CustomerLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "CustomerLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CustomerContacts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractRenewalHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RenewalNumber = table.Column<int>(type: "int", nullable: false),
                    PreviousEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RenewalPeriodMonths = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    NewValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PriceChangePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    RenewalDiscount = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PONumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreviousType = table.Column<int>(type: "int", nullable: true),
                    NewType = table.Column<int>(type: "int", nullable: true),
                    PreviousCoverageType = table.Column<int>(type: "int", nullable: true),
                    NewCoverageType = table.Column<int>(type: "int", nullable: true),
                    ChangesSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoticeSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractRenewalHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractRenewalHistory_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductPriceLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PartnerTier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerSegment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MarkupPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MinQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TierPricing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    MinOrderValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PromoCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Conditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPriceLists_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductPriceLists_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductPriceLists_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    ExtendedDurationMonths = table.Column<int>(type: "int", nullable: true),
                    GracePeriodDays = table.Column<int>(type: "int", nullable: true),
                    MaxDurationMonths = table.Column<int>(type: "int", nullable: true),
                    CoveredItems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExcludedItems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverageType = table.Column<int>(type: "int", nullable: false),
                    IncludesParts = table.Column<bool>(type: "bit", nullable: false),
                    IncludesLabor = table.Column<bool>(type: "bit", nullable: false),
                    IncludesOnsite = table.Column<bool>(type: "bit", nullable: false),
                    IncludesShipping = table.Column<bool>(type: "bit", nullable: false),
                    IsTransferable = table.Column<bool>(type: "bit", nullable: false),
                    RequiresRegistration = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationDeadlineDays = table.Column<int>(type: "int", nullable: true),
                    RequiresProofOfPurchase = table.Column<bool>(type: "bit", nullable: false),
                    Conditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtendedWarrantyPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ExtendedWarrantyPricePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    ResponseTimeHours = table.Column<int>(type: "int", nullable: true),
                    ResolutionTimeHours = table.Column<int>(type: "int", nullable: true),
                    SupportHoursType = table.Column<int>(type: "int", nullable: true),
                    MaxClaims = table.Column<int>(type: "int", nullable: true),
                    MaxClaimValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxClaimValuePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DeductibleAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalWarrantyId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warranties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warranties_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Warranties_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warranties_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WarrantyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssetTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RFIDTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManufactureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InstallationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommissionedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtendedWarrantyEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndOfLifeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetiredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisposedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SalvageValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ReplacementCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    PONumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Vendor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DepreciationMethod = table.Column<int>(type: "int", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "int", nullable: true),
                    DepreciationRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AccumulatedDepreciation = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    OwnershipType = table.Column<int>(type: "int", nullable: false),
                    LeaseAgreementNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LeaseStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaseEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeasePayment = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    StatusReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsOperational = table.Column<bool>(type: "bit", nullable: false),
                    Specifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoftwareVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MACAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Hostname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Building = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Floor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Room = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Rack = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RackUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<double>(type: "float(10)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<double>(type: "float(10)", precision: 10, scale: 7, nullable: true),
                    MaintenanceFrequency = table.Column<int>(type: "int", nullable: false),
                    MaintenanceIntervalDays = table.Column<int>(type: "int", nullable: true),
                    ServiceCallCount = table.Column<int>(type: "int", nullable: false),
                    TotalDowntimeHours = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MTBF = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MTTR = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    CurrentMeterReading = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MeterUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastMeterReadingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AverageDailyUsage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CostCenter = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalAssetId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParentAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_Assets_ParentAssetId",
                        column: x => x.ParentAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assets_CustomerLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "CustomerLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assets_Warranties_WarrantyId",
                        column: x => x.WarrantyId,
                        principalTable: "Warranties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetServiceHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    WorkPerformed = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProblemDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RootCause = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationHours = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TravelTimeHours = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    DowntimeHours = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    NextServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    ServiceLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServiceAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PartsCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TravelCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    OtherCosts = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    IsBillable = table.Column<bool>(type: "bit", nullable: false),
                    IsWarrantyCovered = table.Column<bool>(type: "bit", nullable: false),
                    IsContractCovered = table.Column<bool>(type: "bit", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PartsUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartsReplaced = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedPartSerials = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StatusBefore = table.Column<int>(type: "int", nullable: true),
                    StatusAfter = table.Column<int>(type: "int", nullable: true),
                    ConditionBefore = table.Column<int>(type: "int", nullable: true),
                    ConditionAfter = table.Column<int>(type: "int", nullable: true),
                    MeterReadingBefore = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MeterReadingAfter = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    FirmwareVersionBefore = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirmwareVersionAfter = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoftwareVersionBefore = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoftwareVersionAfter = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConfigurationChanges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerSignedOff = table.Column<bool>(type: "bit", nullable: false),
                    SignedOffBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SignedOffAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerRating = table.Column<int>(type: "int", nullable: true),
                    CustomerFeedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FollowUpActions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetServiceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetServiceHistory_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetServiceHistory_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetServiceHistory_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ContractItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SerialNumbers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CoverageStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoverageEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ItemValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    ResponseTimeHours = table.Column<int>(type: "int", nullable: true),
                    ResolutionTimeHours = table.Column<int>(type: "int", nullable: true),
                    PriorityLevel = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SiteInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaintenanceIntervalDays = table.Column<int>(type: "int", nullable: true),
                    ServiceCallCount = table.Column<int>(type: "int", nullable: false),
                    SpecialConditions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractItems_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContractItems_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(1534), new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(1706) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(2062), new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(2063) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(2069), new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(2069) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(2075), new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(2075) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(2079), new DateTime(2025, 12, 29, 22, 15, 40, 40, DateTimeKind.Utc).AddTicks(2079) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1135), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1137) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1145), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1146) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1149), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1150) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1153), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1153) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1156), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1157) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1160), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1161) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1164), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1164) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1168), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1168) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1171), new DateTime(2025, 12, 29, 22, 15, 40, 43, DateTimeKind.Utc).AddTicks(1172) });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_AssetId",
                table: "Complaints",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ContractId",
                table: "Complaints",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CustomerContactId",
                table: "Complaints",
                column: "CustomerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CustomerId",
                table: "Complaints",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CustomerLocationId",
                table: "Complaints",
                column: "CustomerLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ProductId",
                table: "Complaints",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetTag",
                table: "Assets",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Barcode",
                table: "Assets",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CompanyId",
                table: "Assets",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CompanyId_CustomerId",
                table: "Assets",
                columns: new[] { "CompanyId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CompanyId_Status",
                table: "Assets",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Condition",
                table: "Assets",
                column: "Condition");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ContractId",
                table: "Assets",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CustomerId",
                table: "Assets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CustomerId_Status",
                table: "Assets",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_LocationId",
                table: "Assets",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_NextServiceDate",
                table: "Assets",
                column: "NextServiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ParentAssetId",
                table: "Assets",
                column: "ParentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ProductId",
                table: "Assets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_RFIDTag",
                table: "Assets",
                column: "RFIDTag");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SerialNumber",
                table: "Assets",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Status",
                table: "Assets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Type",
                table: "Assets",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WarrantyEndDate",
                table: "Assets",
                column: "WarrantyEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WarrantyId",
                table: "Assets",
                column: "WarrantyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_AssetId",
                table: "AssetServiceHistory",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_AssetId_ServiceStartDate",
                table: "AssetServiceHistory",
                columns: new[] { "AssetId", "ServiceStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_CompanyId",
                table: "AssetServiceHistory",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_CompanyId_ServiceStartDate",
                table: "AssetServiceHistory",
                columns: new[] { "CompanyId", "ServiceStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_ComplaintId",
                table: "AssetServiceHistory",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_ContractId",
                table: "AssetServiceHistory",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_Result",
                table: "AssetServiceHistory",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_ServiceEndDate",
                table: "AssetServiceHistory",
                column: "ServiceEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_ServiceNumber",
                table: "AssetServiceHistory",
                column: "ServiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_ServiceStartDate",
                table: "AssetServiceHistory",
                column: "ServiceStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_ServiceType",
                table: "AssetServiceHistory",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_AssetServiceHistory_TechnicianId",
                table: "AssetServiceHistory",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractItems_AssetId",
                table: "ContractItems",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractItems_ContractId",
                table: "ContractItems",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractItems_ContractId_Status",
                table: "ContractItems",
                columns: new[] { "ContractId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractItems_ProductId",
                table: "ContractItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractItems_Status",
                table: "ContractItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRenewalHistory_Action",
                table: "ContractRenewalHistory",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRenewalHistory_ContractId",
                table: "ContractRenewalHistory",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRenewalHistory_ContractId_RenewalNumber",
                table: "ContractRenewalHistory",
                columns: new[] { "ContractId", "RenewalNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractRenewalHistory_RenewalDate",
                table: "ContractRenewalHistory",
                column: "RenewalDate");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CompanyId",
                table: "Contracts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CompanyId_Status",
                table: "Contracts",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractNumber",
                table: "Contracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CustomerId",
                table: "Contracts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CustomerId_Status",
                table: "Contracts",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_EndDate",
                table: "Contracts",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PartnerId",
                table: "Contracts",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SLASettingsId",
                table: "Contracts",
                column: "SLASettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_Status",
                table: "Contracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_Type",
                table: "Contracts",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_CustomerId",
                table: "CustomerContacts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_CustomerId_Email",
                table: "CustomerContacts",
                columns: new[] { "CustomerId", "Email" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_Email",
                table: "CustomerContacts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_IsPortalUser",
                table: "CustomerContacts",
                column: "IsPortalUser");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_IsPrimaryContact",
                table: "CustomerContacts",
                column: "IsPrimaryContact");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_LocationId",
                table: "CustomerContacts",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLocations_City",
                table: "CustomerLocations",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLocations_CustomerId_Code",
                table: "CustomerLocations",
                columns: new[] { "CustomerId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLocations_IsPrimary",
                table: "CustomerLocations",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLocations_Type",
                table: "CustomerLocations",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AccountManagerId",
                table: "Customers",
                column: "AccountManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AuthenticationProviderId",
                table: "Customers",
                column: "AuthenticationProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyId_Code",
                table: "Customers",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ExternalCustomerId",
                table: "Customers",
                column: "ExternalCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PrimaryEmail",
                table: "Customers",
                column: "PrimaryEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SecondaryAccountManagerId",
                table: "Customers",
                column: "SecondaryAccountManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Status",
                table: "Customers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Type",
                table: "Customers",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseModuleActivations_ExpiresAt",
                table: "LicenseModuleActivations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseModuleActivations_LicenseId",
                table: "LicenseModuleActivations",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseModuleActivations_LicenseId_IsEnabled",
                table: "LicenseModuleActivations",
                columns: new[] { "LicenseId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_LicenseModuleActivations_LicenseId_Module",
                table: "LicenseModuleActivations",
                columns: new[] { "LicenseId", "Module" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_CompanyId",
                table: "Licenses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_CompanyId_IsActive",
                table: "Licenses",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_ExpiresAt",
                table: "Licenses",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseKey",
                table: "Licenses",
                column: "LicenseKey");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Status",
                table: "Licenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContacts_Email",
                table: "PartnerContacts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContacts_IsPortalUser",
                table: "PartnerContacts",
                column: "IsPortalUser");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContacts_IsPrimaryContact",
                table: "PartnerContacts",
                column: "IsPrimaryContact");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContacts_PartnerId",
                table: "PartnerContacts",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContacts_PartnerId_Email",
                table: "PartnerContacts",
                columns: new[] { "PartnerId", "Email" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerCustomers_CustomerId",
                table: "PartnerCustomers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerCustomers_IsActive",
                table: "PartnerCustomers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerCustomers_IsPrimaryPartner",
                table: "PartnerCustomers",
                column: "IsPrimaryPartner");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerCustomers_PartnerId_CustomerId",
                table: "PartnerCustomers",
                columns: new[] { "PartnerId", "CustomerId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_AccountManagerId",
                table: "Partners",
                column: "AccountManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_AuthenticationProviderId",
                table: "Partners",
                column: "AuthenticationProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_CompanyId_Code",
                table: "Partners",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_ExternalPartnerId",
                table: "Partners",
                column: "ExternalPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PrimaryEmail",
                table: "Partners",
                column: "PrimaryEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_SecondaryAccountManagerId",
                table: "Partners",
                column: "SecondaryAccountManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_Status",
                table: "Partners",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CompanyId_Code",
                table: "ProductCategories",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_DefaultSLASettingsId",
                table: "ProductCategories",
                column: "DefaultSLASettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_IsActive",
                table: "ProductCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Level",
                table: "ProductCategories",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentCategoryId",
                table: "ProductCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Path",
                table: "ProductCategories",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Slug",
                table: "ProductCategories",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_CustomerId",
                table: "ProductPriceLists",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_IsActive",
                table: "ProductPriceLists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_PartnerId",
                table: "ProductPriceLists",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_Priority",
                table: "ProductPriceLists",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_ProductId",
                table: "ProductPriceLists",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_ProductId_Type_PartnerId_CustomerId_MinQuantity",
                table: "ProductPriceLists",
                columns: new[] { "ProductId", "Type", "PartnerId", "CustomerId", "MinQuantity" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_PromoCode",
                table: "ProductPriceLists",
                column: "PromoCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_Type",
                table: "ProductPriceLists",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_ValidFrom",
                table: "ProductPriceLists",
                column: "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceLists_ValidTo",
                table: "ProductPriceLists",
                column: "ValidTo");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Brand",
                table: "Products",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId_ProductCode",
                table: "Products",
                columns: new[] { "CompanyId", "ProductCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultSLASettingsId",
                table: "Products",
                column: "DefaultSLASettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ExternalProductId",
                table: "Products",
                column: "ExternalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_HSNCode",
                table: "Products",
                column: "HSNCode");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeatured",
                table: "Products",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsPublic",
                table: "Products",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Manufacturer",
                table: "Products",
                column: "Manufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ParentProductId",
                table: "Products",
                column: "ParentProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PartNumber",
                table: "Products",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ReplacementProductId",
                table: "Products",
                column: "ReplacementProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Type",
                table: "Products",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_CategoryId",
                table: "Warranties",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_CompanyId",
                table: "Warranties",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_CompanyId_Code",
                table: "Warranties",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_IsActive",
                table: "Warranties",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_IsDefault",
                table: "Warranties",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_ProductId",
                table: "Warranties",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_Type",
                table: "Warranties",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Assets_AssetId",
                table: "Complaints",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Contracts_ContractId",
                table: "Complaints",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_CustomerContacts_CustomerContactId",
                table: "Complaints",
                column: "CustomerContactId",
                principalTable: "CustomerContacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_CustomerLocations_CustomerLocationId",
                table: "Complaints",
                column: "CustomerLocationId",
                principalTable: "CustomerLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Customers_CustomerId",
                table: "Complaints",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Products_ProductId",
                table: "Complaints",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Assets_AssetId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Contracts_ContractId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_CustomerContacts_CustomerContactId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_CustomerLocations_CustomerLocationId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Customers_CustomerId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Products_ProductId",
                table: "Complaints");

            migrationBuilder.DropTable(
                name: "AssetServiceHistory");

            migrationBuilder.DropTable(
                name: "ContractItems");

            migrationBuilder.DropTable(
                name: "ContractRenewalHistory");

            migrationBuilder.DropTable(
                name: "CustomerContacts");

            migrationBuilder.DropTable(
                name: "LicenseModuleActivations");

            migrationBuilder.DropTable(
                name: "PartnerContacts");

            migrationBuilder.DropTable(
                name: "PartnerCustomers");

            migrationBuilder.DropTable(
                name: "ProductPriceLists");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Licenses");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "CustomerLocations");

            migrationBuilder.DropTable(
                name: "Warranties");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_AssetId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_ContractId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CustomerContactId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CustomerId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CustomerLocationId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_ProductId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CustomerContactId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CustomerFeedback",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CustomerLocationId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CustomerSatisfactionRating",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "IsUnderContract",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "IsUnderWarranty",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Complaints");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(8837), new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9122) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9480), new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9481) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9486), new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9486) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9491), new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9491) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9495), new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9496) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(681), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(687) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(715), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(716) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(719), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(719) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(722), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(722) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(725), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(725) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(729), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(729) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(732), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(732) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(735), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(735) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(738), new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(738) });
        }
    }
}
