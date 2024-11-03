using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public class DonorService : IDonorService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ILogger<DonorService> _logger;

        private int _employeeId;

        public DonorService(ApplicationDbContext db, IAMSFunc func, ILogger<DonorService> logger)
        {
            _db = db;
            _func = func;
            _logger = logger;

            _employeeId = _func.GetEmployeeId();
        }

        public async Task<Result<List<Donor>>> GetIndexAsync()
        {
            try
            {
                var donors = await _db.Donors.ToListAsync();
                return Result<List<Donor>>.Success(donors);
            }
            catch (Exception ex) {
                _func.LogException(_logger, ex);
                return Result<List<Donor>>.Failure(ex.Message);
            } 
        }
        public Result<Donor> GetCreateDonor() {
            try
            {
                var dto = new Donor();
                return Result<Donor>.Success(dto);
            }
            catch (Exception ex) {
                _func.LogException(_logger, ex);
                return Result<Donor>.Failure(ex.Message);
            }
        }
        public async Task<Result<Donor>> CreateDonorAsync(Donor donor) {
            try
            {
                var donorInDb = await _db.Donors.Where(m => m.Code == donor.Code).FirstOrDefaultAsync();
                if (donorInDb != null)
                {
                    return Result<Donor>.Failure("Donor with this code already exists!");
                }

                await _db.Donors.AddAsync(donor);
                await _db.SaveChangesAsync();

                return Result<Donor>.Success(donor);
            }
            catch (Exception ex) {
                _func.LogException(_logger, ex);
                return Result<Donor>.Failure(ex.Message);
            }
        }
        public async Task<Result<Donor>> GetEditDonorAsync(int donorId) {
            try
            {
                var donor = await _db.Donors.Where(m => m.Id == donorId).FirstOrDefaultAsync();
                if (donor == null)
                    return Result<Donor>.Failure("Donor not found!");

                return Result<Donor>.Success(donor);
            }
            catch (Exception ex) {
                _func.LogException(_logger, ex);
                return Result<Donor>.Failure(ex.Message);
            }
        }
        public async Task<Result<Donor>> EditDonorAsync(Donor donor) {
            try
            {
                var donorInDb = await _db.Donors.Where(m => m.Id == donor.Id).FirstOrDefaultAsync();

                if (donorInDb == null)
                    return Result<Donor>.Failure("Donot not found!");

                donorInDb.Code = donor.Code;
                donorInDb.Name = donor.Name;
                await _db.SaveChangesAsync();

                return Result<Donor>.Success(donorInDb);

            }
            catch (Exception ex) {
                _func.LogException(_logger, ex);
                return Result<Donor>.Failure(ex.Message);
            }
        }
    }
}
