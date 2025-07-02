using UnityEngine;
using UnityEngine.UI;

namespace Core.Module.UI.Examples
{
    /// <summary>
    /// 示例UI Item数据
    /// </summary>
    [System.Serializable]
    public class ExampleItemData
    {
        public string title;
        public string description;
        public Sprite icon;
        public int value;
        
        public ExampleItemData(string title, string description, Sprite icon, int value)
        {
            this.title = title;
            this.description = description;
            this.icon = icon;
            this.value = value;
        }
    }
    
    /// <summary>
    /// 示例UI Item
    /// </summary>
    public class ExampleUIItem : BaseUIItem
    {
        [Header("Example UI Item")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text valueText;
        [SerializeField] private Button itemButton;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            if (itemButton != null)
                itemButton.onClick.AddListener(OnItemClick);
        }
        
        protected override void OnSetData(object data)
        {
            base.OnSetData(data);
            
            ExampleItemData itemData = GetData<ExampleItemData>();
            if (itemData != null)
            {
                UpdateDisplay(itemData);
            }
        }
        
        protected override void OnRefresh()
        {
            base.OnRefresh();
            
            ExampleItemData itemData = GetData<ExampleItemData>();
            if (itemData != null)
            {
                UpdateDisplay(itemData);
            }
        }
        
        private void UpdateDisplay(ExampleItemData data)
        {
            if (titleText != null)
                titleText.text = data.title;
                
            if (descriptionText != null)
                descriptionText.text = data.description;
                
            if (iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;
                
            if (valueText != null)
                valueText.text = data.value.ToString();
        }
        
        private void OnItemClick()
        {
            ExampleItemData itemData = GetData<ExampleItemData>();
            if (itemData != null)
            {
                Debug.Log($"点击了Item: {itemData.title}, 值: {itemData.value}");
                
                // 这里可以触发事件或回调
                // OnItemSelected?.Invoke(itemData);
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (itemButton != null)
                itemButton.onClick.RemoveListener(OnItemClick);
        }
    }
} 