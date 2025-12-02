using System;
using System.Timers;

using TimersTimer = System.Timers.Timer;



namespace ReactCRM.Services

{

    /// <summary>

    /// Background service that runs periodic checks for notifications

    /// </summary>

    public class BackgroundNotificationService

    {

        private static BackgroundNotificationService _instance;

        private static readonly object _lock = new object();

        private TimersTimer dailyCheckTimer;

        private DateTime lastCheckDate;



        private BackgroundNotificationService()

        {

            lastCheckDate = DateTime.MinValue;

            StartDailyChecks();

        }



        public static BackgroundNotificationService Instance

        {

            get

            {

                if (_instance == null)

                {

                    lock (_lock)

                    {

                        if (_instance == null)

                        {

                            _instance = new BackgroundNotificationService();

                        }

                    }

                }

                return _instance;

            }

        }



        /// <summary>

        /// Starts the timer for daily checks (every hour, but only processes once per day)

        /// </summary>

        private void StartDailyChecks()

        {

            // Check every hour

            dailyCheckTimer = new TimersTimer(3600000); // 1 hour = 3,600,000 ms

            dailyCheckTimer.Elapsed += OnDailyCheckTimer;

            dailyCheckTimer.AutoReset = true;

            dailyCheckTimer.Start();



            // Run initial check

            RunDailyChecks();

        }



        private void OnDailyCheckTimer(object sender, ElapsedEventArgs e)

        {

            RunDailyChecks();

        }



        /// <summary>

        /// Runs daily notification checks (birthdays, overdue tasks)

        /// Only runs once per day

        /// </summary>

        private void RunDailyChecks()

        {

            try

            {

                var today = DateTime.Today;



                // Only run once per day

                if (lastCheckDate.Date >= today)

                {

                    return;

                }



                System.Diagnostics.Debug.WriteLine($"[BackgroundNotificationService] Running daily checks for {today:yyyy-MM-dd}");



                // Check for birthdays

                NotificationService.CheckBirthdayNotifications();



                // Check for overdue tasks

                NotificationService.CheckOverdueTasks();



                lastCheckDate = today;



                System.Diagnostics.Debug.WriteLine("[BackgroundNotificationService] Daily checks completed");

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"[BackgroundNotificationService] Error during daily checks: {ex.Message}");

            }

        }



        /// <summary>

        /// Forces an immediate run of all daily checks (for testing or manual trigger)

        /// </summary>

        public void ForceRunChecks()

        {

            lastCheckDate = DateTime.MinValue; // Reset so checks will run

            RunDailyChecks();
            
        }



        /// <summary>

        /// Stops the background service

        /// </summary>

        public void Stop()

        {

            dailyCheckTimer?.Stop();

            dailyCheckTimer?.Dispose();

        }

    }

}