using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices.IServices
{
    public interface ICategoriesService
    {
        Task<Result<CategoriesDTO>> GetIndexAsync(int? categoryId);
        Task<Result<Category>> CreateCategoryAsync(Category dto);
        Task<Result<Category>> GetEditCategoryAsync(int categoryId);
        Task<Result<Category>> EditCategoryAsync(Category dto);
        Task<Result<SubCategoryDTO>> GetCreateSubCategoryAsync(int categoryId);
        Task<Result<SubCategoryDTO>> CreateSubCategoryAsync(SubCategoryDTO dto);
        Task<Result<SubCategoryDTO>> GetEditSubCategoryAsync(int subCategoryId);
        Task<Result<SubCategoryDTO>> EditSubCategoryAsync(SubCategoryDTO dto);
        Task<List<SubCategory>> GetSubCategoriesAsync(string categoryName);
    }
}
