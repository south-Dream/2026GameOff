# Single Scene Level Flow

## 结构

仅保留 Game 场景，包含 LevelManager、LevelRoot、MenuRoot、UIRoot 和 EventSystem

Level 预制体包含 LevelController、SpaceManager、Map、Camera、Player 与 Environment，运行时只实例化已保存的 Level 预制体

资源命名和 Hierarchy 对象统一使用英文，玩家可见文字与代码注释允许中文

## 配置与职责

LevelData 保存十关的顺序，每项配置 Level 预制体、标题、说明和入场提示文字

LevelManager 负责加载、卸载、流程暂停、最后一关完成与重新开始

LevelController 保存包内引用并初始化 SpaceManager 与出口，SpaceManager 继续协调空间、地图、角色和镜头

UIMgr 直接引用场景中已配置的 StartUI、LevelUI、GameUI 和 SettingsUI。GameUI 接收关卡事件，控制共用提示和完成界面；旋转关的特殊提示也预先放在界面预制体中，只切换显隐，不在运行时构建或实例化 UI

## 出口交互

LevelExit 继承 Interactable，放在地图终点，靠近后通过现有事件显示 F 交互提示

只有站稳、三维行走状态、镜头与机关均已停稳才允许使用出口，自动进入终点不切关

F 进入下一关，切换过程中禁止重复请求；最后一关的出口显示完成旅程，按 F 后显示全部完成界面

## 生命周期

暂停旧关卡并清除 UI → 停用并销毁旧包 → 等待销毁完成 → 实例化新包 → 显式初始化 Map、Player、Camera 和 Space → 发送 LevelLoaded → 打开本关提示 → 关闭提示后恢复玩法

R 复位当前关卡，不加载下一关；完成界面的重新开始从第一关重新加载

## 实现与验证计划

- [x] 隔离 Unity 项目记录缺少单场景流程的失败基线
- [x] 实现 LevelData、LevelManager、LevelController 与 LevelExit
- [x] 将 SpaceManager 的 Awake 初始化改为显式 Init，加入流程输入开关，移除自动通关
- [x] GameUI 接收关卡加载、卸载、完成事件，管理共用提示、预置 TipUI 和重新开始
- [x] 保存三个 Level 包及 TipUI、HUD 预制体，生成唯一 Game 场景并移除旧演示场景
- [x] 迁移原地图布局和美术环境，清理中文对象名与旧 Gameplay 组合资源
- [x] Unity 原生验证三关完整路线、F 出口、禁止二维出口、提示暂停、重复请求、卸载清理、最终完成与重新开始
- [x] 检查资源 GUID、英文命名、构建场景和文档路径

## 验证结果

Unity 2022.3 原生运行验证通过三个关卡的完整路线、出口范围限制、二维出口限制、提示暂停、提示关闭按钮、重复交互保护、配置缺失保留当前关卡、旧包销毁、末关完成、重新开始和场景重载后的事件订阅数量

无提示配置能够直接开始，UI 状态回放不会重新打开已关闭的提示，预置 TipUI 在卸载时隐藏

后续补充 LevelPkg04—10，并接入同一 LevelList；十个选关节点均可进入。新增七关通过角色实际移动验证压缩路线、机关操作、展开位置、重置与出口推进，第 10 关出口显示最终完成界面。灰盒布局与试玩边界见 `Documents~/玩法/后续关卡灰盒.md`

资源检查通过 1075 处 GUID 引用，Assets 内资源文件名与预制体对象名均为英文，构建列表仅包含 Game 场景
