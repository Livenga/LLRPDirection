using System;

using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;


namespace LLRPDirection.UhfRfid {
  /// <summary></summary>
  public class LLRPException : Exception {
    /// <summary></summary>
    public ENUM_StatusCode StatusCode => this.statusCode;
    /// <summary></summary>
    public string? Description => this.description;


    private readonly ENUM_StatusCode statusCode;
    private readonly string? description;


    /// <summary></summary>
    public LLRPException(ENUM_StatusCode statusCode, string? description) : base(description ?? string.Empty) {
      this.statusCode = statusCode;
      this.description = description;
    }
  }
}
