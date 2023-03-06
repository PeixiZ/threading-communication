using System.Threading;
using static System.Console;
using System.Runtime.Remoting.Messaging;
class P
{
    static void RunByT(object mess)
    {
        WriteLine($"mess is {(string)mess}");
    }

    public static void Main()
    {
        ThreadPool.QueueUserWorkItem(RunByT, "state");
        Thread.Sleep(3);
        CallContext.LogicalSetData("name", "my");
        ThreadPool.QueueUserWorkItem(state => {
            WriteLine($"in {Thread.CurrentThread.ManagedThreadId}, state is {CallContext.LogicalGetData("name")}");
        });
        Thread.Sleep(3);
        ExecutionContext.SuppressFlow();
        ThreadPool.QueueUserWorkItem(state =>{
            WriteLine($"in {Thread.CurrentThread.ManagedThreadId}, state is {CallContext.LogicalGetData("name")}");
        });
        new Thread(() => {
            WriteLine($"in {Thread.CurrentThread.ManagedThreadId}, state is {CallContext.LogicalGetData("name")}");
        }).Start();
        ExecutionContext.RestoreFlow();
        ThreadPool.QueueUserWorkItem(state =>{
            WriteLine($"in {Thread.CurrentThread.ManagedThreadId}, state is {CallContext.LogicalGetData("name")}");
        });
        new Thread(() => {
            WriteLine($"in {Thread.CurrentThread.ManagedThreadId}, state is {CallContext.LogicalGetData("name")}");
        }).Start();
        Thread.Sleep(3);
    }
}