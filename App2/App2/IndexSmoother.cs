using Microsoft.Gestures.Skeleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Gestures.Samples.Camera3D
{
    public class SmoothedPositionChangeEventArgs : EventArgs
    {
        /// <summary> Position of palm after jitter was smoothed. </summary>
        public Vector3 SmoothedPosition { get; set; }
        /// <summary> Difference between current smoothed position and previous one. </summary>
        public Vector3 SmoothedPositionDelta { get; set; }
    }

    // Note - I borrowed this class from the PalmSmoother class in the Gestures samples
    // simply making it work off the index finger position rather than the palm position.
    // It might be a case for using something like ReactiveExtensions rather than writing
    // this whole class here but it was available so I borrowed it.
    public class IndexSmoother
    {
        const int DefaultWindowSize = 5; // [samples]
        const float DefaultJumpThreshold = 20; // [mm]        

        private readonly int _windowSize;
        private readonly float _jumpThreshold;

        private Queue<Vector3> _window;
        private Vector3 CurrentAverage => _window.Any() ? _window.Aggregate((v1, v2) => v1 + v2) * (1f / _window.Count) : new Vector3(0, 0, 0);

        public event EventHandler<SmoothedPositionChangeEventArgs> SmoothedPositionChanged;

        public IndexSmoother(int windowSize = DefaultWindowSize, float jumpThreshold = DefaultJumpThreshold)
        {
            _window = new Queue<Vector3>(windowSize);
            _windowSize = windowSize;
            _jumpThreshold = jumpThreshold;
        }

        public void Smooth(IHandSkeleton skeleton)
        {
            var indexPosition = skeleton.IndexPosition;
            var previousAverage = CurrentAverage;

            if ((indexPosition - previousAverage).TwoNorm() > _jumpThreshold)
            {
                // filter jump - flush the queue and start over averaging
                _window.Clear();
                _window.Enqueue(indexPosition);
                return;
            }

            _window.Enqueue(indexPosition);
            if (_window.Count > _windowSize)
            {
                _window.Dequeue();
            }

            var currentAverage = CurrentAverage;

            SmoothedPositionChanged?.Invoke(this, new SmoothedPositionChangeEventArgs()
            {
                SmoothedPosition = currentAverage,
                SmoothedPositionDelta = currentAverage - previousAverage
            });
        }

    }
}
