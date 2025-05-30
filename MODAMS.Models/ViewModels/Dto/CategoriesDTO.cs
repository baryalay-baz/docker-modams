
namespace MODAMS.Models.ViewModels.Dto
{
    public class CategoriesDTO
    {
        public int? SelectedCategoryId { get; set; }

        public List<Category> categories { get; set; } = new List<Category>();
        public List<SubCategory> subCategories { get; set; } = new List<SubCategory>();
    }
}
