using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 复制默认记录并保存玩家的首次体验和通关状态
    /// </summary>
    public class RecordManager : MonoBehaviour
    {
        [SerializeField] private RecordData defaults;
        private const string SaveKey = "PlayerRecord";
        public RecordData Data { get; private set; }

        public void Init()
        {
            Data = Instantiate(defaults);
            if (PlayerPrefs.HasKey(SaveKey)) JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(SaveKey), Data);
            for (int i = 0; i < 10; i++)
                if (PlayerPrefs.GetInt("LevelComplete_" + i, 0) == 1 && !Data.completed.Contains(i)) Data.completed.Add(i);
        }

        public bool HasStep(GuideStep step) => Data.steps.Contains(step);

        public void Finish(GuideStep step)
        {
            if (Data.steps.Contains(step)) return;
            Data.steps.Add(step);
            Save();
        }

        public void ReadHelp() { Data.helpRead = true; Save(); }
        public void Visit(int level) { if (!Data.visited.Contains(level)) { Data.visited.Add(level); Save(); } }
        public void Complete(int level)
        {
            if (!Data.completed.Contains(level)) Data.completed.Add(level);
            PlayerPrefs.SetInt("LevelComplete_" + level, 1);
            Save();
        }

        public void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (Data != null) Destroy(Data);
        }
    }
}
