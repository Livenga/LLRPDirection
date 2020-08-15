using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using LLRPDirection.UhfRfid;


namespace LLRPDirection {
  /// <summary></summary>
  public class Program {
    /// <summary></summary>
    public static void Main(string[] args) {
      string host = string.Empty;
      if(args.Length == 0) {
        return;
      }

      host = args[0];
      Console.Error.WriteLine($"host name: {host}");

      using(ImpinjR660Client xspan = new ImpinjR660Client(host: host)) {
        xspan.Open();

        xspan.Start();
        Console.WriteLine($"Please any key ...");
        Console.ReadLine();

        xspan.Stop();
      }
    }
  }
}
