using SQLite;

namespace TindaKo.Models
{
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int CategoryId { get; set; }
        [Unique, Collation("NOCASE")]
        public string CategoryName { get; set; } = string.Empty;
    }
    // Query result for the category list in Product Settings.
    // NOT a database table: don't call CreateTableAsync<CategoryInfo>().
    public class CategoryInfo
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }

        [Ignore]
        public string ProductCountText =>
            ProductCount == 1 ? "1 produkto" : $"{ProductCount} produkto";
    }
}
