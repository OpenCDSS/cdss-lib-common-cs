using System.Runtime.CompilerServices;

namespace RTi.Util.GUI
{
    public class EventTimer
    {

        private bool _isDone = false;

        /**
        Stop generating events.  This synchronized method can be called by a different
        thread (presumably the one that created the EventTimer.  The thread is
        stopped the next time the thread is checked (at an even interval).
        */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void finish()
        {
            _isDone = true;
        }
    }
}
