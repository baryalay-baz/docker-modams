using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public class CategoriesService : ICategoriesService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ILogger<CategoriesService> _logger;

        private readonly bool _isSomali;
        public CategoriesService(ApplicationDbContext db, IAMSFunc func, ILogger<CategoriesService> logger)
        {
            _db = db;
            _func = func;
            _logger = logger;

            _isSomali = CultureInfo.CurrentCulture.Name == "so";
        }

        public async Task<Result<CategoriesDTO>> GetIndexAsync(int? categoryId)
        {
            try
            {
                var categories = await _db.Categories.ToListAsync();
                List<SubCategory> subcategories = new List<SubCategory>();
                var dto = new CategoriesDTO();
                dto.categories = categories;

                if (categoryId != null)
                {
                    subcategories = await _db.SubCategories.Where(m => m.CategoryId == categoryId).ToListAsync();
                    dto.subCategories = subcategories;
                    dto.SelectedCategoryId = categoryId;
                }
                else
                {
                    dto.subCategories = subcategories;
                }
                return Result<CategoriesDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Error: {ex.Message} | Inner Exception: {ex.InnerException.Message}");
                }
                else
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
                return Result<CategoriesDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<Category>> CreateCategoryAsync(Category dto)
        {
            try
            {
                var categoryInDb = await _db.Categories.Where(m => m.CategoryName == dto.CategoryName)
                    .FirstOrDefaultAsync();
                if (categoryInDb != null)
                {
                    var message = _isSomali
                        ? $"Qeybta leh magaca '{dto.CategoryNameSo}' hore ayaa loo abuuray!"
                        : $"Category with name '{dto.CategoryName}' already exists!";

                    return Result<Category>.Failure(message);
                }

                await _db.Categories.AddAsync(dto);
                await _db.SaveChangesAsync();

                return Result<Category>.Success(dto);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Error: {ex.Message} | Inner Exception: {ex.InnerException.Message}");
                }
                else
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
                var message = _isSomali
                    ? $"Waa lagu guuldareystay in la abuuro qeybta {ex.Message}"
                    : $"Failed creating a category {ex.Message}";
                return Result<Category>.Failure(message);
            }
        }
        public async Task<Result<Category>> GetEditCategoryAsync(int categoryId)
        {
            try
            {
                var categoryInDb = await _db.Categories.FirstOrDefaultAsync(m => m.Id == categoryId);
                if (categoryInDb == null)
                    return Result<Category>.Failure(_isSomali ? "Qeyb lama helin!" : "Category not found!");

                return Result<Category>.Success(categoryInDb);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Error: {ex.Message} | Inner Exception: {ex.InnerException.Message}");
                }
                else
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
                return Result<Category>.Failure($"Failed loading category: {ex.Message}");
            }
        }
        public async Task<Result<Category>> EditCategoryAsync(Category dto)
        {
            try
            {
                var categoryInDb = await _db.Categories.Where(m => m.Id == dto.Id).FirstOrDefaultAsync();
                if (categoryInDb == null)
                {
                    return Result<Category>.Failure(_isSomali ? "Qeyb lama helin!" : "Category not found!");
                }

                categoryInDb.CategoryCode = dto.CategoryCode;
                categoryInDb.CategoryName = dto.CategoryName;
                categoryInDb.CategoryNameSo = dto.CategoryNameSo;

                await _db.SaveChangesAsync();
                return Result<Category>.Success(dto);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Error: {ex.Message} | Inner Exception: {ex.InnerException.Message}");
                }
                else
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
                return Result<Category>.Failure(ex.Message);
            }

        }
        public async Task<Result<SubCategoryDTO>> GetCreateSubCategoryAsync(int categoryId)
        {
            try
            {
                var category = await _db.Categories.Where(m => m.Id == categoryId).FirstOrDefaultAsync();
                if (category == null)
                {
                    return Result<SubCategoryDTO>.Failure(_isSomali ? "Qeyb lama helin!" : "Category not found!");
                }
                var dto = new SubCategoryDTO()
                {
                    CategoryId = categoryId,
                    CategoryCode = category.CategoryCode,
                    CategoryName = category.CategoryName,
                };
                return Result<SubCategoryDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<SubCategoryDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<SubCategoryDTO>> CreateSubCategoryAsync(SubCategoryDTO dto)
        {
            try
            {
                var subCategoryInDb = await _db.SubCategories.Where(m => m.SubCategoryName == dto.SubCategoryName).FirstOrDefaultAsync();
                if (subCategoryInDb != null)
                {
                    return Result<SubCategoryDTO>.Failure(_isSomali
                        ? "Hoosaag leh magacaas hore ayaa loo abuuray!"
                        : "Sub-Category with this name already exists!");
                }

                dto.LifeSpan = (dto.LifeSpan < 1) ? 1 : dto.LifeSpan;

                SubCategory subCategory = new SubCategory()
                {
                    CategoryId = dto.CategoryId,
                    SubCategoryCode = dto.SubCategoryCode,
                    SubCategoryName = dto.SubCategoryName,
                    SubCategoryNameSo = dto.SubCategoryNameSo,
                    LifeSpan = dto.LifeSpan
                };
                await _db.SubCategories.AddAsync(subCategory);
                await _db.SaveChangesAsync();

                return Result<SubCategoryDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Error: {ex.Message} | Inner Exception: {ex.InnerException.Message}");
                }
                else
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
                return Result<SubCategoryDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<SubCategoryDTO>> GetEditSubCategoryAsync(int subCategoryId)
        {
            try
            {
                var subCategoryInDb = await _db.SubCategories
                    .Include(m => m.Category)
                    .Where(m => m.Id == subCategoryId).FirstOrDefaultAsync();

                if (subCategoryInDb == null)
                    return Result<SubCategoryDTO>.Failure(_isSomali ? "Hoosaag lama helin!" : "Sub-Category not found!");

                var dto = new SubCategoryDTO()
                {
                    Id = subCategoryInDb.Id,
                    CategoryId = subCategoryInDb.CategoryId,
                    SubCategoryCode = subCategoryInDb.SubCategoryCode,
                    CategoryCode = subCategoryInDb.Category.CategoryCode,
                    CategoryName = subCategoryInDb.Category.CategoryName,
                    SubCategoryName = subCategoryInDb.SubCategoryName,
                    SubCategoryNameSo = subCategoryInDb.SubCategoryNameSo,
                    LifeSpan = subCategoryInDb.LifeSpan
                };

                return Result<SubCategoryDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<SubCategoryDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<SubCategoryDTO>> EditSubCategoryAsync(SubCategoryDTO dto)
        {
            try
            {
                var subCategoryInDb = await _db.SubCategories.Where(m => m.Id == dto.Id).FirstOrDefaultAsync();
                if (subCategoryInDb == null)
                {
                    return Result<SubCategoryDTO>.Failure(_isSomali ? "Hoosaag lama helin!" : "Sub-Category not found!");
                }

                subCategoryInDb.SubCategoryCode = dto.SubCategoryCode;
                subCategoryInDb.SubCategoryName = dto.SubCategoryName;
                subCategoryInDb.SubCategoryNameSo = dto.SubCategoryNameSo;
                subCategoryInDb.LifeSpan = dto.LifeSpan < 1 ? 1 : dto.LifeSpan;
                await _db.SaveChangesAsync();

                return Result<SubCategoryDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Error: {ex.Message} | Inner Exception: {ex.InnerException.Message}");
                }
                else
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
                return Result<SubCategoryDTO>.Failure(ex.Message);
            }
        }
        public async Task<List<SubCategory>> GetSubCategoriesAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return new List<SubCategory>();

            if (categoryName == "All")
            {
                return await _db.SubCategories.ToListAsync();
            }

            var categoryId = await _db.Categories
                .Where(c => c.CategoryName == categoryName)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (categoryId == 0)
                return new List<SubCategory>();

            return await _db.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}
