using Dapper;
using System.Data;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;
using Microsoft.Extensions.Logging;

namespace core8_rest_azure_service_bus.Services
{
    public interface ISaleService {
        Task<List<Sale>> SalesList();        
    }

    public class SaleService: ISaleService {

        public readonly IDbConnection _dbConnection;
        public readonly ILogger<SaleService> _logger;

        public SaleService(IDbConnection dbConnection, ILogger<SaleService> logger) {
            _dbConnection = dbConnection;
            _logger = logger;
        }


        public async Task<List<Sale>> SalesList() {
            const string sql = @"SELECT id, salesamount, salesamount FROM sales";
            var salesData = await _dbConnection.QueryAsync<Sale>(sql);

            var salesList = salesData.ToList();
            if (!salesList.Any())
            {
                _logger.LogWarning("Sales data not found.");
                throw new AppException("Sales data not found.");
            }

            return salesList;
        }
    }
    
}