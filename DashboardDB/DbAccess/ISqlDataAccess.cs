﻿using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Serilog;
using System.ComponentModel;
using System.Data.SqlClient;

namespace DBLibrary.DbAccess
{
    public interface ISqlDataAccess
    {
        Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionId = "Default");
        Task SaveData<T>(string storedProcedure, T parameters, string connectionId = "Default");

        Task<IEnumerable<T>> ExecuteQuery<T>(string query, T parameters, string connectionId);

        static async Task<T> ExecuteSqlTaskWithRetry<T>(Task<T> executeTask)
        {
            IEnumerable<TimeSpan>? delay = Backoff.AwsDecorrelatedJitterBackoff(TimeSpan.FromMilliseconds(50),
                TimeSpan.FromMilliseconds(150), 
                5,
                fastFirst: true);

            AsyncRetryPolicy retryPolicy = Policy
                .Handle<SqlException>(SqlServerTransientExceptionDetector.ShouldRetryOn)
                .Or<TimeoutException>()
                .OrInner<Win32Exception>(SqlServerTransientExceptionDetector.ShouldRetryOn)
                .WaitAndRetryAsync(delay, (exception, span, context) =>
                                          {
                                              Log.Logger.Error(exception, "Sql exception thrown, retrying...");
                                          });
            
            
            PolicyResult<T> policyResult = await retryPolicy.ExecuteAndCaptureAsync(
                                               async () =>
                                               {
                                                   await Task.WhenAll(executeTask);
                                                   return executeTask.Result;
                                               });

            return policyResult.Result;
        }
    }
}