using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabat.Reposatory.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // تم مسح الـ RenameColumn لأن العمود متعدل بالفعل في قاعدة البيانات
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // تم مسح العكس أيضاً
        }
    }
}