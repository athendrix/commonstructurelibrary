using CSL.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSL.SQL
{
    /// <summary>
    /// Uses Sqlite to provide a local Key/Value store and logging functionality.
    /// </summary>
    public class ProtectedDB :IDisposable
    {
        private readonly LocalDB innerDB;
        private readonly IProtector protector;
        /// <summary>
        /// Creates a new ProtectedDB at the filepath.
        /// </summary>
        /// <param name="filepath">The full filepath to create or open the LocalDB.</param>
        public ProtectedDB(LocalDB innerDB, IProtector protector)
        {
            this.innerDB = innerDB;
            this.protector = protector;
        }

        public void Dispose()
        {
            ((IDisposable)innerDB).Dispose();
            ((IDisposable)protector).Dispose();
        }

        public async Task<string?> Get(string key)
        {
            string? toReturn = await innerDB.Get(key);
            if (toReturn != null)
            {
                return await protector.Unprotect(toReturn, key);
            }
            return null;
        }

        public async Task Set(string key, string? value)
        {
            string? toInsert = value;
            if (toInsert == null)
            {
                await innerDB.Set(key, null);
            }
            else
            {
                toInsert = await protector.Protect(toInsert, key);
                await innerDB.Set(key, toInsert);
            }
        }
    }
}
