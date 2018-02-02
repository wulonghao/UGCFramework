# UGCFramework（Unity Game Client Framework）
unity客户端框架(UGUI+C#+protocol+xlua)

这是一个基于C＃语言的unity客户端框架，少部分工具基于UGUI，大部分工具不限制UI类型
代码热更新方面使用的是xlua热补丁技术
通信采用socket+tcp方式，协议采用protocol buffer

框架包括几个部分：
  1、Bundle资源管理器（包括所有资源更新）
  2、音频管理器
  3、UI管理器
  4、对象池管理器
  5、第三方sdk管理器（第三方登录、分享、支付，复制，电池电量等）
  6、提示窗口管理器
  7、socket+ProtoBuf通信
  8、组件式工具（动画、特效、事件封装等）
  9、UGUI扩展组件(Tab、RadioButton等)
  10、xlua热补丁机制
