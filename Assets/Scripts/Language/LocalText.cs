using UnityEngine;
using UnityEngine.UI;
namespace PlaneShift
{
    /// <summary>
    /// 将文本组件绑定至语言词条
    /// </summary>
    public class LocalText : MonoBehaviour
    {
        [SerializeField] private Text target;
        [SerializeField] private string id;
        private object[] args = new object[0];
        private void OnEnable() { LanguageManager.Changed += Apply; Apply(); }
        private void OnDisable() => LanguageManager.Changed -= Apply;
        /// <summary>
        /// 保留词条与参数 语言切换后重新应用
        /// </summary>
        public void Set(string key, params object[] values) { id = key; args = values; Apply(); }
        private void Apply() => target.text = LanguageManager.Get(id, args);
    }
}
