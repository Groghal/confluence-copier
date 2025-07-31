using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using NUnit.Framework;

namespace ConfluenceCopierTests
{
    /// <summary>
    /// Base class for headless Avalonia UI tests
    /// Provides proper Avalonia initialization for testing without a GUI
    /// </summary>
    [TestFixture]
    public abstract class HeadlessTestBase
    {
        private static bool _avaloniaInitialized = false;
        private static readonly object _initLock = new object();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            InitializeAvalonia();
        }

        /// <summary>
        /// Initialize Avalonia for headless testing
        /// This only needs to be done once per test run
        /// </summary>
        private static void InitializeAvalonia()
        {
            lock (_initLock)
            {
                if (_avaloniaInitialized)
                    return;

                try
                {
                    AppBuilder.Configure<ConfluenceCopier.App>()
                        .UseHeadless(new AvaloniaHeadlessPlatformOptions
                        {
                            UseHeadlessDrawing = true
                        })
                        .UseSkia()
                        .SetupWithoutStarting();

                    _avaloniaInitialized = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize Avalonia for testing: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Execute an action on the UI thread safely
        /// </summary>
        protected static T RunOnUIThread<T>(Func<T> action)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                return action();
            }

            T result = default(T)!;
            Dispatcher.UIThread.InvokeAsync(() => result = action()).Wait();
            return result;
        }

        /// <summary>
        /// Execute an action on the UI thread safely
        /// </summary>
        protected static void RunOnUIThread(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                action();
                return;
            }

            Dispatcher.UIThread.InvokeAsync(action).Wait();
        }

        /// <summary>
        /// Create a test window safely on the UI thread
        /// </summary>
        protected static T CreateTestWindow<T>() where T : Window, new()
        {
            return RunOnUIThread(() => new T());
        }
    }

    /// <summary>
    /// Helper class for creating testable versions of application components
    /// without full UI initialization
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Create a mock MainWindow for testing without UI dependencies
        /// </summary>
        public static MockMainWindow CreateMockMainWindow()
        {
            return new MockMainWindow();
        }
    }

    /// <summary>
    /// Lightweight mock version of MainWindow for testing
    /// Avoids UI creation but provides access to business logic
    /// </summary>
    public class MockMainWindow
    {
        public ConfluenceCopier.AppSettings Settings { get; set; } = new ConfluenceCopier.AppSettings();
        
        // Expose methods that need testing without UI dependency
        public string ExtractPageId(string input)
        {
            // Use reflection to access the public instance method from the real MainWindow
            var mainWindowType = typeof(ConfluenceCopier.MainWindow);
            var method = mainWindowType.GetMethod("ExtractPageId", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                try
                {
                    // Create a minimal MainWindow instance for the method call
                    var mainWindow = new ConfluenceCopier.MainWindow();
                    return (string)method.Invoke(mainWindow, new object[] { input })!;
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    // Unwrap the inner exception for proper test assertions
                    throw ex.InnerException ?? ex;
                }
            }
            
            throw new NotImplementedException("ExtractPageId method not found");
        }

        // Mock rate limiting properties
        public DateTime LastRequestTime { get; set; } = DateTime.MinValue;
        public bool IsRateLimited => DateTime.Now.Subtract(LastRequestTime).TotalMilliseconds < 1000;

        public void ApplyRateLimit()
        {
            var timeSinceLastRequest = DateTime.Now.Subtract(LastRequestTime);
            if (timeSinceLastRequest.TotalMilliseconds < 1000)
            {
                var delayNeeded = 1000 - (int)timeSinceLastRequest.TotalMilliseconds;
                if (delayNeeded > 0)
                {
                    System.Threading.Thread.Sleep(delayNeeded);
                }
            }
            LastRequestTime = DateTime.Now;
        }
    }
}