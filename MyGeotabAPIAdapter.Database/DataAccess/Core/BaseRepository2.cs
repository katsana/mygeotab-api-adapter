﻿using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyGeotabAPIAdapter.Database.DataAccess
{
    /// <summary>
    /// A generic base repository class that handles database CRUD operations.
    /// </summary>
    /// <typeparam name="T">The type of entity to be used for representation of database records and for which CRUD operations are to be performed.</typeparam>
    public class BaseRepository2<T> : IBaseRepository2<T>, IDisposable where T : class
    {
        const long NullLongValue = -1;

        /// <inheritdoc/>
        public DateTime MaxDateTimeForDatabaseParameters { get => new(2100, 1, 1); }

        /// <inheritdoc/>
        public DateTime MinDateTimeForDatabaseParameters { get => new(1900, 1, 1); }

        readonly IConnectionContext context;

        public BaseRepository2(IConnectionContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Disposes the current <see cref="BaseRepository{T}"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Add any clean-up code here.
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(T entity, CancellationTokenSource methodCancellationTokenSource)
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            bool result;
            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                var connection = context.GetConnection();
                var transaction = context.GetTransaction();
                result = await connection.DeleteAsync<T>(entity, transaction);
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            methodCancellationToken.ThrowIfCancellationRequested();
            if (result != false)
            {
                return result;
            }
            throw new Exception($"The subject {typeof(T).Name} entity was not deleted.");
        }

        /// <summary>
        /// Executes the stored procedure identified by <paramref name="storedProcedureName"/>, supplying the parameter values included in <paramref name="parameters"/> and returns an <see cref="IEnumerable{T}"/> with the results (or <c>null</c> if no results are returned by the stored procedure). This method must be executed by an inheriting class that is able to receive specific parameter values for <typeparamref name="T"/> and build them into an anonymous type to be supplied as <paramref name="parameters"/>. 
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to be executed.</param>
        /// <param name="parameters">The parameter values to be supplied as an anonymous type to the stored procedure.</param>
        /// <param name="methodCancellationTokenSource">The <see cref="CancellationTokenSource"/>.</param>
        /// <returns></returns>
        protected async Task<IEnumerable<T>> ExecuteStoredProcedureQueryAsync(string storedProcedureName, object parameters, CancellationTokenSource methodCancellationTokenSource)
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            IEnumerable<T> records;
            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                var connection = context.GetConnection();
                var transaction = context.GetTransaction();
                records = await connection.QueryAsync<T>(storedProcedureName, parameters, transaction, commandType: CommandType.StoredProcedure);
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            methodCancellationToken.ThrowIfCancellationRequested();
            return records;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync(CancellationTokenSource methodCancellationTokenSource, int? resultsLimit = null, DateTime? changedSince = null, string sortColumnName = "")
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            IEnumerable<T> records;
            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                var connection = context.GetConnection();
                var transaction = context.GetTransaction();
                records = await connection.GetAllAsync<T>(transaction, context.TimeoutSecondsForDatabaseTasks, resultsLimit, changedSince, sortColumnName);
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            methodCancellationToken.ThrowIfCancellationRequested();
            if (records != null)
            {
                return records;
            }
            throw new Exception($"GetAllAsync<...resultsLimit> method failed to return a result for entity type '{typeof(T).Name}'.");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAsync(dynamic dynamicParams, CancellationTokenSource methodCancellationTokenSource, int? resultsLimit = null, DateTime? changedSince = null, string sortColumnName = "")
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            IEnumerable<T> records;
            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                var connection = context.GetConnection();
                var transaction = context.GetTransaction();
                records = await connection.GetByParamAsync<T>(dynamicParams as object, transaction, null, resultsLimit, changedSince, sortColumnName);
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            methodCancellationToken.ThrowIfCancellationRequested();
            if (records != null)
            {
                return records;
            }
            throw new Exception($"No {typeof(T).Name} entities matching the specified search parameters were found.");
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync(int Id, CancellationTokenSource methodCancellationTokenSource)
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            T record;
            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                var connection = context.GetConnection();
                var transaction = context.GetTransaction();
                record = await connection.GetAsync<T>(Id, transaction);
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            methodCancellationToken.ThrowIfCancellationRequested();
            // TODO: Determine whether to throw exception or just return null.

            //if (record != null)
            //{
            //    return record;
            //}
            //throw new Exception($"{typeof(T).Name} with ID of '{Id}' not found.");
            return record;
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync(string Id, CancellationTokenSource methodCancellationTokenSource)
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            T record;
            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                var connection = context.GetConnection();
                var transaction = context.GetTransaction();
                record = await connection.GetAsync<T>(Id, transaction);
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            methodCancellationToken.ThrowIfCancellationRequested();
            // TODO: Determine whether to throw exception or just return null.

            //if (record != null)
            //{
            //    return record;
            //}
            //throw new Exception($"{typeof(T).Name} with ID of '{Id}' not found.");
            return record;
        }

        /// <inheritdoc/>
        public async Task<long> InsertAsync(T entity, CancellationTokenSource methodCancellationTokenSource)
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            long result;
            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                var connection = context.GetConnection();
                var transaction = context.GetTransaction();
                var uowTransactionId = context.GetUnitOfWorkId();
                result = await connection.InsertAsync(entity, transaction, context.TimeoutSecondsForDatabaseTasks);
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            methodCancellationToken.ThrowIfCancellationRequested();
            if (result != NullLongValue)
            {
                return result;
            }
            throw new Exception($"The subject {typeof(T).Name} entity was not inserted.");
        }

        /// <inheritdoc/>
        public async Task<long> InsertAsync(IEnumerable<T> entities, CancellationTokenSource methodCancellationTokenSource)
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            var insertedEntityCount = 0;

            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                foreach (var entity in entities)
                {
                    await InsertAsync(entity, methodCancellationTokenSource);
                    insertedEntityCount += 1;
                    methodCancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            return insertedEntityCount;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(T entity, CancellationTokenSource methodCancellationTokenSource)
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            bool result;
            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                var connection = context.GetConnection();
                var transaction = context.GetTransaction();
                var uowTransactionId = context.GetUnitOfWorkId();
                result = await connection.UpdateAsync<T>(entity, transaction, context.TimeoutSecondsForDatabaseTasks);
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            methodCancellationToken.ThrowIfCancellationRequested();
            if (result != false)
            {
                return result;
            }
            throw new Exception($"The subject {typeof(T).Name} entity was not updated.");
        }

        /// <inheritdoc/>
        public async Task<long> UpdateAsync(IEnumerable<T> entities, CancellationTokenSource methodCancellationTokenSource)
        {
            CancellationToken methodCancellationToken = methodCancellationTokenSource.Token;

            var updateedEntityCount = 0;

            try
            {
                CancellationTokenSource timeoutCancellationTokenSource = new();
                timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(context.TimeoutSecondsForDatabaseTasks));

                foreach (var entity in entities)
                {
                    await UpdateAsync(entity, methodCancellationTokenSource);
                    updateedEntityCount += 1;
                    methodCancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException exception)
            {
                throw new DatabaseConnectionException($"Database operation did not complete within the allowed time of {context.TimeoutSecondsForDatabaseTasks} seconds.", exception);
            }
            catch (Exception exception)
            {
                throw new DatabaseConnectionException($"Exception encountered while attempting database operation.", exception);
            }

            return updateedEntityCount;
        }
    }
}
