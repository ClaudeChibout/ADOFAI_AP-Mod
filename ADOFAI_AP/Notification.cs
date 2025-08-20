using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ADOFAI_AP
{
    internal class Notification : MonoBehaviour
    {
        public static Notification Instance;
        private string text;
        
        static readonly Dictionary<int, string> notifications = new Dictionary<int, string>();

        internal int notificationCount = 0;
        static int firstNotification = 0; // This will be used to track the first notification for removal

        /*public Notification(string NotificationText)
        {
            text = NotificationText;
            id = notificationCount++; // Assign a unique ID to this notification starting from 0
            notifications[id] = this; // Store the notification in the dictionary
            Task.Delay(5000).ContinueWith(_ => Remove()); // Automatically remove the notification after 5 seconds
        }*/

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ADOFAI_AP.Instance.mls.LogError("NOTIFACTION_AP is initialized!");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                // Example of how to create a notification
                ADOFAI_AP.Instance.mls.LogInfo($"Creating the test notification  N°{Notification.Instance.notificationCount}.");
                ADOFAI_AP.Instance.mls.LogInfo($"Size: {Screen.width}, {Screen.height}");
                CreateNotification($"This is the test notification N°{Notification.Instance.notificationCount}! ");
            }
        }

        void OnGUI()
        {
            // Draw the notifications on the screen
            Draw();
        }



        void Draw()
        {   

            float xDivisor = 4f; // Divisor for the width of the notification box
            float yDivisor = 14f; // Divisor for the height of the notification box

            float x = Screen.width - Screen.width / xDivisor;
            float y = Screen.height - (Screen.height / yDivisor) * (notificationCount - firstNotification);
            float width = Screen.width / xDivisor;
            float height = ( Screen.height / yDivisor) * (notificationCount - firstNotification);
            if ((notificationCount - firstNotification) > 0) GUI.Box(new Rect(x, y, width, height), $"Notification ({(notificationCount - firstNotification)})");
            for (int i = firstNotification; i < notificationCount; i++)
            {   
                // Get the notification text from the dictionary
                text = notifications[i];
                // Draw the notification on the screen
                var yOffset = (i - firstNotification) * (Screen.height / yDivisor);
                GUI.Label(new Rect(x + 10, y + yOffset + ((Screen.height / yDivisor) / 3), width - 20, height - 20), text);
            }

        }

        public void CreateNotification(string NotificationText)
        {
            // Create a new notification with the given text
            var id = notificationCount++;
            notifications[id] = NotificationText; // Store the notification in the dictionary
            Task.Delay(5000).ContinueWith(_ => Remove(id)); // Automatically remove the notification after 5 seconds
        }

        void Remove(int id)
        {
            // Remove the notification from the dictionary
            notifications.Remove(id);
            firstNotification++;
        }

    }
}
