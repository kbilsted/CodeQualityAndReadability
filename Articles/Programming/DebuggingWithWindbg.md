# Debugging software using windbg
*Author: Kasper B. Graversen*
<br>[[Introduction]](<BaseUrl/>) [[All categories]](<BaseUrl/>AllTags.html) [[All articles]](<BaseUrl/>AllArticles.html) [[Edit article <img src="http://firstclassthoughts.co.uk/img/edit.png"> ]](<GithubPageUrl/>)<br>

<Categories Tags="Debugging">
</Categories>

*Debugging certain kinds of situations calls for stronger tools than what Visual Studio and other graphical environment currently provide. Enter the archaic world of `windbg.exe`.*


Please show your support by sharing and voting:

<SocialShareButtons>
</SocialShareButtons>

<img src="img/excavation.jpg">



Table of Content

 * [1. Debugging live software using windbg](#debugging-live-software-using-windbg)
 * [2. Dumping the process](#dumping-the-process)
 * [3. Starting the windbg](#starting-the-windbg)
 * [4. Getting the call stacks](#getting-the-call-stacks)
 * [5. Tracking the allocated objects](#tracking-the-allocated-objects)
 * [6. Trawling the source code](#trawling-the-source-code)
 * [7. Summary](#summary)
 
 


# 1. Debugging live software using windbg

In this article I will explain my work flow I used last when was trying to understand why my service was not  stopping properly. What I observed on the server was a process using an excessive amount of RAM. The CPU usage was not sky rocketing, but Windows was unable to stop my process (all buttons start, stop, etc. were disabled in the windows services Gui). Oh, by the way, my application is a long-running service programmed in managed C#. 

Having an unresponsive application is particularly annoying. What is wrong? What is it doing? We can try to second guess like there is no tomorrow, and for each wild guess we can turn to the source code imagining all sorts of weird scenarios. Is it a deadlock in the db connection pool, a race condition? What is my application doing??
 
It's time to issue a powerful yet strangely archaic tool called windbg. Among most developers its feared. Certainly it's charm is not found in the user interface - at least not at first sight. It is somewhat the same feeling as having worked in Windows your whole life, and suddenly you are presented with nothing more than a shell to type in. But if you think windbg is weird, rest assured that it is nothing in comparison to the [TECO editor](https://en.wikipedia.org/wiki/TECO_(text_editor))'s macro language. 


You will see some fairly verbose and long-winded lists of data. This is quite intentional. And in fact, I've already edited out some of the noise! Yet I wanted to convey to you, the "atmosphere" when working with windbg and make you understand, that using windbg requires excavation of the output for the useful stuff. Note: My experience with windbg is rather limited, so please use the comment box to state corrections or obvious omissions.



# 2. Dumping the process

Before we start the debugger, we need to dump the memory reserved for our application. This serves as input for the debugger. While the dump is made on the machine running the code, the memory dump can be transferred to a developer machine and debugged completely disconnected from the server. 

Depending on whether the process is a 32 or 64 bit application it needs be dumped differently. On a 64 bit operating system the rules are

* 32 bit applications are dumped with `adplus.exe -hang -pn FooCorp.SpaghettiSort.exe -o c:\Debugging\`
* 64 bit applications can be dumped directly by right-clicking the process in the Windows task manager

If you dump the memory file using the wrong procedure, you will get some weird errors when opening the file in windbg.



# 3. Starting the windbg

* Start `windbg.exe`
* Use the menufile -> Open crash dump

Now we need to load the common language runtime in order for windbg to understand your C# code. We issue the command `.loadby`

<pre>0:000> .loadby sos clr
</pre> 

To ensure that we have loaded the correct version of the clr we issue the command `!threads` and observe that it doesn't error on us. You may sometimes be required to run this command twice in order to see output as below.

<pre>0:000> !threads
ThreadCount:      30
UnstartedThread:  0
BackgroundThread: 5
PendingThread:    0
DeadThread:       24
Hosted Runtime:   no
                                                                         Lock  
       ID OSID ThreadOBJ    State GC Mode     GC Alloc Context  Domain   Count Apt Exception
   0    1 2df8 007b1088     2a020 Preemptive  00000000:00000000 007aa018 0     MTA 
   2    2 3c5c 007bd428     2b220 Preemptive  00000000:00000000 007aa018 0     MTA (Finalizer) 
XXXX    4    0 00821ad0   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
   4    5 401c 00822e70   1029220 Preemptive  4CBC094C:00000000 007aa018 0     MTA (Threadpool Worker) 
XXXX    6    0 00825eb8   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
XXXX    7    0 00855f18   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
   7   10 5e04 04c5ef60   102a220 Preemptive  00000000:00000000 007aa018 0     MTA (Threadpool Worker) 
XXXX   11    0 062f4c38   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
XXXX   12    0 062f5170   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
XXXX    3    0 0631cd98   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
XXXX   26    0 06343878   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
XXXX   36    0 063428a0   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
XXXX    8    0 06343dc0   1039820 Preemptive  00000000:00000000 007aa018 0     Ukn (Threadpool Worker) 
...
</pre>

A lot of threads. The threads marked `XXXX` are dead. The rest are alive. Something is still running in my application it seems...



# 4. Getting the call stacks

For each thread we dump the call stack in order to see where our program is currently executing. Maybe this can pin-point an endless loop or something. with `!clrstack` we dump a managed call stack. Note that potentially the parameter `-p` may be useful (dump with parameters). Since we have many threads we need to do this pr. thread, to do this for each thread we use the `~* e` command.  

<pre>0:000> ~* e !clrstack
OS Thread Id: 0x2df8 (0)
Child SP       IP Call Site
0030f3b4 7773f911 [InlinedCallFrame: 0030f3b4] 
0030f3b0 6e62535a DomainBoundILStubClass.IL_STUB_PInvoke(IntPtr)
0030f3b4 6e626d6a [InlinedCallFrame: 0030f3b4] System.ServiceProcess.NativeMethods.StartServiceCtrlDispatcher(IntPtr)
0030f3e4 6e626d6a System.ServiceProcess.ServiceBase.Run(System.ServiceProcess.ServiceBase[])
0030f420 6e626e8d System.ServiceProcess.ServiceBase.Run(System.ServiceProcess.ServiceBase)
0030f430 001e9e9a Topshelf.Runtime.Windows.WindowsServiceHost.Run()
0030f444 001e03cb Topshelf.HostFactory.Run(System.Action`1<Topshelf.HostConfigurators.HostConfigurator>)
0030f468 001e0094 FooCorp.SpaghettiSort.Program.Main(System.String[])
0030f5e8 739d2552 [GCFrame: 0030f5e8] 
OS Thread Id: 0x5918 (1)
Unable to walk the managed stack. The current thread is likely not a 
managed thread. You can run !threads to get a list of managed threads in
the process
Failed to start stack walk: 80070057
OS Thread Id: 0x3c5c (2)
Child SP       IP Call Site
031ff788 7774019d [DebuggerU2MCatchHandlerFrame: 031ff788] 
Unable to walk the managed stack. The current thread is likely not a 
managed thread. You can run !threads to get a list of managed threads in
the process
Failed to start stack walk: 80070057
OS Thread Id: 0x8698 (9)
Child SP       IP Call Site
0781ebec 7774019d [HelperMethodFrame_1OBJ: 0781ebec] System.Threading.WaitHandle.WaitOneNative(System.Runtime.InteropServices.SafeHandle, UInt32, Boolean, Boolean)
0781ecd0 72b764f0 System.Threading.WaitHandle.InternalWaitOne(System.Runtime.InteropServices.SafeHandle, Int64, Boolean, Boolean)
0781ece8 72b764c4 System.Threading.WaitHandle.WaitOne(Int32, Boolean)
0781ecfc 0060531e Microsoft.FSharp.Control.AsyncImpl+ResultCell`1[[System.__Canon, mscorlib]].TryWaitForResultSynchronously(Microsoft.FSharp.Core.FSharpOption`1<Int32>)
0781ed2c 00604f29 Microsoft.FSharp.Control.FSharpMailboxProcessor`1[[System.__Canon, mscorlib]].TryPostAndReply[[System.__Canon, mscorlib]](Microsoft.FSharp.Core.FSharpFunc`2<Microsoft.FSharp.Control.FSharpAsyncReplyChannel`1<System.__Canon>,System.__Canon>, Microsoft.FSharp.Core.FSharpOption`1<Int32>)
0781eda8 00604d26 Microsoft.FSharp.Control.FSharpMailboxProcessor`1[[System.__Canon, mscorlib]].PostAndReply[[System.__Canon, mscorlib]](Microsoft.FSharp.Core.FSharpFunc`2<Microsoft.FSharp.Control.FSharpAsyncReplyChannel`1<System.__Canon>,System.__Canon>, Microsoft.FSharp.Core.FSharpOption`1<Int32>)
0781edec 00604c42 Okanshi.Monitor.stop(Microsoft.FSharp.Control.FSharpMailboxProcessor`1<Okanshi.MonitorMessage>)
0781ee08 00604ba0 Okanshi.MonitorApi.Stop()
0781ee1c 0059f802 FooCorp.SpaghettiSort.ServiceHost.Stop()
0781ee48 0059f6e8 FooCorp.SpaghettiSort.Program.<Main>b__4(FooCorp.SpaghettiSort.ServiceHost)
0781ee4c 0059f6bb Topshelf.ServiceConfiguratorExtensions+<>c__DisplayClassa`1[[System.__Canon, mscorlib]].<WhenStopped>b__9(System.__Canon, Topshelf.HostControl)
0781ee54 0059f64c Topshelf.Builders.DelegateServiceBuilder`1+DelegateServiceHandle[[System.__Canon, mscorlib]].Stop(Topshelf.HostControl)
0781ee68 0059f538 Topshelf.Runtime.Windows.WindowsServiceHost.OnStop()
0781ee98 6e626a92 System.ServiceProcess.ServiceBase.DeferredStop()
0781f098 739d2552 [HelperMethodFrame_PROTECTOBJ: 0781f098] System.Runtime.Remoting.Messaging.StackBuilderSink._PrivateProcessMessage(IntPtr, System.Object[], System.Object, System.Object[] ByRef)
0781f354 72b513bb System.Runtime.Remoting.Messaging.StackBuilderSink.AsyncProcessMessage(System.Runtime.Remoting.Messaging.IMessage, System.Runtime.Remoting.Messaging.IMessageSink)
0781f3b4 72b511e1 System.Runtime.Remoting.Proxies.AgileAsyncWorkerItem.ThreadPoolCallBack(System.Object)
0781f3c0 72bae356 System.Threading.QueueUserWorkItemCallback.WaitCallback_Context(System.Object)
0781f3c8 72b8da07 System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
0781f434 72b8d956 System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
0781f448 72baf120 System.Threading.QueueUserWorkItemCallback.System.Threading.IThreadPoolWorkItem.ExecuteWorkItem()
0781f45c 72bae929 System.Threading.ThreadPoolWorkQueue.Dispatch()
0781f4ac 72bae7d5 System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()
0781f6d4 739d2552 [DebuggerU2MCatchHandlerFrame: 0781f6d4] 
OS Thread Id: 0x75c4 (10)
Child SP       IP Call Site
GetFrameContext failed: 1
00000000 00000000 <unknown>
OS Thread Id: 0x3fa4 (11)
Unable to walk the managed stack. The current thread is likely not a 
managed thread. You can run !threads to get a list of managed threads in
the process
Failed to start stack walk: 80070057
OS Thread Id: 0x1054 (12)
Unable to walk the managed stack. The current thread is likely not a 
managed thread. You can run !threads to get a list of managed threads in
the process
Failed to start stack walk: 80070057
</pre>


What do we see here. Well, first we ignore all the lines like `Unable to walk the managed stack`. First we see a thread with out `main`.

<pre>0030f468 001e0094 FooCorp.SpaghettiSort.Program.Main(System.String[])
</pre>

Reading from that line and up wards does not reveal anything other than our service is multi threaded.

Further down we see the line

<pre>0781ee1c 0059f802 FooCorp.SpaghettiSort.ServiceHost.Stop()
</pre>

which tells us, that although we are in fact trying to stop our service, but something is blocking us. It seems to be the the Okanshi library (an in-process monitoring of an application). 




# 5. Tracking the allocated objects

OK, we know there were issues with a large memory consumption. With `dumpheap` we can get an aggregated view over the memory allocations.


<pre>0:000>!dumpheap -stat

72c90314      242        14520 System.Reflection.RuntimeConstructorInfo
72c84abc      410        14760 System.Security.PermissionSet
6fd594b8      117        14976 System.Data.SqlClient._SqlMetaData
72891f9c      632        15168 System.Collections.Generic.List`1[[System.Object, mscorlib]]
72c6a008      485        15520 System.Reflection.Emit.SignatureHelper
72c6ca80       15        15600 System.Byte[][]
72c8eda8      332        15936 System.RuntimeMethodInfoStub
095f9ed8     1010        16160 Microsoft.CSharp.RuntimeBinder.Semantics.SYMTBL+Key
095fa330      112        16576 Microsoft.CSharp.RuntimeBinder.Semantics.MethodSymbol
72c89e08      338        18928 System.Reflection.Emit.DynamicMethod
72c8fcb0      228        19152 System.RuntimeType+RuntimeTypeCache
72c8f348      354        19824 System.Reflection.RuntimePropertyInfo
72c877fc      138        21528 System.Collections.Hashtable+bucket[]
000b5bfc     1118        22360 System.SZArrayHelper+SZGenericArrayEnumerator`1[[System.Reflection.ParameterInfo, mscorlib]]
72c841b8     1904        22848 System.Object
72c847d8      470        22914 System.Char[]
0533a2d4      173        26296 Okanshi.Statistics+Statistics
72c7b214      338        32448 System.Reflection.Emit.DynamicILGenerator
00c33528      262        39824 Newtonsoft.Json.Serialization.JsonProperty
72c85000     2815        78820 System.RuntimeType
72c855d4     1165        81360 System.Int32[]
72c90594     1942        85448 System.Signature
72c90878     1450        98600 System.Reflection.RuntimeParameterInfo
72c90a7c     7160       143200 Microsoft.Win32.SafeHandles.SafeTokenHandle
72c8f0d4     3211       192660 System.Reflection.RuntimeMethodInfo
72c86d34     2468       409779 System.Byte[]
72c83e18     8713       488768 System.String
007b3790  1438814     27099636      Free
72c40cbc    10696    268835072 System.Object[]
0505d430 20606535    329704560 Okanshi.MonitorMessage+IncrementSuccess
0505e61c 20606519    494556456 Okanshi.MonitorMessage+Time
Total 42751500 objects
Fragmented blocks larger than 0.5 MB:
    Addr     Size      Followed by
4cbd823c    0.7MB         4cc92d94 System.Byte[]
</pre>


Clearly the culprits are `Okanshi.MonitorMessage+IncrementSuccess` and `Okanshi.MonitorMessage+Time` which take up `329704560` and `494556456` bytes of memory respectively. At the same time we see a very high object count `20606535` and `20606519`.

I can dig deeper into the memory allocations investigating the raw memory of the allocations, eg. the `Char[]` allocations found at `$72c847d8`.


<pre>0:000>!dumpheap -mt 72c847d8
4cb845c0 72c847d8       14
4cb84758 72c847d8       44
4cb8492c 72c847d8       14
4cb849cc 72c847d8       14
4cb84adc 72c847d8       14
4cb84b5c 72c847d8       14
4cb84c94 72c847d8       14
4cb84e20 72c847d8       44
4cb84fd4 72c847d8       14
4cb86000 72c847d8       14
4cb89c10 72c847d8       14
4cb89d80 72c847d8       14
4cb8b764 72c847d8       14
4cb8b8b8 72c847d8       14
4cb8ed9c 72c847d8       14
4cb8ef8c 72c847d8       44
4cb8f0c8 72c847d8       14
4cb8f194 72c847d8       14
4cb8f354 72c847d8       14
...

Statistics:
      MT    Count    TotalSize Class Name
72c847d8      470        22914 System.Char[]
Total 470 objects
Fragmented blocks larger than 0.5 MB:
    Addr     Size      Followed by
4cbd823c    0.7MB         4cc92d94 System.Byte[]
</pre>


Now we have the addresses of the individual allocations which we then can further inspect. But we are not going to do that now.




# 6. Trawling the source code

Windbg has provided hints as to what is wrong with the application. Now it is time to turn to the source code. Specifically, the parts of the code that does performance measurements.

Inspecting the SpaghettiSort algorithm it soon became clear that there were problems with task delay calculations resolving to waiting 0 seconds (as opposed to the expected 60 seconds). After each wait we performance measured an action that didn't really have any data to run on, following by a 0-second wait, and another round of performance measurement.. you get the point. We were logging extreme amounts of performance metrics :-).



# 7. Summary

Using windbg I was able to track down *where* in the code my seemingly unresponsive service was executing, and I was able to figure out the cause of the high memory consumption thanks to hints from the debugger. 


You want to learn more about windbg? There is plenty of stuff is to be found at http://www.windbg.org/




Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>
<CommentText>
</CommentText>

<br><br>


