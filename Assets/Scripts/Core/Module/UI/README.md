# UI框架使用说明

## 概述

这是一个完整的Unity UI框架，提供了View和Item的基础架构，支持主视图和弹窗视图的管理。

## 核心组件

### 1. 接口和基类

- **IUIView**: UI视图基础接口
- **BaseUIView**: UI视图基类，实现IUIView接口
- **MainView**: 主视图基类，用于主要的UI界面
- **PopupView**: 弹窗视图基类，用于弹窗界面
- **IUIItem**: UI Item基础接口
- **BaseUIItem**: UI Item基类，实现IUIItem接口

### 2. 管理器

- **UIManager**: UI管理器，负责管理所有UI视图
- **UIModule**: UI模块，提供便捷的UI操作接口

### 3. 层级管理

- **UILayer**: UI层级枚举，定义了不同的UI层级

## 使用方法

### 1. 创建主视图

```csharp
public class MyMainView : MainView
{
    [SerializeField] private Button myButton;
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        myButton.onClick.AddListener(OnMyButtonClick);
    }
    
    protected override void OnMainViewOpen()
    {
        base.OnMainViewOpen();
        // 视图打开时的逻辑
    }
    
    private void OnMyButtonClick()
    {
        // 按钮点击逻辑
    }
}
```

### 2. 创建弹窗视图

```csharp
public class MyPopupView : PopupView
{
    [SerializeField] private Button confirmButton;
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        confirmButton.onClick.AddListener(OnConfirmClick);
    }
    
    private void OnConfirmClick()
    {
        ClosePopup(); // 关闭弹窗
    }
}
```

### 3. 创建UI Item

```csharp
public class MyItemData
{
    public string name;
    public int value;
}

public class MyUIItem : BaseUIItem
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text valueText;
    
    protected override void OnSetData(object data)
    {
        base.OnSetData(data);
        MyItemData itemData = GetData<MyItemData>();
        if (itemData != null)
        {
            nameText.text = itemData.name;
            valueText.text = itemData.value.ToString();
        }
    }
}
```

### 4. 使用UI管理器

```csharp
// 打开主视图
UIModule.Instance.OpenMainView<MyMainView>("MyMainView");

// 打开弹窗
UIModule.Instance.OpenPopupView<MyPopupView>("MyPopupView");

// 关闭视图
UIModule.Instance.CloseView("MyMainView");

// 销毁视图
UIModule.Instance.DestroyView("MyMainView");

// 获取视图
MyMainView view = UIModule.Instance.GetView<MyMainView>("MyMainView");
```

## 层级设置

在UIManager中设置不同层级的父物体：

1. 在场景中创建Canvas
2. 在Canvas下创建不同层级的空物体（Background、Main、Popup、Toast、Top）
3. 将UIManager组件添加到物体上
4. 在UIManager的Layer Parents数组中设置对应的层级父物体

## 预制体注册

```csharp
// 注册视图预制体
UIManager.Instance.RegisterViewPrefab("MyMainView", mainViewPrefab);
UIManager.Instance.RegisterViewPrefab("MyPopupView", popupViewPrefab);
```

## 注意事项

1. 所有视图都应该继承自MainView或PopupView
2. 所有Item都应该继承自BaseUIItem
3. 在OnInitialize中绑定事件，在OnDestroy中解绑事件
4. 使用SetData方法为Item设置数据
5. 弹窗可以通过Close()方法关闭
6. 主视图可以通过Close()方法关闭

## 示例

参考 `Examples` 文件夹中的示例代码：
- ExampleMainView.cs
- ExamplePopupView.cs
- ExampleUIItem.cs 