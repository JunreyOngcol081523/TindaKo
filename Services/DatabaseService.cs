using Microsoft.Maui.Storage;
using SQLite;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

                await _db.CreateTableAsync<Product>();
                await _db.CreateTableAsync<Users>();
                await _db.CreateTableAsync<Sell>();
                await _db.CreateTableAsync<Transaction>();
                await _db.CreateTableAsync<Buyer>();
                await _db.CreateTableAsync<Creditor>();
                await _db.CreateTableAsync<IncomingStocks>();
                await _db.CreateTableAsync<IncomingStocksProductsList>();
            }
            return _db;
        }
        public static async Task ResetDB()
        {
            if (_db == null)
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tindako.db");
                _db = new SQLiteAsyncConnection(dbPath);

                await _db.DropTableAsync<Product>();
                await _db.DropTableAsync<Users>();
                await _db.DropTableAsync<Sell>();
                await _db.DropTableAsync<Transaction>();
                await _db.DropTableAsync<Buyer>();
                await _db.DropTableAsync<Creditor>();
                await _db.DropTableAsync<IncomingStocks>();
                await _db.DropTableAsync<IncomingStocksProductsList>();
            }
        }
    }
}
