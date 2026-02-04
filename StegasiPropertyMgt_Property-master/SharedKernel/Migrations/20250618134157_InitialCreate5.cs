using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedKernel.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "approvalworkflow");

            migrationBuilder.RenameTable(
                name: "ApprovalWorkflows",
                newName: "ApprovalWorkflows",
                newSchema: "approvalworkflow");

            migrationBuilder.RenameTable(
                name: "ApprovalStages",
                newName: "ApprovalStages",
                newSchema: "approvalworkflow");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ApprovalWorkflows",
                schema: "approvalworkflow",
                newName: "ApprovalWorkflows");

            migrationBuilder.RenameTable(
                name: "ApprovalStages",
                schema: "approvalworkflow",
                newName: "ApprovalStages");
        }
    }
}
