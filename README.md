# UGCFramework（Unity Game Client Framework）
unity-HybridCLR客户端框架(UGUI+C#+protocol+HybridCLR)

这是一个基于C＃语言的unity客户端框架，小部分工具基于UGUI，大部分工具不限制UI类型</p>
代码热更新方面使用的是HybridCLR热更技术</p>
通信包含http和socket+tcp两种方式，协议采用protocol buffer</p>
因为HybridCLR热更形式不同于ILRuntime，所以依据Hybrid的规则，框架部分修改较多，尤其是加载流程</p>
具体大家可以参照Hybrid官方文档https://hybridclr.doc.code-philosophy.com/docs/intro</p>
后续，除非HybridCLR被证明有无解的严重问题，否则ILRuntime分支将不再更新</p>

框架包括几个部分：</p>
  1、Bundle资源管理系统（包括所有资源更新）</p>
  2、音频管理系统</p>
  3、UI系统（基于UGUI）</p>
  4、对象池管理器</p>
  5、第三方及原生sdk管理器（内含微信登录、分享、支付，QQ登录、分享，支付宝支付，苹果支付，本机号码一键登录，复制等）</p>
  6、提示窗管理器</p>
  7、下载管理器（多线程实现，包含断点续传功能）</p>
  8、渠道管理</p>
  9、通信系统</p>
  10、组件工具（动画、事件系统、定时器等）</p>
  11、实用性工具（图片处理、图片置灰、修改图片色相、文件下载、定位、文件和字符串加密等等）</p>
  12、UGUI扩展组件(Tab、RadioButton、ScrollRect扩展、计时器、文本处理、各种特殊图片组件等)</p>
  13、HybridCLR热更系统</p>
  
## v3.0.0更新日志 - 2023.8.17
  1、切出HybridCLR分支，热更新采用HybridCLR</p>
  2、Json库从LitJson改为NewtonsoftJson
  3、ILRuntime分支unity版本升级到unity2020.3.48
  
## v2.2.0更新日志 - 2021.8.7
  1、优化部分扩展工具</p>
  2、优化事件系统</p>
  3、更新热更新系统，修改ILRuntime接入方式

## v2.1.2更新日志 - 2020.9.14
  1、优化ImageColorChange和ImageColorChangeV，使色相的修改更加平滑，并增加透明度变化下的表现</p>

## v2.1.1更新日志 - 2020.9.10
  1、优化PanelCenterScrollRect和ScrollRectCircle，修复某些情况下循环异常的问题</p>
  2、优化Plugins相关文件夹结构</p>
  3、将第三方SDK文件从package剥离</p>
 
## v2.1.0更新日志 - 2020.9.9
  1、优化ScrollRect相关工具组件</p>
  2、优化渠道管理工具</p>
  3、优化部分ILRuntime相关适配器</p>
  4、修复部分跨平台编译问题</p>
  5、优化UI系统，简化UI结构，修改部分接口的调用方式，修复部分已知问题</p>
  6、优化热更系统，简化热更注册执行方式</p>
  7、优化CommonAnimation组件，修复在某些情况下Foward的改变会导致动画异常的问题</p>
  8、修复示例脚本中的部分已知bug</p>

## v2.0.1更新日志 - 2020.8.28
  1、调整所有脚本行尾设置</p>
  2、优化所有字段、属性的定义(本次修改了部分字段或属性的名称，请谨慎更新)</p>
  3、优化部分编辑器设定</p>
  4、优化了Scroll相关工具组件</p>
  
## v2.0.0更新日志 - 2020.8.13
  1、优化网络框架，优化在网络不稳定情况下的处理，优化网络重连逻辑</p>
  2、增加短链接请求，并实现长、短链接的混合使用</p>
  3、更新UI框架，优化跳转流程</p>
  4、更新工具类，新增部分工具函数</p>
  5、优化通用动画工具，支持多组动画混合运行</p>
  6、优化原生相关逻辑，更新部分三方SDK，新增部分原生功能（苹果登录、本机号码一键登录等）</p>
  7、删除部分无用工具函数</p>
  8、优化框架文件结构，整合拆分示例代码和框架代码，移植更方便</p>
