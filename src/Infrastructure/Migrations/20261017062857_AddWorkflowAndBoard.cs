using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowAndBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    OwnerProjectId = table.Column<int>(nullable: true),
                    IsSystem = table.Column<bool>(nullable: false),
                    IsDraft = table.Column<bool>(nullable: false),
                    Version = table.Column<int>(nullable: false),
                    PayloadJson = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    lastModified = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    IsDelete = table.Column<bool>(nullable: false)
                }, constraints: table => table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id));
            migrationBuilder.CreateTable(
                name: "ProjectWorkflowBoardSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(nullable: false),
                    WorkflowDefinitionRecordId = table.Column<int>(nullable: true),
                    KanbanBacklogEnabled = table.Column<bool>(nullable: false),
                    BoardColumnsJson = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    lastModified = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    IsDelete = table.Column<bool>(nullable: false)
                }, constraints: table => table.PrimaryKey("PK_ProjectWorkflowBoardSettings", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_ProjectWorkflowBoardSettings_ProjectId", table: "ProjectWorkflowBoardSettings", column: "ProjectId", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProjectWorkflowBoardSettings");
            migrationBuilder.DropTable(name: "WorkflowDefinitions");
        }
    }
}
