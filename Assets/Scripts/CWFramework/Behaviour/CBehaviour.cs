using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CWFramework
{
    /// <summary>
    /// CWFramework 컴포넌트의 최소 공통 기반 클래스입니다.
    /// 파생 클래스는 Unity 생명주기 메서드를 직접 선언하지 않고 On... 훅을 재정의합니다.
    /// </summary>
    public class CBehaviour : MonoBehaviour
    {
        public bool IsDestroyed { get; private set; }

        #region Transform

        public Vector3 Position
        {
            get => this ? transform.position : Vector3.zero;
            set
            {
                if (this)
                    transform.position = value;
            }
        }

        public Vector3 LocalPosition
        {
            get => this ? transform.localPosition : Vector3.zero;
            set
            {
                if (this)
                    transform.localPosition = value;
            }
        }

        public Quaternion Rotation
        {
            get => this ? transform.rotation : Quaternion.identity;
            set
            {
                if (this)
                    transform.rotation = value;
            }
        }

        public Quaternion LocalRotation
        {
            get => this ? transform.localRotation : Quaternion.identity;
            set
            {
                if (this)
                    transform.localRotation = value;
            }
        }

        public Vector3 LocalScale
        {
            get => this ? transform.localScale : Vector3.one;
            set
            {
                if (this)
                    transform.localScale = value;
            }
        }

        public Vector3 LossyScale => this ? transform.lossyScale : Vector3.one;

        #endregion

        #region GameObject

        public bool ActiveSelf => this && gameObject.activeSelf;
        public bool ActiveInHierarchy => this && gameObject.activeInHierarchy;

        public int Layer
        {
            get => this ? gameObject.layer : 0;
            set
            {
                if (this)
                    gameObject.layer = value;
            }
        }

        public void SetActive(bool value)
        {
            if (this && gameObject.activeSelf != value)
                gameObject.SetActive(value);
        }

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            IsDestroyed = false;
            OnAwake();
        }

        private void Start()
        {
            OnStarted();
        }

        private void OnEnable()
        {
            OnEnabled();
        }

        private void OnDisable()
        {
            OnDisabled();
        }

        private void OnDestroy()
        {
            OnReleased();
            IsDestroyed = true;
        }

        private void OnApplicationQuit()
        {
            OnApplicationQuitting();
        }

        protected virtual void OnAwake() { }
        protected virtual void OnStarted() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void OnReleased() { }
        protected virtual void OnApplicationQuitting() { }

        #endregion

        #region Coroutine

        protected Coroutine StartIfActive(IEnumerator routine)
        {
            if (routine == null || !isActiveAndEnabled)
                return null;

            return StartCoroutine(routine);
        }

        protected void StopAndClear(ref Coroutine coroutine)
        {
            if (coroutine != null && this)
                StopCoroutine(coroutine);

            coroutine = null;
        }

        protected Coroutine NextFrame(UnityAction callback)
        {
            return callback == null ? null : StartIfActive(CWCoroutineEx.NextFrame(callback));
        }

        protected Coroutine NextTimer(float delay, UnityAction callback)
        {
            if (callback == null)
                return null;

            if (delay <= 0f)
            {
                callback.Invoke();
                return null;
            }

            return StartIfActive(CWCoroutineEx.NextTimer(delay, callback));
        }

        protected Coroutine NextRealTimer(float delay, UnityAction callback)
        {
            if (callback == null)
                return null;

            if (delay <= 0f)
            {
                callback.Invoke();
                return null;
            }

            return StartIfActive(CWCoroutineEx.NextRealTimer(delay, callback));
        }

        #endregion
    }
}
