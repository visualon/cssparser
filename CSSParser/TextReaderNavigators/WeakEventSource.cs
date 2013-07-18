using System;
using System.Linq;
using System.Reflection;

namespace CSSParser.TextReaderNavigators
{
	/// <summary>
	/// This class enables the specification of "weak events" (meaning that there is no direct reference to the listener from the event publisher) so
	/// long as the event publisher can be written to make use of this (no change to listeners will then be required)
	/// </summary>
	public class WeakEventSource<TEventArgs> where TEventArgs : EventArgs
	{
		private SimpleImmutableSet<IMakeEventCallbacks> _callbacks;
		public WeakEventSource() : this(new SimpleImmutableSet<IMakeEventCallbacks>()) { }
		private WeakEventSource(SimpleImmutableSet<IMakeEventCallbacks> callbacks)
		{
			if (callbacks == null)
				throw new ArgumentNullException("callbacks");
			if (callbacks.Any(c => c == null))
				throw new ArgumentException("Null reference encountered in callbacks set");

			_callbacks = callbacks;
		}

		public static WeakEventSource<TEventArgs> operator +(WeakEventSource<TEventArgs> e1, EventHandler<TEventArgs> e2)
		{
			if (e2 == null)
				return e1;

			var e2Callbacks = (e2.GetInvocationList() ?? new Delegate[0])
				.Select(
					singleCastDelegate => MakeWeak((EventHandler<TEventArgs>)singleCastDelegate)
				);
			if (!e2Callbacks.Any())
				return e1;

			// Since _callbacks is an immutable set it can't be changed in place, so we just need to get a reference to its current state
			// and generate a new instance based off that data (and the e2 handler).
			if (e1 == null)
				return new WeakEventSource<TEventArgs>(new SimpleImmutableSet<IMakeEventCallbacks>(e2Callbacks));

			return new WeakEventSource<TEventArgs>(e1._callbacks.AddRange(e2Callbacks));
		}

		public static WeakEventSource<TEventArgs> operator -(WeakEventSource<TEventArgs> e1, EventHandler<TEventArgs> e2)
		{
			if (e2 == null)
				return e1;

			if (e1 == null)
				return null;

			// As with the addition operator, we only need a copy of the reference to the current state of the _callbacks data for e1,
			// the new instance will be based off that
			var e1CallbacksCopy = e1._callbacks;
			var removedAnyCallbacks = false;
			foreach (var eventHandlerToRemove in e2.GetInvocationList().Cast<EventHandler<TEventArgs>>())
			{
				var callbackToRemoveIfAny = e1CallbacksCopy
					.Select((callback, index) => new { Callback = callback, Index = index })
					.FirstOrDefault(c => c.Callback.CorrespondsTo(eventHandlerToRemove));
				if (callbackToRemoveIfAny != null)
				{
					e1CallbacksCopy = e1CallbacksCopy.RemoveAt(callbackToRemoveIfAny.Index);
					removedAnyCallbacks = true;
				}
			}
			return removedAnyCallbacks ? new WeakEventSource<TEventArgs>(e1CallbacksCopy) : e1;
		}

		private static IMakeEventCallbacks MakeWeak(EventHandler<TEventArgs> singleCastDelegate)
		{
			if (singleCastDelegate == null)
				throw new ArgumentNullException("singleCastDelegate");

			if (singleCastDelegate.Target == null)
				return new StaticCallbackData(singleCastDelegate);

			var instanceCallbackTypeForListener = typeof(InstanceCallbackData<>).MakeGenericType(new[]
			{
				typeof(TEventArgs),
				singleCastDelegate.Target.GetType()
			});
			return (IMakeEventCallbacks)instanceCallbackTypeForListener
				.GetConstructor(new[] { typeof(EventHandler<TEventArgs>) })
				.Invoke(new[] { singleCastDelegate });
		}

		public void Fire(object sender, TEventArgs e)
		{
			if (sender == null)
				throw new ArgumentNullException("sender");
			if (e == null)
				throw new ArgumentNullException("e");

			var haveAnyCallbacksExpired = false;
			var callbacksCopy = _callbacks;
			foreach (var callback in callbacksCopy)
			{
				var didCallbackExecute = callback.ExecuteCallbackIfTargetStillAlive(sender, e);
				if (!didCallbackExecute)
					haveAnyCallbacksExpired = true;
			}

			if (haveAnyCallbacksExpired)
			{
				lock (_callbacks)
				{
					_callbacks = _callbacks.RemoveWhere(c => !c.IsTargetStillAlive);
				}
			}
		}

