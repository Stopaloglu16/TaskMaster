using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ServiceLayer.Models.Diagnostic;

    public class TaskManagerDiagnostics
    {

        /// <summary>
        /// Request Counts: Total number of forecast requests made
        //Request Duration: How long forecast requests take to complete
        //Error Rates: Number of failed requests
        //Cache Performance: Cache hit and miss rates
        //Distributed Tracing: Activity source for following requests
        /// </summary>
        private static readonly Meter meter = new Meter("TaskManagerMetrics", "1.0");
        public static readonly Counter<int> taskRequestCounter = meter.CreateCounter<int>("task_requests_total", "Total number of task requests");
        public static readonly Histogram<double> taskRequestDuration = meter.CreateHistogram<double>("task_request_duration_seconds", "Histogram of task request durations");
        public static readonly Counter<int> failedRequestCounter = meter.CreateCounter<int>("failed_requests_total", "Total number of failed requests");
        //public static readonly Counter<int> cacheHitCounter = meter.CreateCounter<int>("cache_hits_total", "Total number of cache hits");
        //public static readonly Counter<int> cacheMissCounter = meter.CreateCounter<int>("cache_misses_total", "Total number of cache misses");
        public static readonly ActivitySource activitySource = new ActivitySource("TaskManager");
    }

