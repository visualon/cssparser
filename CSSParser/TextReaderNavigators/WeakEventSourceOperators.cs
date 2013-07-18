using System;
using System.Threading;

namespace CSSParser.TextReaderNavigators
{
	public static class WeakEventSourceThreadSafeOperations
	{
		public static void Fire<TEventArgs>(WeakEventSource<TEventArgs> eventSource, object sender, TEventArgs e) where TEventArgs : EventArgs
		{
			if (eventSource != null)
				eventSource.Fire(sender, e);
		}

		public static void Add<TEventArgs>(ref WeakEventSource<TEventArgs> eventSource, EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
		{
			WeakEventSource<TEventArgs> valueBeforeChangeAttempt, valueAtChangePoint;
			do
			{
				valueBeforeChangeAttempt = eventSource;
				var newValue = valueBeforeChangeAttempt + handler;

				// Compare _readAhead to valueBeforeChangeAttempt and change _readAhead's value to newValue if they match. Return the value of _readAhead
				// before the method call, regardless of whether its value was changed or not.
				// - If the returned valueAtChangePoint does not match the valueBeforeChangeAttempt then no change to _readAhead was performed (another
				//   thread must have made a change to it before the CompareExchange call) and so we must loop round to try again
				valueAtChangePoint = Interlocked.CompareExchange<WeakEventSource<TEventArgs>>(
					ref eventSource,
					newValue,
					valueBeforeChangeAttempt
				);
			}
			while (valueAtChangePoint != valueBeforeChangeAttempt);
		}

		public static void Remove<TEventArgs>(ref WeakEventSource<TEventArgs> eventSource, EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
		{
			WeakEventSource<TEventArgs> valueBeforeChangeAttempt, valueAtChangePoint;
			do
			{
				valueBeforeChangeAttempt = eventSource;
				var newValue = valueBeforeChangeAttempt - handler;
				valueAtChangePoint = Interlocked.CompareExchange<WeakEventSource<TEventArgs>>(
					ref eventSource,
					newValue,
					valueBeforeChangeAttempt
				);
			}
			while (valueAtChangePoint != valueBeforeChangeAttempt);
		}
	}
}
