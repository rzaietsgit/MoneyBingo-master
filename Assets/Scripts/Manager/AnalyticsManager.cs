using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using UnityEngine;

namespace ITSoft
{
    public class AnalyticsManager : MonoBehaviour
    {
        //[RuntimeInitializeOnLoadMethod]
        private void Awake()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if(dependencyStatus == DependencyStatus.Available)
                {
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        public static void Log(string message)
        {
            FirebaseAnalytics.LogEvent(message);
        }

    }
}
