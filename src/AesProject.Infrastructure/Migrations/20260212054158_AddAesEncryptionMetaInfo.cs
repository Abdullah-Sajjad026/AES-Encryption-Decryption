using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AesProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAesEncryptionMetaInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aes_encryption_meta_info",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    secret_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    iv = table.Column<byte[]>(type: "bytea", nullable: false),
                    tag_length = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aes_encryption_meta_info", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aes_encryption_meta_info");
        }
    }
}
