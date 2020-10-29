# Qi
学习内网穿透技术

思路正确，原先的做废了，底层改用xCode
# 步骤

1、在公网搭建服务端，自己和用户要记住那个域名或者是IP，还有端口号。

2、服务端编程

3、在内网使用客户端，与服务端进行通信。

4、编写客户端



# 要求

## 服务器端

### 配置参数（暂定）

```
    /// <summary>
    /// 服务器端配置项
    /// </summary>
    public class ServiceConfig
    {
        /// <summary>
        /// IP地址，这个IP作用不大，将来应该使用的是域名。
        /// </summary>
        public string ServiceIP { get; set; } = "127.0.0.1";
        /// <summary>
        /// 端口号
        /// </summary>
        public ushort ServicePort { get; set; } = 2020;
        /// <summary>
        /// 服务器最大连接数
        /// </summary>
        public ushort ServiceMaxConnectCount { get; set; } = 30;
        /// <summary>
        /// 服务启动时，是否自动启动监控服务：默认自动
        /// </summary>
        public bool ServiceAutoRun { get; set; } = true;
    }
```

### 流程

1、~~启动端口监听，等待客户端来访~~

2、~~客户端访问，被服务端记录连接IP和端口，上交一把秘钥（作为标识）。同时，客户端也把自身内网的端口告知服务器端，相当于留个电话号码，预备从外网访问。~~

3、~~其他可联网电脑，通过HTTP网页（因为刚学，暂时先把这个作为首要支持功能）访问服务器地址，需要提供秘钥，还有上面提到的电话号码。（因为是内网，也不能让所有人都来看）~~

4、服务器端进行查询，匹配连接的客户端信息，然后进行抓包（暂时我想先这样做），把包数据返回给第3步的电脑，实现打通内网功能。

我知道这里有不好的地方，实力所限，一点一点去做。

感觉好像不对，比如说，一个网站baidu.com，我进去了，进了表单，开始登录，弄两下，就跑到真实网站中了。

## 客户端

## 整体想法

![](image-20201028105315081-1603854494311.png)