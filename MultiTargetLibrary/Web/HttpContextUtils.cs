using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;

#if NETFRAMEWORK
using System.Web;
using System.Web.SessionState;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace MultiTargetLibrary.Web
{
    /// <summary>
    /// Cross-framework HTTP context utilities that work with both .NET Framework and .NET 8
    /// </summary>
    public static class HttpContextUtils
    {
        /// <summary>
        /// Get the current HTTP context in a framework-agnostic way
        /// </summary>
        public static object? GetCurrentHttpContext()
        {
#if NETFRAMEWORK
            return HttpContext.Current;
#else
            // In .NET 8, HttpContext is typically accessed through dependency injection
            // This method provides a fallback, but DI is the recommended approach
            return null; // Should be injected via IHttpContextAccessor
#endif
        }

        /// <summary>
        /// Get a session value in a framework-agnostic way
        /// </summary>
        public static T? GetSessionValue<T>(string key, object? httpContext = null)
        {
#if NETFRAMEWORK
            var context = (HttpContext?)httpContext ?? HttpContext.Current;
            if (context?.Session?[key] != null)
            {
                return (T)context.Session[key];
            }
            return default(T);
#else
            var context = httpContext as HttpContext;
            if (context?.Session != null && context.Session.Keys.Contains(key))
            {
                var value = context.Session.GetString(key);
                if (value != null && typeof(T) == typeof(string))
                {
                    return (T)(object)value;
                }
                // For complex types, you'd need to implement JSON serialization/deserialization
            }
            return default(T);
#endif
        }

        /// <summary>
        /// Set a session value in a framework-agnostic way
        /// </summary>
        public static void SetSessionValue<T>(string key, T value, object? httpContext = null)
        {
#if NETFRAMEWORK
            var context = (HttpContext?)httpContext ?? HttpContext.Current;
            if (context?.Session != null)
            {
                context.Session[key] = value;
            }
#else
            var context = httpContext as HttpContext;
            if (context?.Session != null)
            {
                if (typeof(T) == typeof(string))
                {
                    context.Session.SetString(key, value?.ToString() ?? string.Empty);
                }
                // For complex types, you'd need to implement JSON serialization
            }
#endif
        }

        /// <summary>
        /// Get request URL in a framework-agnostic way
        /// </summary>
        public static string? GetRequestUrl(object? httpContext = null)
        {
#if NETFRAMEWORK
            var context = (HttpContext?)httpContext ?? HttpContext.Current;
            return context?.Request?.Url?.ToString();
#else
            var context = httpContext as HttpContext;
            if (context?.Request != null)
            {
                return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            }
            return null;
#endif
        }

        /// <summary>
        /// Get user agent in a framework-agnostic way
        /// </summary>
        public static string? GetUserAgent(object? httpContext = null)
        {
#if NETFRAMEWORK
            var context = (HttpContext?)httpContext ?? HttpContext.Current;
            return context?.Request?.UserAgent;
#else
            var context = httpContext as HttpContext;
            return context?.Request?.Headers["User-Agent"].ToString();
#endif
        }

        /// <summary>
        /// Get client IP address in a framework-agnostic way
        /// </summary>
        public static string? GetClientIPAddress(object? httpContext = null)
        {
#if NETFRAMEWORK
            var context = (HttpContext?)httpContext ?? HttpContext.Current;
            if (context?.Request != null)
            {
                return context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? 
                       context.Request.ServerVariables["REMOTE_ADDR"];
            }
            return null;
#else
            var context = httpContext as HttpContext;
            if (context?.Request != null)
            {
                return context.Request.Headers["X-Forwarded-For"].ToString() ?? 
                       context.Connection.RemoteIpAddress?.ToString();
            }
            return null;
#endif
        }

        /// <summary>
        /// Check if request is HTTPS in a framework-agnostic way
        /// </summary>
        public static bool IsHttps(object? httpContext = null)
        {
#if NETFRAMEWORK
            var context = (HttpContext?)httpContext ?? HttpContext.Current;
            return context?.Request?.IsSecureConnection ?? false;
#else
            var context = httpContext as HttpContext;
            return context?.Request?.IsHttps ?? false;
#endif
        }

        /// <summary>
        /// Get HTTP method in a framework-agnostic way
        /// </summary>
        public static string? GetHttpMethod(object? httpContext = null)
        {
#if NETFRAMEWORK
            var context = (HttpContext?)httpContext ?? HttpContext.Current;
            return context?.Request?.HttpMethod;
#else
            var context = httpContext as HttpContext;
            return context?.Request?.Method;
#endif
        }
    }
}
