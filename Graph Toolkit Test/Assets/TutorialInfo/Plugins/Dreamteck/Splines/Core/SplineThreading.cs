using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines
{
#if !UNITY_WSA
	using System.Threading;
#endif

	public static class SplineThreading
	{
		public delegate void EmptyHandler();

		public static int threadCount
		{
			get
			{
#if UNITY_WSA
                return 0;
#else
				return threads.Length;
#endif
			}
			set
			{
#if !UNITY_WSA
				if (value > threads.Length)
				{
					while (threads.Length < value)
					{
						var thread = new ThreadDef();
#if UNITY_EDITOR
						if (Application.isPlaying)
						{
							thread.Restart();
						}
#else
                        thread.Restart();
#endif
						ArrayUtility.Add(ref threads, thread);
					}
				}
#endif
			}
		}

		public static void Run(EmptyHandler handler)
		{
#if !UNITY_WSA
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				handler();
				return;
			}
#endif
			for (var i = 0; i < threads.Length; i++)
			{
				if (!threads[i].isAlive)
				{
					threads[i].Restart();
				}

				if (!threads[i].computing || i == threads.Length - 1)
				{
					threads[i].Queue(handler);
					if (!threads[i].computing)
					{
						threads[i].Interrupt();
					}

					break;
				}
			}
#endif
		}

		public static void PrewarmThreads()
		{
#if !UNITY_WSA
			for (var i = 0; i < threads.Length; i++)
			{
				if (!threads[i].isAlive)
				{
					threads[i].Restart();
				}
			}
#endif
		}

		public static void Stop()
		{
#if !UNITY_WSA
			for (var i = 0; i < threads.Length; i++)
			{
				threads[i].Abort();
			}
#endif
		}
#if !UNITY_WSA
		internal class ThreadDef
		{
			private readonly ParameterizedThreadStart m_start;
			private readonly Worker m_worker = new();
			internal Thread thread;

			internal ThreadDef()
			{
				m_start = RunThread;
			}

			internal bool isAlive => thread != null && thread.IsAlive;

			internal bool computing => m_worker.computing;

			internal void Queue(EmptyHandler handler)
			{
				m_worker.instructions.Enqueue(handler);
			}

			internal void Interrupt()
			{
				thread.Interrupt();
			}

			internal void Restart()
			{
				thread = new Thread(m_start);
				thread.Start(m_worker);
			}

			internal void Abort()
			{
				if (isAlive)
				{
					thread.Abort();
				}
			}

			internal class Worker
			{
				internal bool computing;
				internal Queue<EmptyHandler> instructions = new();
			}

			internal delegate void BoolHandler(bool flag);
		}

		internal static ThreadDef[] threads = new ThreadDef[2];
		internal static readonly object locker = new();

		static SplineThreading()
		{
			Application.quitting += Quitting;
			for (var i = 0; i < threads.Length; i++)
			{
				threads[i] = new ThreadDef();
			}

#if UNITY_EDITOR
			PrewarmThreads();
			EditorApplication.playModeStateChanged += OnPlayStateChanged;
#endif
		}

#if UNITY_EDITOR
		private static void OnPlayStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingPlayMode)
			{
				Quitting();
			}
		}
#endif

		private static void Quitting()
		{
			Stop();
		}

		private static void RunThread(object o)
		{
			var work = (ThreadDef.Worker)o;
			while (true)
			{
				try
				{
					work.computing = false;
					Thread.Sleep(Timeout.Infinite);
				}
				catch (ThreadInterruptedException)
				{
					work.computing = true;
					lock (locker)
					{
						while (work.instructions.Count > 0)
						{
							EmptyHandler h = work.instructions.Dequeue();
							if (h != null)
							{
								h();
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (ex.Message != "")
					{
						Debug.LogError("THREAD EXCEPTION " + ex.Message);
					}

					break;
				}
			}

			Debug.Log("Thread stopped");
			work.computing = false;
		}
#endif
	}
}