using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Infrastructure.Persistence.Context;

#nullable disable

namespace Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261017062930_AddWorkflowAuditColumns")]
public partial class AddWorkflowAuditColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "lastModifiedBy", table: "WorkflowDefinitions", nullable: true);
        migrationBuilder.AddColumn<string>(name: "lastModifiedBy", table: "ProjectWorkflowBoardSettings", nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "lastModifiedBy", table: "WorkflowDefinitions");
        migrationBuilder.DropColumn(name: "lastModifiedBy", table: "ProjectWorkflowBoardSettings");
    }
}
