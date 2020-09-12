using System;
using System.IO;
using System.Text;


namespace LLRPDirection.Utils {
  /// <summary></summary>
  public static class Log {
    /// <summary></summary>
    public static void WriteLine(Stream stream, string message) {
      string _msg = $"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}: {message}\n";
      byte[] buffer = Encoding.UTF8.GetBytes(s: _msg);
      stream.Write(buffer, 0, buffer.Length);
    }


    /// <summary></summary>
    public static void WriteLine(string path, string message) {
      FileMode mode = File.Exists(path) ? FileMode.Append : FileMode.OpenOrCreate;

      using(FileStream stream = File.Open(path, mode, FileAccess.Write)) {
        WriteLine(stream, message);
      }
    }

    /// <summary></summary>
    public static void WriteLine(string message) {
      string path = $"{DateTime.Now.ToString("yyyy-MM-dd")}.log";
      WriteLine(path, message);
    }


    /// <summary></summary>
    public static void Write(Stream stream, Exception except) {
      WriteLine(stream, $"{except.GetType().Name} [{except.Message}] [{except.StackTrace}]");
    }


    /// <summary></summary>
    public static void Write(string path, Exception except) {
      WriteLine(path, $"{except.GetType().Name} [{except.Message}] [{except.StackTrace}]");
    }

    /// <summary></summary>
    public static void Write(Exception except) {
      WriteLine($"{except.GetType().Name} [{except.Message}] [{except.StackTrace}]");
    }
  }
}
