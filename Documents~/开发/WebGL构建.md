# WebGL 构建

## 当前工程

- Unity 2022.3.62f3c1
- 构建场景仅保留 Assets/Scenes/Game.unity
- 初始画布 1280 × 720
- Compression Format 为 Gzip，Decompression Fallback 已开启，减少普通静态托管对压缩响应头的要求
- 建议输出到 Builds/WebGL，此目录已被 Git 忽略

当前编辑器只安装了 Windows 构建模块，尚未安装 WebGL Build Support，因此还没有生成 WebGL 构建包，也尚未完成浏览器运行验证

## 安装模块

Unity Hub → 安装 → 找到 2022.3.62f3c1 → 齿轮 → 添加模块 → WebGL Build Support

必须给当前版本安装对应模块。当前安装记录中的模块下载约 588 MB，安装约占 2.7 GB。安装完成后重新打开工程

如果 Hub 的添加模块不可用，可使用当前编辑器安装记录指向的官方独立安装器，并选择相同的 Unity 安装目录：

[2022.3.62f3c1 WebGL Build Support](https://download.unitychina.cn/download_unity/1623fc0bbb97/TargetSupportInstaller/UnitySetup-WebGL-Support-for-Editor-2022.3.62f3c1.exe)

## 构建与本地运行

1. File → Build Settings，确认 Scenes In Build 只有 Game 场景且勾选
2. 选择 WebGL → Switch Platform，等待资源导入和脚本编译完成
3. 关闭 Development Build 与 Script Debugging，首次测试可保留默认构建设置
4. 点击 Build And Run，输出选择工程下 Builds/WebGL
5. 使用 Unity 自动启动的本地 HTTP 地址测试开始、选关、压缩、交互、通关、音量和双语切换

不要双击输出的 index.html 通过 file:// 直接运行。WebGL 应使用 HTTP 服务，Build And Run 会自动启动本地服务

## 上传网页平台

将 WebGL 输出目录的内容整体打成 ZIP，让 index.html 位于 ZIP 根目录，Build 与 TemplateData 保持原有相对位置。上传 itch.io 时选择 HTML 游戏并设置在浏览器中运行，建议开启全屏入口

构建产物不提交到源码仓库。正式发布前应在实际托管地址再次测试加载、音频和存档，浏览器构建结果不能由编辑器内测试代替

参考：[Unity 2022.3 构建说明](https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-building.html)、[压缩与部署说明](https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-deploying.html)
