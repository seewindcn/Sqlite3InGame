# Sqlite3InGame
use wxsqlite3's secure sqilte3 in unity3d and cocos2d-x quick

简介
======
sqlite3官方虽然提供了加密相关接口，不过默认的都没有实现数据加密；找到wxsqlite3，提供了加密版的sqlite3。在使用中，碰到一些坑，特建个项目记录下。
注：cocos2d-x用的是lua绑定版v3quick。
##目录说明：
- 
* secure: wxsqlite3中的加密版sqlite3目录；
* u3dsqlite3: unity3d项目例子；
* v3quick: v3quick用到的一些相关代码文件；
* EncodeDB: 对数据库进行加密的工具, u3d项目的可以直接用ChangePassword对数据库进行加密。
可以直接参考目录和例子，直接使用。

准备
=====
-下载wxsqlite3-3.2.1.zip，解压；wxsqlite3-3.2.1\sqlite3\secure\目录就是加密版sqlite3。
只使用aes128，aes256相关代码(sha2.c,sha2.h)、shell.c、sqlite3userauth.h、userauth.c删除。
编译需要添加预定义：
```
SQLITE_USER_AUTHENTICATION=0
```
项目中sqlite3secure.c有两个版本，凌乱，有空再整理；

unity3d
=======
unity3d当前版本5.2，免费版支持使用native插件扩展功能。用Mono.Data.Sqlite来操作sqlite3。由于不同平台使用方式不同（win、android和osx可以用动态库，iOS只能用静态方式），只能讲Mono.Data.Sqlite源码复制过来，根据平台信息进行一些改动。
由于Mono.Data.Sqlite的UnsafeNativeMethods.cs有unsafe代码，需要在Assets目录下添加编译参数文件（gmcs.rsp和smcs.rsp，针对"Api Compatibility Level",一个是.NET 2.0用的，一个是.NET 2.0 Subset用的）。修改UnsafeNativeMethods.cs的代码：
```
#if !SQLITE_STANDARD

#if !USE_INTEROP_DLL

#if !PLATFORM_COMPACTFRAMEWORK
    private const string SQLITE_DLL = "Mono.Data.Sqlite.DLL";
#else
    internal const string SQLITE_DLL = "SQLite.Interop.061.DLL";
#endif // PLATFORM_COMPACTFRAMEWORK

#else
    private const string SQLITE_DLL = "SQLite.Interop.DLL";
#endif // USE_INTEROP_DLL

#elif MONOTOUCH
        private const string SQLITE_DLL = "/usr/lib/libsqlite3.dylib";
#elif UNITY_IOS
          private const string SQLITE_DLL = "__Internal";
#elif UNITY_EDITOR_OSX
          private const string SQLITE_DLL = "sqlite3sec";
#else
          private const string SQLITE_DLL = "sqlite3";
#endif
```
说明：
win,osx,android,ios都需要在u3d的Player Settings中的Scripting Define Symbols中添加：
```
SQLITE_STANDARD
```
就是，win使用sqlite3.dll，osx使用sqlite3sec.bundle，android使用sqlite3.so，ios用静态连接"__Internal"。
unity3d for osx，在启动时应该就已经加载了系统自带的sqlite3.dylib，所有项目没办法让editor使用加密的sqlite3，只能通过改名来加载。
加密和不加密sqlite3的使用，就在于一句代码：
```
conn.SetPassword(password);
```

- win
-- 安装premake4工具，执行：
--premake4 vs2012
生成vs2012的项目，试过生成vs2013出错；vs2012直接打开编译就行，vs2013打开工程后，编译会出错，添加下面编译定义到preprocessor：
```
HAVE_ISBLANK
HAVE_ACOSH
HAVE_ASINH
HAVE_ATANH
```
自己调整项目配置，生成32和64位sqlite3.dll，在win系统下的unity3d使用。
将编译好的dll放到u3d项目的

- android
unity3d for android可以使用加密版的sqlite3.so。编译加密版sqlite3.so，参考secure\sqlitendk\jni目录下的Android.mk文件，Application.mk用于设定编译哪些target；在secure\sqlitendk\目录执行：
```
ndk-build
```
就能自动编译出动态库；

- mac
参考项目中proj.mac\sqlite3.xcodeproj工程，编译出sqlite3.dylib，改名sqlite3sec.bundle并复制到u3d项目的Assets\Plugins\目录，并在u3d中设置其只在osx下使用。

- iOS
复制加密版sqlite3原代码到u3d的Assets\Plugins\iOS\目录，在u3d中讲除了sqlite3secure.c的其它文件的“Select platforms for plugin"项都去掉，就是生成的xcode项目不需要编译其它代码，只编译sqlite3secure.c；sqlite3secure.c需要添加编译选项"Compile flags":
```
-DSQLITE_ENABLE_COLUMN_METADATA
```
在u3d中，生成ios的xcode项目后，还需要手动（或者写脚本）讲Assets\Plugins\iOS\下的其它原代码复制到该xcode项目的Libraries/Plugins/iOS目录下。编译xcode，并真机运行（模拟器不能使用__Internal，好奇怪）。

cocos2d-x v3quick (V3.3)
================
v3quick使用的是lua的sqlite3绑定库lsqlite3，在项目的目录：
```
frameworks\runtime-src\Classes\quick-src\lua_extensions
```
将secure中的代码复制过去。

- win和osx使用player3来运行调试项目，win下player3是使用sqlite3.dll的，将加密版sqlite3.dll和sqlite3.lib替换到：
```
frameworks\cocos2d-x\external\sqlite3\libraries\win32\
```
修改lsqlite3.c代码，添加：
```
#define AES_KEY1 "1234567890123456"
```
改sqlite3_open为sqlite3_open_v2（好像只是设定了自读，如果只是加密并不需要）,并设置密码：
```
    if (sqlite3_open_v2(filename, &db->db,SQLITE_OPEN_READONLY,NULL) == SQLITE_OK) {
    //if (sqlite3_open(filename, &db->db) == SQLITE_OK) {
        /* database handle already in the stack - return it */
        int result = sqlite3_key(db->db,AES_KEY1,strlen(AES_KEY1));
        if(result==SQLITE_OK)
        {
            return 1;
        }        
    }
```
重新编译libcocos2d和player3项目。编译出来的player3，只能访问用指定密码加密的数据库。
osx应该差不多吧。

- android:
修改frameworks\runtime-src\Classes\quick-src\lua_extensions\Android.mk，用sqlite3secure.c替换sqlite3.c，编译就行；好像不需要设置编译预定义；

- iOS:
待续


