using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FMDC.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameImage",
                columns: table => new
                {
                    GameImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    ImageRelativePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameImage", x => x.GameImageId);
                });

            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false),
                    CardImageId = table.Column<int>(type: "int", nullable: false),
                    CardDescriptionImageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardType = table.Column<int>(type: "int", nullable: false),
                    MonsterType = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstGuardianStar = table.Column<int>(type: "int", nullable: true),
                    SecondGuardianStar = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: true),
                    AttackPoints = table.Column<int>(type: "int", nullable: true),
                    DefensePoints = table.Column<int>(type: "int", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StarchipCost = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.CardId);
                    table.ForeignKey(
                        name: "FK_Card_GameImage_CardDescriptionImageId",
                        column: x => x.CardDescriptionImageId,
                        principalTable: "GameImage",
                        principalColumn: "GameImageId");
                    table.ForeignKey(
                        name: "FK_Card_GameImage_CardImageId",
                        column: x => x.CardImageId,
                        principalTable: "GameImage",
                        principalColumn: "GameImageId");
                });

            migrationBuilder.CreateTable(
                name: "Character",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    CharacterImageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Biography = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character", x => x.CharacterId);
                    table.ForeignKey(
                        name: "FK_Character_GameImage_CharacterImageId",
                        column: x => x.CharacterImageId,
                        principalTable: "GameImage",
                        principalColumn: "GameImageId");
                });

            migrationBuilder.CreateTable(
                name: "Equippable",
                columns: table => new
                {
                    EquippableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetCardId = table.Column<int>(type: "int", nullable: false),
                    EquipCardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equippable", x => x.EquippableId);
                    table.ForeignKey(
                        name: "FK_Equippable_Card_EquipCardId",
                        column: x => x.EquipCardId,
                        principalTable: "Card",
                        principalColumn: "CardId");
                    table.ForeignKey(
                        name: "FK_Equippable_Card_TargetCardId",
                        column: x => x.TargetCardId,
                        principalTable: "Card",
                        principalColumn: "CardId");
                });

            migrationBuilder.CreateTable(
                name: "Fusion",
                columns: table => new
                {
                    FusionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FusionType = table.Column<int>(type: "int", nullable: false),
                    TargetCardId = table.Column<int>(type: "int", nullable: true),
                    TargetMonsterType = table.Column<int>(type: "int", nullable: true),
                    FusionMaterialCardId = table.Column<int>(type: "int", nullable: true),
                    FusionMaterialMonsterType = table.Column<int>(type: "int", nullable: true),
                    ResultantCardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fusion", x => x.FusionId);
                    table.ForeignKey(
                        name: "FK_Fusion_Card_FusionMaterialCardId",
                        column: x => x.FusionMaterialCardId,
                        principalTable: "Card",
                        principalColumn: "CardId");
                    table.ForeignKey(
                        name: "FK_Fusion_Card_ResultantCardId",
                        column: x => x.ResultantCardId,
                        principalTable: "Card",
                        principalColumn: "CardId");
                    table.ForeignKey(
                        name: "FK_Fusion_Card_TargetCardId",
                        column: x => x.TargetCardId,
                        principalTable: "Card",
                        principalColumn: "CardId");
                });

            migrationBuilder.CreateTable(
                name: "SecondaryType",
                columns: table => new
                {
                    SecondaryTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    MonsterType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecondaryType", x => x.SecondaryTypeId);
                    table.ForeignKey(
                        name: "FK_SecondaryType_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "CardId");
                });

            migrationBuilder.CreateTable(
                name: "CardPercentage",
                columns: table => new
                {
                    CardPercentageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    PercentageType = table.Column<int>(type: "int", nullable: false),
                    GenerationPercentage = table.Column<double>(type: "float", nullable: false),
                    GenerationRatePer2048 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardPercentage", x => x.CardPercentageId);
                    table.ForeignKey(
                        name: "FK_CardPercentage_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "CardId");
                    table.ForeignKey(
                        name: "FK_CardPercentage_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "CharacterId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Card_CardDescriptionImageId",
                table: "Card",
                column: "CardDescriptionImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Card_CardImageId",
                table: "Card",
                column: "CardImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardPercentage_CardId",
                table: "CardPercentage",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardPercentage_CharacterId",
                table: "CardPercentage",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_CharacterImageId",
                table: "Character",
                column: "CharacterImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equippable_EquipCardId",
                table: "Equippable",
                column: "EquipCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Equippable_TargetCardId",
                table: "Equippable",
                column: "TargetCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Fusion_FusionMaterialCardId",
                table: "Fusion",
                column: "FusionMaterialCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Fusion_ResultantCardId",
                table: "Fusion",
                column: "ResultantCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Fusion_TargetCardId",
                table: "Fusion",
                column: "TargetCardId");

            migrationBuilder.CreateIndex(
                name: "IX_SecondaryType_CardId",
                table: "SecondaryType",
                column: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardPercentage");

            migrationBuilder.DropTable(
                name: "Equippable");

            migrationBuilder.DropTable(
                name: "Fusion");

            migrationBuilder.DropTable(
                name: "SecondaryType");

            migrationBuilder.DropTable(
                name: "Character");

            migrationBuilder.DropTable(
                name: "Card");

            migrationBuilder.DropTable(
                name: "GameImage");
        }
    }
}
