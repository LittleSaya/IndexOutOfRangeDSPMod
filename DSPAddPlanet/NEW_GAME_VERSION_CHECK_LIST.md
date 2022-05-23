# 游戏版本更新

## 矿脉生成

PlanetAlgorithm 类及其所有子类中 GenerateVeins 函数

主要考察哪些类有 GenerateVeins 方法，哪些类没有，这些需要与 Patch_PlanetAlgorithms 类中的内容保持一致

## 轨道位置

静态对象 StarGen.orbitRadius

影响 orbitIndex 的最大值

## 垃圾系统

TrashSystem 的 Gravity 方法中进行比较然后返回 false 的部分

影响在大半径行星上扔垃圾的表现

## 行星大气层的渲染半径

PlanetSimulator.SetPlanetData() 的代码

## 设置行星主题的逻辑

PlanetGen.SetPlanetTheme() 的代码

与 theme 参数相关的地方要跟着一起改

# mod版本更新

改代码

改2个版本号

改所有的README

在Release模式下build
