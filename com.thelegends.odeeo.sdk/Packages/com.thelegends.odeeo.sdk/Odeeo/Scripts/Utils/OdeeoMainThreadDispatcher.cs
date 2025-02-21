using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Odeeo.Utils
{
    internal class OdeeoMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> executionQueue = new Queue<Action>();
        
        private static OdeeoMainThreadDispatcher _instance;

        public static OdeeoMainThreadDispatcher Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;
                
                GameObject go = new GameObject("OdeeoMainThreadDispatche");
                _instance = go.AddComponent<OdeeoMainThreadDispatcher>();

                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                {
                    executionQueue.Dequeue().Invoke();
                }
            }
        }

        internal void Enqueue(Action action)
        {
            Enqueue(ActionWrapper(action));
        }
        
        /// <summary>
        /// Locks the queue and adds the IEnumerator to the queue
        /// </summary>
        /// <param name="action">IEnumerator function that will be executed from the main thread.</param>
        private void Enqueue(IEnumerator action)
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(() => { StartCoroutine(action); });
            }
        }
        
        private static IEnumerator ActionWrapper(Action action)
        {
            action();
            yield return null;
        }
    }
}