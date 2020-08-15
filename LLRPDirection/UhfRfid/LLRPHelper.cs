using System;
using System.Diagnostics;

using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;


namespace LLRPDirection.UhfRfid {
  /// <summary></summary>
  public static class LLRPHelper {
    /// <summary></summary>
    public static void CheckLLRPResponse(
        Message? message,
        MSG_ERROR_MESSAGE? error) {
      if(message == null && error == null) {
        throw new Exception("timeout");
      }


      PARAM_LLRPStatus? llrpStatus = null;

      // message を優先
      llrpStatus = (PARAM_LLRPStatus?)message?
        .GetType()
        .GetField(name: "LLRPStatus")?
        .GetValue(message);

      if(llrpStatus == null && error != null) {
        llrpStatus = error.LLRPStatus;
      }

      if(llrpStatus == null) {
        throw new InvalidOperationException();
      }

      if(llrpStatus.StatusCode != ENUM_StatusCode.M_Success) {
        throw new LLRPException(
            llrpStatus.StatusCode,
            llrpStatus.ErrorDescription);
      }
    }
  }
}
