using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CommandSystem.Utilities
{
    /// <summary>
    /// Provides utility methods for asynchronous operations and runtime method information.
    /// </summary>
    public static class PatternExtensions
    {
        /// <summary>
        /// Waits asynchronously until the specified condition is met or a timeout occurs.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="cancellationToken">Token to cancel the waiting operation.</param>
        /// <param name="checkIntervalMilliseconds">The interval (in milliseconds) to recheck the condition.</param>
        /// <param name="timeoutMilliseconds">Optional timeout (in milliseconds). A value of -1 means no timeout.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown if the timeout is reached before the condition is met.</exception>
        public static async Task WaitUntil(Func<bool> condition, CancellationToken cancellationToken = default, int checkIntervalMilliseconds = 100, int timeoutMilliseconds = -1)
        {
            int elapsed = 0;

            while (!condition())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                if (timeoutMilliseconds > 0)
                {
                    elapsed += checkIntervalMilliseconds;
                    if (elapsed >= timeoutMilliseconds)
                    {
                        throw new TimeoutException("WaitUntil timed out.");
                    }
                }

                await Task.Delay(checkIntervalMilliseconds, cancellationToken);
            }
        }

        /// <summary>
        /// Retrieves the method name of an IEnumerator object used in coroutines.
        /// </summary>
        /// <param name="enumerator">The IEnumerator instance representing the coroutine.</param>
        /// <returns>
        /// The name of the method associated with the coroutine, or "Unknown" if the method cannot be determined.
        /// </returns>
        public static string GetEnumeratorMethodName(IEnumerator enumerator)
        {
            if (enumerator == null) return "Unknown";

            var type = enumerator.GetType();
            var methodInfo = type.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (methodInfo != null)
            {
                var declaringType = methodInfo.DeclaringType;
                if (declaringType != null)
                {
                    return declaringType.Name.Split('>')[0].TrimStart('<');
                }
            }
            return "Unknown";
        }
    }
}