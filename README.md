
# threading-communication
write c# or cs file to imply variable ways of communication between different threadings.本项目根据《CLR via C#》及《C# in Threading》作为补充建成，如有更正或更新，欢迎PR.


## 线程概念及其开销
线程创建的目的是为了虚拟CPU，比起进程来说，线程的创建更加的轻量，线程可以共享同一进程中的资源，如dll或者打开的文件等。

### 1. 空间开销
线程创建后，会有如下空间的开销

|类型|描述|
|----|----|
|线程内核对象|描述线程的属性，线程上下文|
|线程环境块||
|用户模式栈|含局部变量及调用返回地址|
|内核模式栈|用于在应用程序与内核之间传递参数|
|dll线程连接通知和线程分离通知|每创建或者卸载一个线程，会向非托管dll的DllMain函数传递一个通知|

### 2. 时间开销
线程创建后，会有如下时间的开销

|类型|描述|线程创建后，会有如下空间的开销
|----|----|
|线程上下文切换|用于CPU的寄存器的值存入线程内核对象的线程上下文中，并提取新线程中的上下文到CPU寄存器|
|进程切换|若是切换到不同的进程的线程中，则还需要页表或者段表的切换|
|CLR清理内存|垃圾处理器触发时，会挂起全部线程，遍历根后，再遍历一次以移动新对象的位置|

## CLR线程与window线程的关系
目前是一模一样的

## 创建线程的方法
目前常用的都是使用线程池或者线程池的包装类来隐式使用线程。但可出于如下原因，自己主动使用Thread类创建线程：

1. 要使用前台线程。线程池都是后台线程。
2. 线程要以非普通优先级运行，虽然线程池也可以指定设置优先级，但是线程一回到线程池后优先级便会回到普通优先级
3. 如果某个任务需要长时间运行的话，为了避免线程池会多创建线程，也可以主动创建线程
4. 可能用Thread.Abort方法进行阻止线程

## 线程优先级
首先，优先级有进程优先级类和线程优先级类，先定进程的优先级类，再写进程中的线程的优先级类，最后根据这两个级别得到线程优先级。

## 异步计算
异步不一定是多线程实现，异步跟多线程是一个平等级别的概念，可以使用多线程实现异步，也可以使用单线程实现异步。
对于异步来说，有两种需求需要异步操作。一种是计算限制，一种是I/O限制

### 1. 计算限制的异步操作
使用线程池，Task, parallel

### 2. I/O限制的异步操作
使用线程池， Task, async及await

## 线程同步概要
线程或者进程访问同一代码段时，需要起到保护数据的作用。线程同步可以使用锁和值类型。
在.NET框架中，分类如下

|类型|线程安全|
|----|----|
|静态方法|是|
|实例方法|否|

同时按照难易，用户态和内核态，线程同步可分为如下部分

|分类|类型|描述|缺点|推荐使用|
|----|----|----|----|----|
|基元|基元用户模式|使用特殊CPU指令，在硬件上发生，操作系统检测不到阻塞|在用户态，线程没有办法暂停，所以会造成不断自旋，浪费CPU时间，即活锁|🌟🌟🌟🌟|
|基元|基元内核模式|使用系统调用进行内核，内核发现资源被占用时会主动阻塞该线程，即死锁|用户态切换到内核态造成性能损失|🌟🌟🌟|
|混合||无竞争时，像用户态，有竞争时，像内核态||🌟🌟🌟🌟🌟|

CLR规定，变量读写的原子性，都是32位以下，32位以上会发生撕裂度--一个线程读前面32位，一个线程读后面32位。
对于可原子性读写的变量，却不能规定这些操作什么时候会发生，于是有了————基元用户模式构造，解决int64，double等不能原子性读写的对象及规定好了原子性操作发生的时间。

基元用户模式有两种，易变构造和互锁构造

1. 易变构造

阻塞类型的同步分为如下：

|类型|描述|适合场景|推荐值|
|----|----|----|----|
|阻塞|线程调用join, sleep, wait等函数方法|线程满足条件很快的时候，可以主动阻塞下|🌟|
|排斥锁|使用此lock(Monitor), Mutex, SpinLock，只有一个线程可以进入此代码区|线程均会修改该代码区时|🌟🌟🌟|
|非排它锁|使用此Semaphore, SemaphoreSlim|需要限制进入该代码区的线程数量时|🌟🌟🌟🌟|
|读写锁||不需要限制可读的线程时|🌟🌟🌟🌟|

线程的等待可以分为两种情形，一种是阻塞，一种是自旋。
阻塞的线程会让出运行时间片，而不会消耗CPU，会额外消耗的情形发生在线程被唤醒进行线程切换的情况下。
自旋的线程会浪费CPU的时间，但不会额外消耗线程切换的时间。
两者有时候是单独使用，有时候可以结合使用。在面对等待条件很快能满足的情况下，使用自旋效果会更好，在编写一般等待的代码时候，通常写为

'''
while (condition != true)
    Threading.Sleep(100);
'''

### 排斥锁
Lock在使用的时候，底层使用的是Monitor类型，不过是做了封装