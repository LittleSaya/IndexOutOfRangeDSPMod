# DSP Add Planet

## 警告

**务必提前备份游戏存档！**

**目前一旦对存档使用该mod就不能回头了！**

**如果已经使用了mod，也请务必保存好您的配置文件！**

## 介绍

该mod允许您向自己已有的存档中添加新的行星。

当您第一次安装该mod并启动游戏时，该mod会在游戏的存档目录下创建一个配置文件（配置文件的具体位置见下文），您可以在配置文件中添加新的星球。

首先，您需要确保您想添加新行星的存档有一个名字，自动保存的新游戏没有名字，如果您不确定的话，请手动保存游戏，并在保存时输入存档名称。

要添加新的行星，首先需要知道一些原有行星的信息，按V打开星图，点击右下角的“Add Planet”按钮可以打开显示行星信息的工具。

工具左侧是恒星列表，当前恒星在最上方，右侧是当前选中恒星的所有行星的信息，包含了行星的index、orbitIndex、orbitAround和number等信息。

点击右侧的“Copy”按钮可以将“唯一恒星ID”复制到系统的剪贴板上，“唯一恒星ID”是一串由“存档名称”、“星区名称”和“恒星名称”构成的字符串，基本上唯一确定了一个恒星。

获取必要的信息后，您可以按照本文“配置文件”小节中的内容在配置文件中添加新的行星。

添加好新行星的配置，保存配置文件，然后退出游戏，**切记一定要备份游戏存档**，然后重新启动游戏，加载刚才添加了新行星的存档，应该就能看见新的行星了。

备注：自动保存的存档的“存档名称”和最后一次加载的存档名称相同（尽管没有显示在界面上），所以可以放心使用自动存档。

## 配置文件

### 配置文件的位置

配置文件位于游戏存档目录下的一个子目录中，子目录的名字叫“modData/IndexOutOfRange.DSPAddPlanet”，配置文件的文件名叫“config.txt”

举例来说，一个存档的完整路径可能是：`C:\Users\administrator\Documents\Dyson Sphere Program\Save\modData\IndexOutOfRange.DSPAddPlanet\config.txt`

### 行星的顺序

新的行星在游戏内被创建的顺序与您在配置文件中书写它们的顺序相同。

### 格式

一行一个行星，可以有空行和“#”开头的备注行，行格式类似URL参数但不完全相同（该mod使用的URL参数解析器非常简单）。

例子：`uniqueStarId=2_test-44525415-64-A10-Erakis&index=4&orbitAround=0&orbitIndex=4&number=5&gasGiant=false`

### 参数描述

```
     uniqueStarId (必须): 能够基本上唯一确定一个恒星的字符串，由“存档名称”、“星区名称”和“恒星名称”构成
                          举例来说，如果“2_test”是您的存档名称，“44525415-64-A10”是您的星区名称，“Erakis”是您想要添加新的行星的恒星名称，则该恒星的“唯一恒星ID”就是“2_test-44525415-64-A10-Erakis”

            index (必须): 新行星的索引，应该比目标恒星的最后一个行星的索引大1
                          比如说如果原来这个星系里有4个行星，您新增了一个行星，那么这个新行星的索引就是4（索引从0开始）

      orbitAround (必须): 新行星围绕哪个行星旋转，设置成目标行星的“number”就围绕目标行星旋转，设置成0就围绕恒星旋转

       orbitIndex (必须): 新行星在哪个轨道上公转，最小1，最大16，不能和原有行星重合

           number (必须): 新行星的编号，编号规则见下图

         gasGiant (必须): 新行星是否是气态巨星，值可以为true或false，设置成true会导致下面关于星星半径的设置失效

        info_seed (可选): 默认值为0，两个用于生成行星的种子之一。

         gen_seed (可选): 默认值为0，两个用于生成行星的种子之一。

     planetRadius (可选): 默认值为200，行星半径，默认的200是正常大小，最大600
                          200、400和600是测试过的，800会出问题

forcePlanetRadius (可选): 默认值为false，如果您想测试半径大于600的行星，请将该项设置为true

    orbitalPeriod (可选): 默认值为3600，公转周期，单位“秒”

   rotationPeriod (可选): 默认值为3600，自转周期，单位“秒”

    isTidalLocked (可选): 默认值为true，该项为true时，行星描述会多一串“潮汐锁定 永昼永夜”
                          这个选项应该没有实际影响，毕竟公转周期等于自转周期就已经是实际上的“潮汐锁定”了

 orbitInclination (可选): 默认值为0，轨道倾角

        obliquity (可选): 默认值为0，地轴倾角

 dontGenerateVein (可选): 默认值为true，是否**不**生成矿脉

            theme (可选): 无默认值，用于指定新行星的主题，下方“可选主题”表格中列出了当前游戏中所有可选的主题，如果不指定该选项将会使用游戏自动生成的主题，
                          这里的选择的行星主题必须和上面的`gasGiant`选项相匹配，如果设置了`gasGiant=true`，则应该选择“气态行星”或“冰巨星”主题
```

