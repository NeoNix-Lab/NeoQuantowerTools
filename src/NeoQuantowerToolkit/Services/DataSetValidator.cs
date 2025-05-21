using Neo.Quantower.Abstractions.Interfaces;
using Neo.Quantower.Abstractions.Models;
using Neo.Quantower.Toolkit.Sqlite3Connections;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Services
{

    //📝 TODO: [Custom Logs]
    //📝 TODO: [Use Custom Async]
    //📝 TODO: [Handle db exepcions]

    public class DataSetValidator
    {
        private IJsonSettingsService _jsonSettingsService;
        private ISqliteDynamicAccessService _sqliteDynamicAccessService;

        public DataSetValidator(IJsonSettingsService jsonSettingsService, ISqliteDynamicAccessService sqliteDynamicAccessService)
        {
            _jsonSettingsService = jsonSettingsService;
            _sqliteDynamicAccessService = sqliteDynamicAccessService;
        }


        //🧠 HINT: [Attenzione alla comparazione troppo stringente]

        public async Task<bool> ValidateDataSet(Config config , IModelStruct modelStruct)
        {
            try
            {
                var path = _jsonSettingsService.GetValue(config.DataPath);

                var dataColums = await _sqliteDynamicAccessService.GetTableColumnsAsync(modelStruct.Name!);

                return modelStruct.Columns == dataColums.Select(x => x.Name).ToList() && modelStruct.Columns.Count == dataColums.Count;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }
    }
}
