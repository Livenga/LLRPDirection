using System;

namespace LLRPDirection.DataType {
  /// <summary></summary>
  public static class UnixDateTime {
    /// <summary></summary>
    public static readonly DateTime BaseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary></summary>
    public static DateTime Convert(ulong ticks) {
      return new DateTime((long)(ticks * 10 + (ulong)BaseDateTime.Ticks), DateTimeKind.Utc);
    }
  }
}
