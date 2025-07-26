

using UnityEngine;
using UnityEngine.Playables;

namespace SnowRabbit.Timeline
{
    [ExecuteAlways]
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelineHelper : MonoBehaviour, INotificationReceiver
    {
        public PlayableDirector director
        {
            get
            {
                if (_director == null)
                {
                    _director = GetComponent<PlayableDirector>();
                }
                return _director;
            }
        }
        private PlayableDirector _director;


        void Awake()
        {
            if (director == null)
            {
                Debug.LogError("PlayableDirector is not found", this);
                return;
            }

        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            // if (notification is IngameBubbleMarker marker)
            // {
            //     Debug.Log("OnNotify: " + marker.dialog);
            // }
        }
    }
}