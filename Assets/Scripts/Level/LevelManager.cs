using System.Collections;
using UnityEngine;
using UnityLightFrame;

namespace PlaneShift
{
    /// <summary>
    /// 管理单场景内的关卡加载与推进
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelData data;
        [SerializeField] private Transform levelRoot;
        private LevelController current;
        private int index = -1;
        private bool loading;
        private bool completed;
        private bool paused;
        private bool helpPaused;
        public RecordManager record;
        public bool Paused => paused || helpPaused;
        public bool CanGuide => current != null && !loading && !completed && !Paused;
        private EventCenter.IDisposable restartSubscription;
        private EventCenter.IDisposable refreshSubscription;

        public LevelController Current => current;
        public LevelData Data => data;
        public int CurrentIndex => index;
        public bool HasNext => data != null && data.levels != null && index + 1 < data.levels.Length;

        private void OnEnable()
        {
            restartSubscription = EventCenter.Instance.SubscribeEvent(E_EventType.RestartRequested, Restart);
            refreshSubscription = EventCenter.Instance.SubscribeEvent(E_EventType.RequestUI, PublishLevel);
        }

        private void OnDisable()
        {
            restartSubscription?.Invoke();
            refreshSubscription?.Invoke();
        }

        public bool IsComplete(int level) => record.Data.completed.Contains(level);

        public void SetHelpPaused(bool value) { helpPaused = value; ApplyControl(); }

        public void SetPaused(bool value)
        {
            paused = value;
            ApplyControl();
        }

        /// <summary>
        /// 返回菜单时卸载关卡并清除本关提示
        /// </summary>
        public void Leave()
        {
            StopAllCoroutines();
            loading = false;
            completed = false;
            if (current != null)
            {
                current.SetControl(false);
                current.gameObject.SetActive(false);
                Destroy(current.gameObject);
                current = null;
            }
            EventCenter.Instance.TriggerEvent(E_EventType.LevelUnloading);
        }

        /// <summary>
        /// 只接受当前关卡的出口请求并阻止重复推进
        /// </summary>
        public bool TryFinishLevel(LevelController source)
        {
            if (loading || completed || source == null || source != current || !source.CanExit) return false;
            record.Complete(index);
            if (HasNext) return LoadLevel(index + 1);
            completed = true;
            current.SetControl(false);
            EventCenter.Instance.TriggerEvent(E_EventType.LevelCompleted);
            return true;
        }

        public bool LoadLevel(int target)
        {
            if (loading) return false;
            if (levelRoot == null || data == null || data.levels == null || target < 0 || target >= data.levels.Length ||
                data.levels[target] == null || data.levels[target].prefab == null || !data.levels[target].prefab.IsConfigured)
            {
                EventCenter.Instance.TriggerEvent(E_EventType.Notice, "notice.configuration");
                return false;
            }
            loading = true;
            StartCoroutine(ChangeLevel(target));
            return true;
        }

        /// <summary>
        /// 停用旧包并等待销毁后初始化新包 保证同一时间只有一套玩法对象
        /// </summary>
        private IEnumerator ChangeLevel(int target)
        {
            if (current != null)
            {
                current.SetControl(false);
                current.gameObject.SetActive(false);
                Destroy(current.gameObject);
                current = null;
            }
            EventCenter.Instance.TriggerEvent(E_EventType.LevelUnloading);
            yield return null;
            index = target;
            completed = false;
            LevelEntry entry = data.levels[index];
            current = Instantiate(entry.prefab, levelRoot);
            current.name = entry.prefab.name;
            current.Init(this);
            record.Visit(index);
            loading = false;
            ApplyControl();
            PublishLevel();
        }

        private void ApplyControl()
        {
            if (current != null) current.SetControl(!loading && !completed && !Paused);
        }

        private void Restart()
        {
            if (completed) LoadLevel(0);
        }

        private void PublishLevel()
        {
            if (loading || current == null) return;
            EventCenter.Instance.TriggerEvent(E_EventType.LevelLoaded, new LevelInfo
            {
                level = data.levels[index], number = index + 1, total = data.levels.Length
            });
            if (completed) EventCenter.Instance.TriggerEvent(E_EventType.LevelCompleted);
        }
    }
}
