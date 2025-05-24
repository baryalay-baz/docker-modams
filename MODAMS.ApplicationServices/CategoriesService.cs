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

        private int _employeeId;

        public CategoriesService(ApplicationDbContext db, IAMSFunc func, ILogger<CategoriesService> logger)
        {
            _db = db;
            _func = func;
            _logger = logger;

            _employeeId = _func.GetEmployeeId();
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
                var categoryInDb = await _db.Categories.Where(m => m.CategoryName == dto.CategoryName).FirstOrDefaultAsync();
                if (categoryInDb != null)
                {
                    return Result<Category>.Failure("Category with this name already exists!");
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
                return Result<Category>.Failure($"Failed creating a category {ex.Message}");
            }
        }
        public async Task<Result<Category>> GetEditCategoryAsync(int categoryId)
        {
            try
            {
                var categoryInDb = await _db.Categories.FirstOrDefaultAsync(m => m.Id == categoryId);
                if (categoryInDb == null)
                {
                    return Result<Category>.Failure("Category not found!");
                }

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
                    return Result<Category>.Failure("Category not found!");
                }

                categoryInDb.CategoryCode = dto.CategoryCode;
                categoryInDb.CategoryName = dto.CategoryName;

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
        public async Task<Result<SubCategoryDTO>> CreateSubCategoryAsync(SubCategoryDTO dto)
        {
            try
            {
                var subCategoryInDb = await _db.SubCategories.Where(m => m.SubCategoryName == dto.SubCategoryName).FirstOrDefaultAsync();
                if (subCategoryInDb != null)
                {
                    return Result<SubCategoryDTO>.Failure("Sub-Category with this name already exists!");
                }

                dto.LifeSpan = (dto.LifeSpan < 1) ? 1 : dto.LifeSpan;

                SubCategory subCategory = new SubCategory()
                {
                    CategoryId = dto.CategoryId,
                    SubCategoryCode = dto.SubCategoryCode,
                    SubCategoryName = dto.SubCategoryName,
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
        public async Task<Result<SubCategory>> GetEditSubCategoryAsync(int subCategoryId)
        {
            try
            {
                var dto = await _db.SubCategories.Where(m => m.Id == subCategoryId).FirstOrDefaultAsync();

                if (dto == null)
                {
                    return Result<SubCategory>.Failure("Sub-Category not found!");
                }

                return Result<SubCategory>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<SubCategory>.Failure(ex.Message);
            }
        }
        public async Task<Result<SubCategory>> EditSubCategoryAsync(SubCategory dto)
        {
            try
            {
                var subCategoryInDb = await _db.SubCategories.Where(m => m.Id == dto.Id).FirstOrDefaultAsync();
                if (subCategoryInDb == null)
                {
                    return Result<SubCategory>.Failure("Sub-Category not found!");
                }

                subCategoryInDb.SubCategoryCode = dto.SubCategoryCode;
                subCategoryInDb.SubCategoryName = dto.SubCategoryName;
                subCategoryInDb.LifeSpan = dto.LifeSpan < 1 ? 1 : dto.LifeSpan;
                await _db.SaveChangesAsync();

                return Result<SubCategory>.Success(dto);
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
                return Result<SubCategory>.Failure(ex.Message);
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