#### 可选主题（游戏版本 0.9.25.12201 ）

| ID | 名称 | 行星类型 | 温度 | 气体种类 | 产气速度 | 风 | ion height | 海面高度 | 海洋类型 | culling radius | ice flag |
| --- | ---- | ----------- | ----------- | --------- | ---------- | ---- | ---------- | ------------ | ---------- | -------------- | -------- |
| 1 | 地中海 | Ocean | 0 |  |  | 1 | 60 | 0 | 水 | 0 | 0 |
| 2 | 气态巨星 | Gas | 2 | 氢, 重氢 | 0.96, 0.04 | 0 | 0 | 0 |  | 0 | 0 |
| 3 | 气态巨星 | Gas | 1 | 氢, 重氢 | 0.96, 0.04 | 0 | 0 | 0 |  | 0 | 0 |
| 4 | 冰巨星 | Gas | -1 | 可燃冰, 氢 | 0.7, 0.3 | 0 | 0 | 0 |  | 0 | 0 |
| 5 | 冰巨星 | Gas | -2 | 可燃冰, 氢 | 0.7, 0.3 | 0 | 0 | 0 |  | 0 | 0 |
| 6 | 干旱荒漠 | Desert | 2 |  |  | 1.5 | 70 | 0 |  | 0 | 0 |
| 7 | 灰烬冻土 | Desert | -1 |  |  | 0.4 | 50 | 0 |  | 0 | 0 |
| 8 | 海洋丛林 | Ocean | 0 |  |  | 1 | 60 | 0 | 水 | 0 | 0 |
| 9 | 熔岩 | Vocano | 5 |  |  | 0.7 | 60 | -2.5 |  | 0 | 0 |
| 10 | 冰原冻土 | Ice | -5 |  |  | 0.7 | 55 | 0 | 水 | 0 | 0 |
| 11 | 贫瘠荒漠 | Desert | -2 |  |  | 0 | 0 | 0 |  | 0 | 0 |
| 12 | 戈壁 | Desert | 1 |  |  | 0.8 | 60 | 0 |  | 0 | 0 |
| 13 | 火山灰 | Vocano | 4 |  |  | 0.8 | 70 | 0 | 硫酸 | 0 | 0 |
| 14 | 红石 | Ocean | 0 |  |  | 1 | 60 | 0 | 水 | 0 | 0 |
| 15 | 草原 | Ocean | 0 |  |  | 1.1 | 60 | 0 | 水 | 0 | 0 |
| 16 | 水世界 | Ocean | 0 |  |  | 1.1 | 60 | 0 | 水 | -10 | 0 |
| 17 | 红土荒漠 | Desert | 1 |  |  | 1.1 | 60 | 0 |  | 0 | 0 |
| 18 | 白色海洋 | Ocean | 0 |  |  | 1 | 60 | 0 | 水 | 0 | 0 |
| 19 | 三色荒漠 | Desert | 1 |  |  | 1.6 | 70 | 0 |  | 0 | 0 |
| 20 | 红冰荒漠 | Desert | -2 |  |  | 0.7 | 55 | 0 |  | 0 | 1 |
| 21 | 气态巨星 | Gas | 1 | 氢, 重氢 | 0.84, 0.16 | 0 | 0 | 0 |  | 0 | 0 |

