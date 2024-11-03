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
        Task<Result<SubCategoryDTO>> CreateSubCategoryAsync(SubCategoryDTO dto);
        Task<Result<SubCategory>> GetEditSubCategoryAsync(int subCategoryId);
        Task<Result<SubCategory>> EditSubCategoryAsync(SubCategory dto);
        Task<string> GetSubCategoriesAsync(string categoryName);
    }
}
