using System;

using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;


namespace LLRPDirection.UhfRfid {
  /// <summary></summary>
  public abstract class AbsLLRPReader {

    /// <summary></summary>
    public string Host => this.host;

    /// <summary></summary>
    public int Port => this.port;

    public int Timeout => this.timeout;


    /// <summary></summary>
    public LLRPClient? BaseClient {
      protected set => this.baseClient = value;
      get => this.baseClient;
    }

    private LLRPClient? baseClient = null;

    private readonly int port;
    private readonly string host;
    private readonly int timeout;


    /// <summary></summary>
    public AbsLLRPReader(string host, int port, int timeout) {
      this.port = port;
      this.host = host;
      this.timeout = timeout;
    }


    /// <summary></summary>
    public AbsLLRPReader(string host, int port) : this(host, port, 3000) {
    }

    /// <summary></summary>
    public AbsLLRPReader(string host) : this(host, 5084) {
    }

    /// <summary></summary>
    protected abstract void SetReaderConfig();
    protected abstract void AddROSpec(uint roSpecId);

    /// <summary></summary>
    protected void ResetToFactoryDefault() {
      MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();

      msg.ResetToFactoryDefault = true;

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_SET_READER_CONFIG_RESPONSE? msgResp = this.baseClient?.SET_READER_CONFIG(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.timeout);

      LLRPHelper.CheckLLRPResponse(
          message: msgResp,
          error: msgErr);
    }


    /// <summary></summary>
    protected void EnableROSpec(uint roSpecId) {
      MSG_ENABLE_ROSPEC msg = new MSG_ENABLE_ROSPEC();
      msg.ROSpecID = roSpecId;

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_ENABLE_ROSPEC_RESPONSE? msgResp = this.BaseClient?.ENABLE_ROSPEC(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.timeout);

      LLRPHelper.CheckLLRPResponse(msgResp, msgErr);
    }


    /// <summary></summary>
    protected void DisableROSpec(uint roSpecId = 0) {
      MSG_DISABLE_ROSPEC msg = new MSG_DISABLE_ROSPEC();
      msg.ROSpecID = roSpecId;

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_DISABLE_ROSPEC_RESPONSE? msgResp = this.BaseClient?.DISABLE_ROSPEC(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.timeout);

      LLRPHelper.CheckLLRPResponse(msgResp, msgErr);
    }


    /// <summary></summary>
    protected void StartROSpec(uint roSpecId) {
      MSG_START_ROSPEC msg = new MSG_START_ROSPEC();
      msg.ROSpecID = roSpecId;

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_START_ROSPEC_RESPONSE? msgResp = this.BaseClient?.START_ROSPEC(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.timeout);

      if(msgResp != null) {
        Console.Error.WriteLine($"[Debug] Message ID: {msgResp.MSG_ID}");
      }
      LLRPHelper.CheckLLRPResponse(msgResp, msgErr);
    }


    /// <summary></summary>
    protected void StopROSpec(uint roSpecId) {
      MSG_STOP_ROSPEC msg = new MSG_STOP_ROSPEC();
      msg.ROSpecID = roSpecId;

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_STOP_ROSPEC_RESPONSE? msgResp = this.BaseClient?.STOP_ROSPEC(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.timeout);

      LLRPHelper.CheckLLRPResponse(msgResp, msgErr);
    }


    /// <summary></summary>
    protected void DeleteROSpec(uint roSpecId = 0) {
      MSG_DELETE_ROSPEC msg = new MSG_DELETE_ROSPEC();
      msg.ROSpecID = roSpecId;

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_DELETE_ROSPEC_RESPONSE? msgResp = this.BaseClient?.DELETE_ROSPEC(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.timeout);

      LLRPHelper.CheckLLRPResponse(msgResp, msgErr);
    }
  }
}
