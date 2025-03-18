using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeaTimeDemo.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class dbcontext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "DisplayOrder", "Name" },
                values: new object[,]
                {
                    { 4, 4, "能量特調" },
                    { 5, 5, "奶昔" },
                    { 6, 6, "手搖飲" },
                    { 7, 7, "健康飲品" },
                    { 8, 8, "養生茶" },
                    { 9, 9, "高級養生" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "DisplayOrder", "ImageUrl", "Name", "Price", "Size", "Stock" },
                values: new object[,]
                {
                    { 4, 4, "濃縮紅牛、濃縮咖啡、濃縮可樂加上鹽巴，保證讓血壓直衝雲霄！", 0, "", "高血壓特調", 99.0, "大杯", 0 },
                    { 5, 5, "純奶油、全脂牛奶、蛋黃、起司、炸培根混合而成，喝一口就像體檢報告爆表的感覺。", 0, "", "膽固醇奶昔", 120.0, "大杯", 0 },
                    { 6, 6, "超高糖、超高鹽、超高油，奶蓋厚到可以當被子蓋，珍珠浸泡在煉乳中，糖度100%起跳！", 0, "", "三高手搖茶", 85.0, "中杯", 0 },
                    { 7, 7, "新鮮羽衣甘藍、菠菜、酪梨、奇亞籽與燕麥奶混合，讓你的心跳穩定如心電圖。", 0, "", "心電圖綠拿鐵", 80.0, "大杯", 0 },
                    { 8, 8, "枸杞、紅棗、決明子與菊花泡製，專為熬夜加班族打造，讓肝臟回春！", 0, "", "護肝好茶", 65.0, "中杯", 0 },
                    { 9, 9, "燕窩、純牛奶、龍眼蜜，補充滿滿膠原蛋白，喝了讓皮膚發光，醫護人員最愛！", 0, "", "白袍燕窩牛奶", 150.0, "大杯", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
