# DSP Add Planet

## 警告

**务必提前备份游戏存档！**

**目前一旦对存档使用该mod就不能回头了！**

**如果已经使用了mod，也请务必保存好您的配置文件！**

## 介绍

该mod允许您向自己已有的存档中添加新的行星（或者修改原有行星和修改出生点）。

当您第一次安装该mod并启动游戏时，该mod会在游戏的存档目录下创建一个配置文件（配置文件的具体位置见下文），您可以在配置文件中添加新的星球（或者修改原有的行星）。

要添加新的行星，首先需要知道一些原有行星的信息，按V打开星图，点击右下角的“Add Planet”按钮可以打开显示行星信息的工具。

工具左侧是恒星列表，当前恒星在最上方，右侧是当前选中恒星的所有行星的信息，包含了行星的index、orbitIndex、orbitAround和number等信息。

点击右侧的“Copy”按钮可以将“唯一恒星ID”复制到系统的剪贴板上，“唯一恒星ID”是一串由可选的“存档名称”、“星区名称”和“恒星名称”构成的字符串，基本上唯一确定了一个恒星。

获取必要的信息后，您可以按照本文“配置文件”小节中的内容在配置文件中添加新的行星。

添加好新行星的配置，保存配置文件，然后退出游戏，**切记一定要备份游戏存档**，然后重新启动游戏，加载刚才添加了新行星的存档，应该就能看见新的（或者修改的）行星了。

备注：自动保存的存档的“存档名称”和最后一次加载的存档名称相同（尽管没有显示在界面上），所以可以放心使用自动存档。

## 配置文件

### 配置文件的位置

配置文件位于游戏存档目录下的一个子目录中，子目录的名字叫“modData/IndexOutOfRange.DSPAddPlanet”，配置文件的文件名叫“config.xml”

举例来说，一个存档的完整路径可能是：`C:\Users\administrator\Documents\Dyson Sphere Program\Save\modData\IndexOutOfRange.DSPAddPlanet\config.xml`

### 行星的顺序

新的行星在游戏内被创建的顺序与您在配置文件中书写它们的顺序相同。

### 格式

