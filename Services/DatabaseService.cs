using Microsoft.Maui.Storage;
using SQLite;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using TindaKo.Helpers;
using TindaKo.Models;
namespace TindaKo.Services
{


    public class DatabaseService
    {
        public static SQLiteAsyncConnection _db;

        public static async Task InitAsync()
        {
            if (_db != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tindako.db");
            _db = new SQLiteAsyncConnection(dbPath);
            // Create tables

            //-------Users------------//
            // Check if Users table is empty

        }
        public static async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_db == null)
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tindako.db");
                _db = new SQLiteAsyncConnection(dbPath);
            }
            await _db.CreateTableAsync<Product>();
            await _db.CreateTableAsync<Users>();
            await _db.CreateTableAsync<Sell>();
            await _db.CreateTableAsync<Transaction>();
            await _db.CreateTableAsync<Buyer>();
            await _db.CreateTableAsync<Creditor>();
            await _db.CreateTableAsync<IncomingStocks>();
            await _db.CreateTableAsync<IncomingStocksProductsList>();

            return _db;
        }

        public static async Task ResetDB()
        {
            await InitAsync();

            if (_db == null)
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tindako.db");
                _db = new SQLiteAsyncConnection(dbPath);
            }

            var tables = new List<string>
                {
                    nameof(Product),
                    nameof(Users),
                    nameof(Sell),
                    nameof(Transaction),
                    nameof(Buyer),
                    nameof(Creditor),
                    nameof(IncomingStocks),
                    nameof(IncomingStocksProductsList)
                };

            StringBuilder sb = new StringBuilder();

            foreach (var table in tables)
            {
                try
                {
                    switch (table)
                    {
                        case nameof(Product): await _db.DropTableAsync<Product>(); break;
                        case nameof(Users): await _db.DropTableAsync<Users>(); break;
                        case nameof(Sell): await _db.DropTableAsync<Sell>(); break;
                        case nameof(Transaction): await _db.DropTableAsync<Transaction>(); break;
                        case nameof(Buyer): await _db.DropTableAsync<Buyer>(); break;
                        case nameof(Creditor): await _db.DropTableAsync<Creditor>(); break;
                        case nameof(IncomingStocks): await _db.DropTableAsync<IncomingStocks>(); break;
                        case nameof(IncomingStocksProductsList): await _db.DropTableAsync<IncomingStocksProductsList>(); break;
                    }

                    sb.AppendLine($"Dropped {table} → ✅ Success");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Dropped {table} → ❌ Failed ({ex.Message})");
                }
            }

            await ToastHelper.ShowMessage(sb.ToString());
        }


    }
}
