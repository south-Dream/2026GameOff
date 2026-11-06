using UnityEngine;
using UnityEngine.UI;

namespace PlaneShift
{
    /// <summary>
    /// 独立显示音量设置并暂停当前关卡输入
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private LevelManager levels;
        public GuideManager guide;
        [SerializeField] private Slider musicVolume;
        [SerializeField] private Slider soundVolume;
        [SerializeField] private Text musicValue;
        [SerializeField] private Text soundValue;
        [SerializeField] private AudioSource music;
        [SerializeField] private AudioSource sound;

        public void Init()
        {
            float musicLevel = PlayerPrefs.GetFloat("MusicVolume", 0.4f);
            float soundLevel = PlayerPrefs.GetFloat("SoundVolume", 0.7f);
            musicVolume.SetValueWithoutNotify(musicLevel);
            soundVolume.SetValueWithoutNotify(soundLevel);
            SetMusic(musicLevel);
            SetSound(soundLevel);
            music.Play();
        }

        public void Show()
        {
            levels.SetPaused(true);
            guide.Apply();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            PlayerPrefs.Save();
            levels.SetPaused(false);
            guide.Apply();
        }

        public void SetMusic(float value)
        {
            music.volume = value;
            musicValue.text = Mathf.RoundToInt(value * 100) + "%";
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        public void SetSound(float value)
        {
            sound.volume = value;
            soundValue.text = Mathf.RoundToInt(value * 100) + "%";
            PlayerPrefs.SetFloat("SoundVolume", value);
        }

        public void SetLanguage(string code) { LanguageManager.SetLanguage(code); PlayerPrefs.SetString("Language", code); PlayerPrefs.Save(); }

        public void PlayClick() => sound.Play();
    }
}
