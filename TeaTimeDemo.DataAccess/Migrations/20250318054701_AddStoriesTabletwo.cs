using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeaTimeDemo.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStoriesTabletwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Stories",
                columns: new[] { "Id", "Content", "CreatedDate", "ImageUrl", "IsFeatured", "ProductId", "StoryType", "Title" },
                values: new object[,]
                {
                    { 1, "1950年代，年輕時的張阿嬤在鄉間小路上偶遇了一位老茶農。這位老茶農將祖傳的製茶秘方傾囊相授，從此改變了張阿嬤的一生。\n\n每天清晨，張阿嬤都會親自挑選最新鮮的茶葉，用古老的木製工具細心處理，堅持傳統工藝，不添加任何人工添加物。\n\n「好茶需要耐心等待，就像人生一樣，不能急躁。」這是老奶奶常說的一句話，也是她六十多年來堅守的信念。", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/stories/grandma-tea.jpg", true, null, 0, "老奶奶的製茶故事" },
                    { 2, "古厝烏龍茶的故事要從老奶奶的祖厝說起。那是一棟有著百年歷史的紅磚古厝，庭院裡種著幾棵烏龍茶樹。\n\n在三代以前，老奶奶的祖父將這些茶樹從高山帶回，種在家門前。每當春天來臨，全家人都會聚在一起採茶、製茶，那時的笑聲至今仍迴盪在老奶奶的記憶中。\n\n「我們的古厝烏龍茶不僅僅是一種飲品，更是家族的記憶和智慧的結晶。」老奶奶說道，「每一口都能品嚐到祖先留下的味道。」", new DateTime(2025, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/stories/oolong-tea.jpg", true, 2, 1, "古厝烏龍茶的由來" },
                    { 3, "在這個快節奏的時代，人們越來越懷念過去那種慢生活的氛圍。一杯老奶奶的懷舊茶飲，不僅能夠滿足味蕾，更能撫慰浮躁的心靈。\n\n研究表明，傳統工藝製作的茶飲含有豐富的抗氧化物質，對健康大有裨益。而且，慢慢品茗的過程本身就是一種修行，能夠幫助現代人找回內心的平靜。\n\n下次當你感到壓力山大時，不妨泡一杯老奶奶的茶，靜靜品味，讓時間慢下來，感受傳統智慧帶來的寧靜與喜悅。", new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/stories/modern-life.jpg", false, null, 2, "懷舊茶飲與現代生活" },
                    { 4, "「阿嬤的果園鮮飲」的獨特風味來自於老奶奶童年時期家中果園的記憶。\n\n在老奶奶小時候，家裡有一片小果園，種著各種台灣本土水果。夏天的午後，阿嬤總會摘下最新鮮的水果，搭配當地山泉水製作成消暑飲品。\n\n「我們現在的配方還保留了60年前的原汁原味，」老奶奶驕傲地說，「就連榨汁的力道和順序都有講究，這是無法用機器取代的。」\n\n每一杯「阿嬤的果園鮮飲」都承載著老奶奶兒時的回憶和對品質的堅持。", new DateTime(2025, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/stories/fruit-drink.jpg", false, 1, 1, "阿嬤的果園鮮飲秘密" },
                    { 5, "茶，是台灣人生活中不可或缺的一部分。從早期的茶葉貿易到現在的手搖飲料店，茶文化一直在演變，但不變的是人們對茶的熱愛。\n\n老奶奶年輕時曾在傳統茶行工作，親眼目睹了台灣茶文化的變遷。她常說：「現在的年輕人喝的飲料多了糖分和添加物，少了茶最純粹的韻味。」\n\n正是這樣的堅持，讓老奶奶決定開設「老奶奶的懷舊時光」，將傳統的茶飲方式帶回現代人的生活，讓更多人能夠品嚐到真正的台灣茶香。", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/stories/tea-memory.jpg", true, null, 0, "老時光中的茶香記憶" },
                    { 6, "老時光手沖黑咖啡的故事要追溯到1960年代，當時老奶奶在台北市的一家小咖啡館學習烘焙和手沖技藝。\n\n「那時候的咖啡豆都是從國外運來的，非常珍貴，一點都不能浪費。」老奶奶回憶道，「我的師父教導我，每一顆咖啡豆都有自己的個性，要用不同的溫度和方式去對待它們。」\n\n多年來，老奶奶一直保持著這種對咖啡的敬意。她堅持使用傳統的方式手工烘焙咖啡豆，再以精確的水溫和時間沖泡，確保每一杯咖啡都能展現出最完美的風味。\n\n「現代人生活節奏太快，連喝咖啡都變成了一種例行公事。」老奶奶說，「但在我這裡，咖啡是一種藝術，一種生活態度，值得我們慢下來細細品味。」", new DateTime(2025, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/stories/hand-drip-coffee.jpg", true, 3, 1, "老時光手沖黑咖啡的故事" },
                    { 7, "台灣的四季分明，使得茶葉在不同季節有著截然不同的風味特色。春茶清新甘甜，夏茶濃郁厚實，秋茶香氣馥郁，冬茶醇厚耐泡。\n\n老奶奶深知這個道理，因此她會根據季節變化調整茶的選擇和沖泡方式。「要尊重自然的節奏，」她常說，「飲茶也是如此。」\n\n在「老奶奶的懷舊時光」，顧客可以品嚐到最應季的茶飲，感受大自然的奧妙與茶葉的生命力。這是一種傳統智慧，也是老奶奶對生活的理解。", new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/stories/tea-seasons.jpg", false, null, 2, "茶葉的四季變化" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Stories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Stories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Stories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Stories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Stories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Stories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Stories",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
