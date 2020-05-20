using System;
using System.Threading;

namespace MultiMinesweeper
{
    public class TimerManager
    {
        private Timer _timer;
        private AutoResetEvent _autoResetEvent;
        private Action _action;
 
        public DateTime TimerStarted { get; }
 
        public TimerManager(Action action)
        {
            _action = action;
            _autoResetEvent = new AutoResetEvent(false);
            _timer = new Timer(Execute, _autoResetEvent, 0, 20000);
            TimerStarted = DateTime.Now;
        }
 
        public void Execute(object stateInfo)
        {
            _action();
 
            if((DateTime.Now - TimerStarted).Seconds > 20)
            {
                _timer.Dispose();
            }
        }
    }
}