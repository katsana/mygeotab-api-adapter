﻿using MyGeotabAPIAdapter.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyGeotabAPIAdapter.Database.DataAccess
{
    /// <summary>
    /// A repository class that handles database CRUD operations for <see cref="DbCondition"/> entities.
    /// </summary>
    public class DbConditionRepository : BaseRepository<DbCondition>
    {
        /// <summary>
        /// Deletes a number of <see cref="DbCondition"/> entities into the database within a single transaction.
        /// </summary>
        /// <param name="connectionInfo">The database connection information.</param>
        /// <param name="dbConditions">A list of <see cref="DbCondition"/> entities to be deleted.</param>
        /// <param name="cancellationTokenSource">The <see cref="CancellationTokenSource"/>.</param>
        /// <param name="commandTimeout">The number of seconds before command execution timeout.</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(ConnectionInfo connectionInfo, IEnumerable<DbCondition> dbConditions, CancellationTokenSource cancellationTokenSource, int commandTimeout)
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            long deletedRowsCount = 0;
            try
            {
                using (var connection = await new ConnectionProvider(connectionInfo).GetOpenConnectionAsync())
                {
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        foreach (var dbCondition in dbConditions)
                        {
                            await DeleteAsync(connection, transaction, dbCondition, commandTimeout);
                            deletedRowsCount += 1;
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        await transaction.CommitAsync();
                    }
                    return deletedRowsCount;
                }
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }
        }

        /// <summary>
        /// Retrieves a <see cref="DbCondition"/> with the specified <see cref="DbCondition.GeotabId"/>. Throws an <see cref="Exception"/> if an entity with the specified ID cannot be found.
        /// </summary>
        /// <param name="connectionInfo">The database connection information.</param>
        /// <param name="id">The ID of the database record to be returned.</param>
        /// <param name="cancellationTokenSource">The <see cref="CancellationTokenSource"/>.</param>
        /// <param name="commandTimeout">The number of seconds before command execution timeout.</param>
        /// 
        /// <returns></returns>
        public async Task<DbCondition> GetAsync(ConnectionInfo connectionInfo, string id, CancellationTokenSource cancellationTokenSource, int commandTimeout)
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            var record = await GetAsync(connectionInfo, new { Id = id }, commandTimeout);
            cancellationToken.ThrowIfCancellationRequested();
            if (record.Any())
            {
                return record.FirstOrDefault();
            }
            throw new Exception($"{typeof(DbCondition).Name} with Id '{id}' not found.");
        }

        /// <summary>
        /// Inserts a number of <see cref="DbCondition"/> entities into the database within a single transaction.
        /// </summary>
        /// <param name="connectionInfo">The database connection information.</param>
        /// <param name="dbConditions">A list of <see cref="DbCondition"/> entities to be inserted.</param>
        /// <param name="cancellationTokenSource">The <see cref="CancellationTokenSource"/>.</param>
        /// <param name="commandTimeout">The number of seconds before command execution timeout.</param>
        /// <returns></returns>
        public async Task<long> InsertAsync(ConnectionInfo connectionInfo, IEnumerable<DbCondition> dbConditions, CancellationTokenSource cancellationTokenSource, int commandTimeout)
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            long insertedRowsCount = 0;
            try
            {
                using (var connection = await new ConnectionProvider(connectionInfo).GetOpenConnectionAsync())
                {
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        foreach (var dbCondition in dbConditions)
                        {
                            await InsertAsync(connection, transaction, dbCondition, commandTimeout);
                            insertedRowsCount += 1;
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        await transaction.CommitAsync();
                    }
                    return insertedRowsCount;
                }
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }
        }
    }
}