		private interface IMakeEventCallbacks
		{
			/// <summary>
			/// If false then the target was a WeakReference whose target has been collected. If true then either the reference is still active or a
			/// static callback was specified (which has no target instance). This should be used to remove expired entries from a callback list, not
			/// called before ExecuteCallbackIfTargetStillAlive since the target may be collected between testing this and calling that method.
			/// </summary>
			bool IsTargetStillAlive { get; }

			/// <summary>
			/// This will execute the callback unless its target was a WeakReference whose target has been collected, in which case it will return
			/// false. It will return true if the callback was exected.
			/// </summary>
			bool ExecuteCallbackIfTargetStillAlive(object sender, TEventArgs e);
			
			/// <summary>
			/// This indicates whether its callback corresponds to the specified event handler to enable the unhooking of event registrations
			/// </summary>
			bool CorrespondsTo(EventHandler<TEventArgs> eventHandler);
		}

		private class StaticCallbackData : IMakeEventCallbacks
		{
			private readonly EventHandler<TEventArgs> _callback;
			public StaticCallbackData(EventHandler<TEventArgs> callback)
			{
				if (callback == null)
					throw new ArgumentNullException("callback");
				if (callback.Target != null)
					throw new ArgumentException("The specified callback may not have a Target, it must be static");

				_callback = callback;
			}

			public bool ExecuteCallbackIfTargetStillAlive(object sender, TEventArgs e)
			{
				if (sender == null)
					throw new ArgumentNullException("sender");
				if (e == null)
					throw new ArgumentNullException("e");

				// There is no target reference to expire so this will always return true after executing the callback
				_callback(sender, e);
				return true;
			}

			public bool IsTargetStillAlive
			{
				get { return true; } // There is no target to stop being alive so technicallyt this should always return true
			}

			public bool CorrespondsTo(EventHandler<TEventArgs> eventHandler)
			{
				if (eventHandler == null)
					throw new ArgumentNullException("eventHandler");

				// If it's not a static callback then it can't apply to this class since it only deal with static methods
				if (eventHandler.Target != null)
					return false;

				return (_callback.Method == eventHandler.Method);
			}
		}

		private class InstanceCallbackData<TListener> : IMakeEventCallbacks
		{
			private readonly WeakReference _target;
			private readonly MethodInfo _originalMethod;
			private readonly Action<TListener, object, TEventArgs> _callback;
			public InstanceCallbackData(EventHandler<TEventArgs> callback)
			{
				if (callback == null)
					throw new ArgumentNullException("callback");
				var target = callback.Target;
				if (target == null)
					throw new ArgumentException("The specified callback may not have a null Target, it may not be static");
				if (target.GetType() != typeof(TListener))
					throw new ArgumentException("The specified callback's Target must match the typeparam TListener");

				_callback = (Action<TListener, object, TEventArgs>)Delegate.CreateDelegate(
					typeof(Action<TListener, object, TEventArgs>),
					callback.Method,
					true // Specify true for throwOnBindFailure, want to fail early where appropriate
				);
				_target = new WeakReference(target);
				_originalMethod = callback.Method; // This is only required to implement IMakeEventCallbacks.CorrespondsTo
			}

			public bool ExecuteCallbackIfTargetStillAlive(object sender, TEventArgs e)
			{
				if (sender == null)
					throw new ArgumentNullException("sender");
				if (e == null)
					throw new ArgumentNullException("e");

				var target = _target.Target;
				if (target == null)
					return false;

				_callback((TListener)target, sender, e);
				return true;
			}

			public bool IsTargetStillAlive
			{
				get { return _target.IsAlive; }
			}

			public bool CorrespondsTo(EventHandler<TEventArgs> eventHandler)
			{
				if (eventHandler == null)
					throw new ArgumentNullException("eventHandler");
				
				// If it's a static callback then it can't apply to this class since it only deal with instance methods
				if (eventHandler.Target == null)
					return false;

				// If the target is no longer accessible then we can't confirm whether there's a match or not (and this callback entry will be removed from
				// the list next time the event is fired as it will report IsTargetStillAlive as false)
				var target = _target.Target;
				if (target == null)
					return false;

				return ((target == eventHandler.Target) && (_originalMethod == eventHandler.Method));
			}
		}
	}
}
