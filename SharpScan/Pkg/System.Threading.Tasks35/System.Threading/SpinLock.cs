using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace System.Threading
{
    [DebuggerDisplay("IsHeld = {IsHeld}")]
    public struct SpinLock
    {
        private readonly bool _disableThreadTracking;
        private int _isHeld;
        private Thread _ownerThread;

        public SpinLock(bool enableThreadOwnerTracking)
        {
            _disableThreadTracking = !enableThreadOwnerTracking;
            _ownerThread = null;
            _isHeld = 0;
        }

        public bool IsHeld
        {
            get
            {
                return Thread.VolatileRead(ref _isHeld) == 1;
            }
        }

        public bool IsHeldByCurrentThread
        {
            get
            {
                if (_disableThreadTracking)
                {
                    throw new InvalidOperationException("Thread ownership tracking is disabled");
                }
                else
                {
                    return IsHeld && ReferenceEquals(_ownerThread, Thread.CurrentThread);
                }
            }
        }

        public bool IsThreadOwnerTrackingEnabled
        {
            get
            {
                return _disableThreadTracking;
            }
        }

        public void Enter(ref bool lockTaken)
        {
            if (lockTaken)
            {
                lockTaken = false;
                throw new ArgumentException();
            }
            else
            {
                if (_disableThreadTracking)
                {
                    var check = Interlocked.CompareExchange(ref _isHeld, 1, 0);
                    if (check == 0)
                    {
                        lockTaken = true;
                    }
                    else
                    {
                        //Deadlock on recursion
                        TryEnter(-1, ref lockTaken);
                    }
                }
                else
                {
                    if (IsHeldByCurrentThread)
                    {
                        //Throw on recursion
                        throw new LockRecursionException();
                    }
                    else
                    {
                        if (Interlocked.CompareExchange(ref _isHeld, 1, 0) == 0 && ReferenceEquals(Interlocked.CompareExchange(ref _ownerThread, Thread.CurrentThread, null), null))
                        {
                            lockTaken = true;
                        }
                        else
                        {
                            TryEnter(-1, ref lockTaken);
                        }
                    }
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Exit()
        {
            Exit(true);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Exit(bool useMemoryBarrier)
        {
            if (_disableThreadTracking)
            {
                //Allow corruption: There is no check for what thread this is being called from
                ExitExtracted(useMemoryBarrier);
            }
            else
            {
                if (IsHeldByCurrentThread)
                {
                    ExitExtracted(useMemoryBarrier);
                }
                else
                {
                    throw new SynchronizationLockException();
                }
            }
        }

        public void TryEnter(ref bool lockTaken)
        {
            if (lockTaken)
            {
                lockTaken = false;
                throw new ArgumentException();
            }
            TryEnter(0, ref lockTaken);
        }

        public void TryEnter(TimeSpan timeout, ref bool lockTaken)
        {
            TryEnter((int)timeout.TotalMilliseconds, ref lockTaken);
        }

        public void TryEnter(int millisecondsTimeout, ref bool lockTaken)
        {
            if (_disableThreadTracking)
            {
                if (ThreadingHelper.SpinWaitSet(ref _isHeld, 1, 0, millisecondsTimeout))
                {
                    lockTaken = true;
                }
            }
            else
            {
                if (IsHeldByCurrentThread)
                {
                    //Throw on recursion
                    throw new LockRecursionException();
                }
                else
                {
                    if (ThreadingHelper.SpinWaitSet(ref _isHeld, 1, 0, millisecondsTimeout) && ReferenceEquals(Interlocked.CompareExchange(ref _ownerThread, Thread.CurrentThread, null), null))
                    {
                        lockTaken = true;
                    }
                }
            }
        }

        private void ExitExtracted(bool useMemoryBarrier)
        {
            if (useMemoryBarrier)
            {
                Thread.VolatileWrite(ref _isHeld, 0);
                ThreadingHelper.VolatileWrite(ref _ownerThread, null);
            }
            else
            {
                _isHeld = 0;
                _ownerThread = null;
            }
        }
    }
}
