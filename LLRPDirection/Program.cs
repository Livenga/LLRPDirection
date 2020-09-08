using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LLRPDirection.UhfRfid;


namespace LLRPDirection {
  /// <summary></summary>
  public class Program {
    private static EventWaitHandle? eventHandle = null;


    /// <summary></summary>
    public static void Main(string[] args) {
      string host = string.Empty;
      if(args.Length == 0) {
        return;
      }

      host = args[0];
      Console.Error.WriteLine($"host name: {host}");

      eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
      using(ImpinjR660Client xspan = new ImpinjR660Client(host: host)) {
        xspan.ConnectionLost += OnUhfReaderConnectionLost;
        xspan.Open();

        xspan.Start();

        eventHandle.WaitOne();

        xspan.Stop();
      }
    }

    /// <summary></summary>
    private static void OnUhfReaderConnectionLost(IUhfReader source) {
      Console.Error.WriteLine($"# Connection Lost.");
      eventHandle?.Set();
    }
  }
}
