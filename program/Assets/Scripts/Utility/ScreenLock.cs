using System;
using UnityEngine.EventSystems;

namespace Utility {
    public class ScreenLock : IDisposable {
        private static int ReferenceCount = 0;
        private static EventSystem currentSystem;
        
        public ScreenLock() {
            ReferenceCount++;
            if (EventSystem.current) {
                currentSystem = EventSystem.current;
                EventSystem.current.enabled = false;
            }
        }

        public void Dispose() {
            ReferenceCount--;
            if (ReferenceCount <= 0) currentSystem.enabled = true;
        }
    }
}