旧txt文件的格式请参考旧版本的[说明文档](https://github.com/LittleSaya/IndexOutOfRangeDSPMod/blob/5c8a22a6f428211fcf242fe265cbc7423f76e00d/DSPAddPlanet/README-zh-Hans.md "说明文档")

自0.1.0版本起，配置文件改为xml格式，旧txt文件仍能读取但不会包含新的功能，xml文件的基本结构如下所示：

自0.2.0版本起，配置文件被分成了两部分：<Global>和<GameNameSpecific>

总地来说，<Global>行星的UniqueStarId没有GameName，其优先级较低，但是会影响任何“星区名称”和“恒星名称”与之相同的已经保存的游戏或者新的游戏，同时<GameNameSpecific>行星的UniqueStarId必须有GameName，其优先级比<Global>行星高，但是只会影响指定GameName（存档名称）的游戏。

另外，如果您创建了一个新游戏，但是没有手动保存它，那么它的GameName以及它的自动存档的GameName都会为空，也就是说它会使用<Global>里的配置。

```xml
<Config>
    <Global>
        <Planets>
            <Planet>
                <UniqueStarId>
                    <!-- <Global>中的行星配置没有GameName</Global> -->
                    <ClusterString>The cluster string</ClusterString>
                    <Star>The star name</Star>
                </UniqueStarId>
                <!-- 其他参数和<GameNameSpecific>中的相同 -->
            </Planet>
        </Planets>
    </Global>
    <GameNameSpecific>
        <Planets>
            <Planet>
                <UniqueStarId>
                    <GameName>The game name of your save file</GameName>
                    <ClusterString>The cluster string</ClusterString>
                    <Star>The star name</Star>
                </UniqueStarId>
                <IsBirthPoint>false</IsBirthPoint>
                <Index>4</Index>
                <OrbitAround>0</OrbitAround>
                <OrbitIndex>2</OrbitIndex>
                <Number>3</Number>
                <GasGiant>false</GasGiant>
                <InfoSeed>0</InfoSeed>
                <GenSeed>0</GenSeed>
                <ForcePlanetRadius>false</ForcePlanetRadius>
                <Radius>200</Radius>
                <OrbitalPeriod>3600</OrbitalPeriod>
                <RotationPeriod>3600</RotationPeriod>
                <IsTidalLocked>true</IsTidalLocked>
                <OrbitInclination>0</OrbitInclination>
                <Obliquity>0</Obliquity>
                <DontGenerateVein>true</DontGenerateVein>
                <ThemeId>0</ThemeId>
                <OrbitLongitude>0</OrbitLongitude>
                <VeinCustom>
                    <Iron>
                        <VeinGroupCount>
                            <Type>Accurate</Type>
                            <AccurateValue>10</AccurateValue>
                        </VeinGroupCount>
                        <VeinSpotCount>
                            <Type>Random</Type>
                            <RandomBaseValue>100000</RandomBaseValue>
                            <RandomCoef>1</RandomCoef>
                            <RandomMulOffset>0</RandomMulOffset>
                            <RandomAddOffset>5</RandomAddOffset>
                        </VeinSpotCount>
                        <VeinAmount>
                            <Type>Default</Type>
                        </VeinAmount>
                    </Iron>
                </VeinCustom>
                <ReplaceAllVeinsTo>Copper</ReplaceAllVeinsTo>
            </Planet>
        </Planets>
    </GameNameSpecific>
</Config>
```

### 参数描述

```xml
<Config>
    <GameNameSpecific>
        <Planets>
            <Planet>
                <UniqueStarId>
                    (必须)
                    能够基本上唯一确定一个恒星的字符串，由“存档名称”、“星区名称”和“恒星名称”构成
                    举例来说，如果“2_test”是您的存档名称，“44525415-64-A10”是您的星区名称，“Erakis”是您想要添加新的行星的恒星名称，
                    则该恒星的“唯一恒星ID”就是“2_test-44525415-64-A10-Erakis”
                </UniqueStarId>
                <IsBirthPoint>
                    (必须/可选)
                    默认值为false
                    该行星是否是该星区的出生点
                    如果您正在修改原始出生行星的主题的话，请至少在配置文件中的一个行星上将该选项设置为true，其原因是修改原始出生行星的主题后出生点信息会被抹掉。
                </IsBirthPoint>
                <Index>
                    (必须)
                    如果您正在添加新的行星，这就是新行星的索引，应该比目标恒星的最后一个行星的索引大1，并且同一个恒星周围每个行星的Index都应该唯一，
                    比如说如果原来这个星系里有4个行星，您新增了一个行星，那么这个新行星的索引就是4（索引从0开始）。
                    如果您正在修改原有的行星，那么这就是原有行星的索引，
                    举例来说，如果您想修改出生行星，首先您必须知道出生行星的索引，这就意味着您必须先不携带任何配置，使用相同的种子创建一个星区，然后进去瞥一眼原始出生行星的索引。
                </Index>
                <OrbitAround>
                    (必须)
                    新行星围绕哪个行星旋转，设置成目标行星的“Number”就围绕目标行星旋转，设置成0就围绕恒星旋转
                </OrbitAround>
                <OrbitIndex>
                    (必须)
                    新行星在哪个轨道上公转，最小1，最大16
                </OrbitIndex>
                <Number>
                    (必须)
                    新行星的编号，编号规则见下图
                </Number>
                <GasGiant>
                    (可选)
                    默认值为false
                    新行星是否是气态巨星，值可以为true或false，设置成true会导致关于行星半径的设置失效
                </GasGiant>
                <InfoSeed>
                    (可选)
                    默认值为0
                    两个用于生成行星的种子之一
                </InfoSeed>
                <GenSeed>
                    (可选)
                    默认值为0
                    两个用于生成行星的种子之一
                </GenSeed>
                <ForcePlanetRadius>
                    (可选)
                    默认值为false
                    如果需要测试半径大于600（或小于50）的行星，请将该项设置为true
                </ForcePlanetRadius>
                <Radius>
                    (可选)
                    默认值为200
                    行星半径，默认的200是正常大小，最大600
                    200、400和600是测试过的，800会崩溃
                </Radius>
                <OrbitalPeriod>
                    (可选)
                    默认值为3600
                    公转周期，单位“秒”
                </OrbitalPeriod>
                <RotationPeriod>
                    (可选)
                    默认值为3600
                    自转周期，单位“秒”
                </RotationPeriod>
                <IsTidalLocked>
                    (可选)
                    默认值为true
                    该项为true时，行星描述会多一串“潮汐锁定 永昼永夜”
                    这个选项应该没有实际影响，毕竟公转周期等于自转周期就已经是实际上的“潮汐锁定”了
                </IsTidalLocked>
                <OrbitInclination>
                    (可选)
                    默认值为0
                    轨道倾角
                </OrbitInclination>
                <Obliquity>
                    (可选)
                    默认值为0
                    地轴倾角
                </Obliquity>
                <DontGenerateVein>
                    (可选)
                    默认值为true
                    是否**不**生成矿脉
                </DontGenerateVein>
                <ThemeId>
                    (可选)
                    无默认值
                    用于指定新行星的主题，下方“可选主题”表格中列出了当前游戏中所有可选的主题，如果不指定该选项将会使用游戏自动生成的主题，
                    这里的选择的行星主题必须和上面的GasGiant选项相匹配，如果设置了<GasGiant>true</GasGiant>，则应该选择“气态行星”或“冰巨星”主题
                </ThemeId>
                <OrbitLongitude>
                    (可选)
                    无默认值
                    轨道升交点经度，格式为“度度,分分”，例如“30,30”（30度30分）
                </OrbitLongitude>
                <VeinCustom>
                    (可选)
                    无默认值
                    在此处添加新的节点可以修改矿物的类型、大小和数量，也可以用于添加在正常的矿脉生成过程中受限不能生成的矿物
                    节点的标签名称必须是游戏内已有的矿物类型，所有可用的矿物类型名称见下表
                    <Iron>
                        对每一种矿物，您可以修改其矿脉生成过程的三个方面：
                            1. VeinGroupCount：生成多少组矿脉
                            2. VeinSpotCount：每组矿脉生成多少个矿点
                            3. VeinAmount：每个矿点含有多少矿物
                        对每个方面，您可以通过若干参数指定数值的生成方式：
                            第一个参数是Type，Type是必要参数，有三个可选值：Accurate、Random和Default
                            如果Type的值为Accurate，那么您还需要设置AccurateValue，意思是生成固定组数的矿脉、固定数量的矿点或固定的矿物含量
                            如果Type的值为Random，可以不设置其他参数（使用默认值），或者手动设置RandomBaseValue、RandomCoef、RandomMulOffset和RandomAddOffset的值（随机数的计算方式见下文）
                            如果Type的值为Default，表示不对该方面进行干预
                        <VeinGroupCount>
                            <Type>Accurate</Type>
                            <AccurateValue>10</AccurateValue>
                        </VeinGroupCount>
                        <VeinSpotCount>
                            <Type>Random</Type>
                            <RandomBaseValue>100000</RandomBaseValue>
                            <RandomCoef>1</RandomCoef>
                            <RandomMulOffset>0</RandomMulOffset>
                            <RandomAddOffset>5</RandomAddOffset>
                        </VeinSpotCount>
                        <VeinAmount>
                            <Type>Default</Type>
                        </VeinAmount>
                    </Iron>
                </VeinCustom>
                <ReplaceAllVeinsTo>
                    (可选)
                    默认值
                    把所有矿脉的类型都改为由该参数指定的类型
                    替换所有矿脉的步骤发生在生成矿脉之后（也就是发生在VeinCustom参数起作用之后）
                    所有可用的矿脉类型见下文
                </ReplaceAllVeinsTo>
            </Planet>
        </Planets>
    </GameNameSpecific>
</Config>
```

#### 可选主题（游戏版本 0.9.26.13034 ）

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
| 20 | 红冰荒漠 | Desert | -2 |  |  | 0.7 | 55 | 0 |  | -5 | 1 |
| 21 | 气态巨星 | Gas | 1 | 氢, 重氢 | 0.84, 0.16 | 0 | 0 | 0 |  | 0 | 0 |
| 22 | 草原荒漠 | Ocean | 0 |  |  | 1.1 | 60 | -0.7 | 水 | 0 | 0 |
| 23 | 橙晶荒漠 | Desert | 0.08 |  |  | 1.5 | 55 | 0 |  | 0 | 0 |
| 24 | 极寒荒漠 | Ice | -4 |  |  | 1.3 | 55 | 0 |  | 0 | 0 |
| 25 | 潘多拉荒漠 | Ocean | 0 |  |  | 1 | 60 | -2 |  | 0 | 0 |

#### 可用矿脉/矿物类型 (game version 0.9.26.13034)

| 矿脉类型 | 给人类阅读的矿脉类型名称 |
| --------- | ------------------- |
| Iron      | 铁                |
| Copper    | 铜              |
| Silicium  | 硅            |
| Titanium  | 钛            |
| Stone     | 石头               |
| Coal      | 煤炭                |
| Oil       | 原油                 |
| Fireice   | 可燃冰             |
| Diamond   | 金伯利矿石          |
| Fractal   | 分形硅石     |
| Crysrub   | 有机晶体     |
| Grat      | 光栅石 |
| Bamboo    | 刺笋结晶 |
| Mag       | 单极磁石 |

#### 随机数的计算方式

```
A = RandomBaseValue * RandomCoef
B = A * RandomMulOffset
C1 = |A - B|
C2 = A + B
D = 在范围 [C1, C2] 中随机选择
E1 = |D - RandomAddOffset|
E2 = D + RandomAddOffset
F = 在范围 [E1, E2] 中随机选择
最终结果是 F
```

#### 关于“number”参数：

![parameter_number.png](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/parameter_number.png "关于“number”参数")

### 其他重要事项

小心处理存档名称，新增的行星是通过“唯一恒星ID”与您的存档绑定的。

### 截图

![screenshot1.jpg](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/screenshot1.jpg "screenshot1")

![screenshot2.jpg](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/screenshot2.jpg "screenshot2")

![screenshot3.jpg](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/screenshot3.jpg "screenshot3")

![screenshot4.jpg](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPAddPlanet/Doc/screenshot4-zh-Hans.jpg "screenshot4")

## 兼容性

### 0.2.1

Build target ：游戏版本 0.9.27.15033 ， BepInEx 版本： 5.4.19 （应该也能够在 5.4.17 版本下正常运行）

### 0.2.0

Build target ：游戏版本 0.9.26.13034 ， BepInEx 版本： 5.4.19 （应该也能够在 5.4.17 版本下正常运行）

### 0.1.1

Build target ：游戏版本 0.9.26.12900 ， BepInEx 版本： 5.4.19 （应该也能够在 5.4.17 版本下正常运行）

### 0.0.1 ~ 0.1.0

Build target ：游戏版本 0.9.25.12201 ， BepInEx 版本： 5.4.19 （应该也能够在 5.4.17 版本下正常运行）

## 更新日志

### 0.2.0 -> 0.2.1

- 兼容游戏版本 0.9.27.15033

- 修复第二次及以后载入存档时在星图界面点击Add Planet按钮失效的问题
- 修复了替换原有行星后，在Add Planet菜单出现重复行星的问题

### 0.1.1 -> 0.2.0

- 兼容游戏版本 0.9.26.13034

- 变更配置文件的结构
- 变更UniqueStarId的格式（应该只有内部影响）

- 添加修改原有行星的功能
- 添加修改出生点的功能

### 0.1.0 -> 0.1.1

- 兼容游戏版本 0.9.26.12900

### 0.0.12 -> 0.1.0

- 将配置文件的类型从纯文本改为XML格式

- 添加自定义矿脉生成的参数
- 将参数`gasGiant`和`GasGiant`改为可选参数，默认为`false`

- 修复了抽水机会把水填上的问题

### 0.0.11 -> 0.0.12

- 修复了在半径125的行星上摆放地基时出现的一些错误
- 修复了在半径大于200的行星上粘贴蓝图时出现不正常的“超出垂直建造高度”的问题

### 0.0.10 -> 0.0.11

- 添加了“orbitLongitude”参数

- 修复了在半径250的行星上摆放地基时出现的一些错误
- 修复了垃圾在某些行星上不受重力影响的问题

### 0.0.9 -> 0.0.10

- 修复了在大行星上无法在正常距离放置分拣器的问题（提示“太近了”）（感谢 GalacticScale ）

### 0.0.8 -> 0.0.9

- 添加`theme`选项，用于指定新行星的主题

### 0.0.7 -> 0.0.8

- 修正行星大气层的渲染半径

### 0.0.6 -> 0.0.7

- 修复当新行星数量太多时运输船无法正常停靠的问题

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
