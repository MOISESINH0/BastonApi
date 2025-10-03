using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Baston.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddConfianzaAndUserTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "dispositivo",
                columns: table => new
                {
                    dispositivo_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    direccion_bt = table.Column<string>(type: "text", nullable: false),
                    modelo = table.Column<string>(type: "text", nullable: true),
                    firmware = table.Column<string>(type: "text", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispositivo", x => x.dispositivo_id);
                });

            migrationBuilder.CreateTable(
                name: "evento_alerta",
                columns: table => new
                {
                    evento_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dispositivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ts_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    detalle = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    insertado_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evento_alerta", x => x.evento_id);
                });

            migrationBuilder.CreateTable(
                name: "kv_meta_servidor",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    k = table.Column<string>(type: "text", nullable: false),
                    v = table.Column<string>(type: "text", nullable: false),
                    actualizado_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kv_meta_servidor", x => new { x.usuario_id, x.k });
                });

            migrationBuilder.CreateTable(
                name: "posicion_gps",
                columns: table => new
                {
                    posicion_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dispositivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ts_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lat = table.Column<double>(type: "double precision", nullable: false),
                    lon = table.Column<double>(type: "double precision", nullable: false),
                    alt_m = table.Column<double>(type: "double precision", nullable: true),
                    insertado_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posicion_gps", x => x.posicion_id);
                    table.CheckConstraint("ck_pos_lat", "lat BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_pos_lon", "lon BETWEEN -180 AND 180");
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "text", nullable: false),
                    nombre_completo = table.Column<string>(type: "text", nullable: false),
                    contrasena_hash = table.Column<string>(type: "text", nullable: false),
                    rol = table.Column<string>(type: "text", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tag = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.usuario_id);
                });

            migrationBuilder.CreateTable(
                name: "confianza_request",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    remitente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receptor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enviado_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_confianza_request", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_confianza_request_usuario_receptor_id",
                        column: x => x.receptor_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_confianza_request_usuario_remitente_id",
                        column: x => x.remitente_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_confianza_request_receptor_id",
                table: "confianza_request",
                column: "receptor_id");

            migrationBuilder.CreateIndex(
                name: "IX_confianza_request_remitente_id",
                table: "confianza_request",
                column: "remitente_id");

            migrationBuilder.CreateIndex(
                name: "u_dispositivo_usuario_bt",
                table: "dispositivo",
                columns: new[] { "usuario_id", "direccion_bt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_evento_ts",
                table: "evento_alerta",
                column: "ts_utc");

            migrationBuilder.CreateIndex(
                name: "u_evento_dispositivo_ts_tipo",
                table: "evento_alerta",
                columns: new[] { "dispositivo_id", "ts_utc", "tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_posicion_ts",
                table: "posicion_gps",
                column: "ts_utc");

            migrationBuilder.CreateIndex(
                name: "u_posicion_dispositivo_ts",
                table: "posicion_gps",
                columns: new[] { "dispositivo_id", "ts_utc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_tag",
                table: "usuario",
                column: "tag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "confianza_request");

            migrationBuilder.DropTable(
                name: "dispositivo");

            migrationBuilder.DropTable(
                name: "evento_alerta");

            migrationBuilder.DropTable(
                name: "kv_meta_servidor");

            migrationBuilder.DropTable(
                name: "posicion_gps");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
