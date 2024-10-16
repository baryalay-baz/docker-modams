using MODAMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public interface IDonorService
    {
        Task<Result<List<Donor>>> GetIndexAsync();
        Result<Donor> GetCreateDonor();
        Task<Result<Donor>> CreateDonorAsync(Donor dto);
        Task<Result<Donor>> GetEditDonorAsync(int donorId);
        Task<Result<Donor>> EditDonorAsync(Donor donor);

    }
}