#### 关于“number”参数：

![parameter_number.png](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/parameter_number.png "关于“number”参数")

### 示例配置文件

```
# Add additional planets to your game.
# New planets will be added in the same order as they are written in this file.
# The format of the config value is similar to URL query string (but not the same, the parser used here is extremely simple)
# For detailed description, please refer to https://dsp.thunderstore.io/package/IndexOutOfRange/DSPAddPlanet/
(EXAMPLE)uniqueStarId=UNIQUE_STAR_ID&index=INDEX&orbitAround=ORBIT_AROUND&orbitIndex=ORBIT_INDEX&number=NUMBER&gasGiant=GAS_GIANT&info_seed=INFO_SEED&gen_seed=GEN_SEED&planetRadius=PLANET_RADIUS&forcePlanetRadius=FORCE_PLANET_RADIUS&orbitalPeriod=ORBITAL_PERIOD&rotationPeriod=ROTATION_PERIOD&isTidalLocked=IS_TIDAL_LOCKED&orbitInclination=ORBIT_INCLINATION&obliquity=OBLIQUITY&dontGenerateVein=DONT_GENERATE_VEIN

uniqueStarId=2_test-44525415-64-A10-Erakis&index=4&orbitAround=0&orbitIndex=6&number=4&gasGiant=false&planetRadius=600
```

### 其他重要事项

小心处理存档名称，新增的行星是通过“唯一恒星ID”与您的存档绑定的。

### 截图

![screenshot1.jpg](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/screenshot1.jpg "screenshot1")

![screenshot2.jpg](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/screenshot2.jpg "screenshot2")

![screenshot3.jpg](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/screenshot3.jpg "screenshot3")

## 待办

## 兼容性

### 0.0.1 ~ 0.0.10

Build target ：游戏版本 0.9.25.12201 ， BepInEx 版本： 5.4.19 （应该也能够在 5.4.17 版本下正常运行）

## 更新日志

### 0.0.9 -> 0.0.10

- 修正了在大行星上无法在正常距离放置分拣器的问题（提示“太近了”）（感谢 GalacticScale ）

### 0.0.8 -> 0.0.9

- 添加`theme`选项，用于指定新行星的主题

### 0.0.7 -> 0.0.8

- 修正行星大气层的渲染半径

### 0.0.6 -> 0.0.7

- 修正当新行星数量太多时运输船无法正常停靠的问题

### 0.0.5 -> 0.0.6

- 对配置文件的改动将会在载入存档时生效，不再需要重启游戏

### 0.0.4 -> 0.0.5

- 使蓝图摄像机的最小和最大高度根据当前行星的实际半径计算（在大行星上使用蓝图体验更好）

### 0.0.3 -> 0.0.4

- 修复在大半径行星上丢垃圾时垃圾消失的问题（半径 >= 600）

### 0.0.2 -> 0.0.3

- 修复大半径行星上的物流站点无法正常停靠的问题

### 0.0.1 -> 0.0.2

- 添加中文翻译的 README

## 其他

### 关于对存档的不可逆影响

每次载入存档，游戏都会使用存档中的种子把星区重新生成一遍，如果该mod被打开，则该mod会在游戏生成完星区之后，再根据配置文件中的内容生成新的行星，也就是说，除了配置文件，没有其他地方会存储新增的行星。

如果您关闭该mod，新的行星会从您的游戏里消失，但是您可能已经在新的行星上摆放了建筑物，或者有一些航线和新的行星相关联，这些东西仍然存在于您的存档中，并且会指向不存在的行星并导致错误发生。

### 关于行星半径

行星半径的最大值似乎被一个无符号短整型限制了，最大半径可能是655.35。

## 鸣谢

感谢GalacticScale的作者，让我知道了该如何正确处理GetModPlane函数

感谢ILSpy的作者，创造了功能如此强大的工具

感谢BepInEx和Harmony的作者，创造了modder们所依赖的mod平台
