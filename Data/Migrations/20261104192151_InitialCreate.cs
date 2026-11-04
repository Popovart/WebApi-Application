using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImgUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Products",
                columns: new[] { "Id", "Name", "ImgUri", "Price", "Description" },
                values: new object[,]
                {
                    {
                        new Guid("00000000-0000-0000-0000-000000000001"),
                        "Laptop",
                        "https://placehold.co/600x400?text=Laptop",
                        24999.90m,
                        "15-inch laptop with 16 GB of RAM and a 512 GB SSD."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000002"),
                        "Smartphone",
                        "https://placehold.co/600x400?text=Smartphone",
                        12999.00m,
                        "Smartphone with a 6.1-inch display and 128 GB of storage."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000003"),
                        "Wireless mouse",
                        "https://placehold.co/600x400?text=Wireless+Mouse",
                        399.90m,
                        "Compact wireless mouse with an adjustable sensor."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000004"),
                        "Mechanical keyboard",
                        "https://placehold.co/600x400?text=Mechanical+Keyboard",
                        1299.00m,
                        "Mechanical keyboard with a numeric keypad."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000005"),
                        "USB-C hub",
                        "https://placehold.co/600x400?text=USB-C+Hub",
                        749.50m,
                        "USB-C hub with HDMI, USB-A and Ethernet ports."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000006"),
                        "Laptop stand",
                        "https://placehold.co/600x400?text=Laptop+Stand",
                        599.00m,
                        null
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000007"),
                        "Webcam",
                        "https://placehold.co/600x400?text=Webcam",
                        1899.90m,
                        "Full HD webcam with a built-in microphone."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000008"),
                        "Bluetooth speaker",
                        "https://placehold.co/600x400?text=Bluetooth+Speaker",
                        999.00m,
                        "Portable speaker with Bluetooth connectivity."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000009"),
                        "Wireless headphones",
                        "https://placehold.co/600x400?text=Wireless+Headphones",
                        2499.00m,
                        "Over-ear wireless headphones with noise cancellation."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000010"),
                        "27-inch monitor",
                        "https://placehold.co/600x400?text=27-inch+Monitor",
                        5999.00m,
                        "27-inch QHD monitor with an adjustable stand."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000011"),
                        "Portable SSD",
                        "https://placehold.co/600x400?text=Portable+SSD",
                        2199.90m,
                        "Portable 1 TB solid-state drive with USB-C connectivity."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000012"),
                        "USB flash drive",
                        "https://placehold.co/600x400?text=USB+Flash+Drive",
                        249.00m,
                        null
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000013"),
                        "Ethernet cable",
                        "https://placehold.co/600x400?text=Ethernet+Cable",
                        129.50m,
                        "Two-metre Cat 6 Ethernet cable."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000014"),
                        "HDMI cable",
                        "https://placehold.co/600x400?text=HDMI+Cable",
                        199.00m,
                        "Two-metre HDMI cable supporting 4K video."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000015"),
                        "Power bank",
                        "https://placehold.co/600x400?text=Power+Bank",
                        899.90m,
                        "Power bank with a capacity of 20000 mAh."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000016"),
                        "USB-C charger",
                        "https://placehold.co/600x400?text=USB-C+Charger",
                        699.00m,
                        "Compact 65 W USB-C charger."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000017"),
                        "Smartwatch",
                        "https://placehold.co/600x400?text=Smartwatch",
                        3499.00m,
                        "Smartwatch with activity tracking and notifications."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000018"),
                        "Wi-Fi router",
                        "https://placehold.co/600x400?text=Wi-Fi+Router",
                        1799.00m,
                        "Dual-band Wi-Fi 6 router with four Ethernet ports."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000019"),
                        "Laptop backpack",
                        "https://placehold.co/600x400?text=Laptop+Backpack",
                        1199.90m,
                        "Backpack with a padded compartment for a 15-inch laptop."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000020"),
                        "Mouse pad",
                        "https://placehold.co/600x400?text=Mouse+Pad",
                        299.00m,
                        null
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000021"),
                        "Desk lamp",
                        "https://placehold.co/600x400?text=Desk+Lamp",
                        799.00m,
                        "LED desk lamp with adjustable brightness."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000022"),
                        "USB microphone",
                        "https://placehold.co/600x400?text=USB+Microphone",
                        1599.90m,
                        "USB microphone with a desktop stand and mute button."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000023"),
                        "External hard drive",
                        "https://placehold.co/600x400?text=External+Hard+Drive",
                        2399.00m,
                        "External 2 TB hard drive for backups and file storage."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000024"),
                        "Tablet",
                        "https://placehold.co/600x400?text=Tablet",
                        8999.00m,
                        "10-inch tablet with 128 GB of storage."
                    },
                    {
                        new Guid("00000000-0000-0000-0000-000000000025"),
                        "E-book reader",
                        "https://placehold.co/600x400?text=E-book+Reader",
                        3299.90m,
                        null
                    }
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products",
                schema: "dbo");
        }
    }
}
