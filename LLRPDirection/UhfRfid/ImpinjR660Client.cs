using System;
using System.Data;
using System.Diagnostics;

using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;

using LLRPDirection.DataType;


namespace LLRPDirection.UhfRfid {
  /// <summary></summary>
  public partial class ImpinjR660Client : AbsLLRPReader, IUhfReader, IDisposable {
    /// <summary></summary>
    public bool IsConnected => this.BaseClient?.IsConnected ?? false;
    public bool IsReading => this.isReading;


    private bool isReading = false;


    /// <summary></summary>
    public ImpinjR660Client(string host, int port) : base(host, port, 15000) {
    }


    /// <summary></summary>
    public ImpinjR660Client(string host)
      : base(host: host, port: 5084, timeout: 15000) {
      }


    /// <summary></summary>
    public void Open() {
      if(this.IsConnected) {
        return;
      }

      this.BaseClient = new LLRPClient(port: this.Port);
      this.BaseClient.OnKeepAlive += this.OnLLRPClientKeepalive;
      this.BaseClient.OnRoAccessReportReceived += this.OnLLRPClientRoAccessReportReceived;
      this.BaseClient.OnReaderEventNotification += this.OnLLRPClientReaderEventNotification;


      ENUM_ConnectionAttemptStatusType status = ENUM_ConnectionAttemptStatusType.Success;
      bool isSuccessed = this.BaseClient.Open(
          llrp_reader_name: this.Host,
          status: out status,
          timeout: 5000,
          useTLS: false);

      if(! isSuccessed || status != ENUM_ConnectionAttemptStatusType.Success) {
        throw new Exception($"接続失敗({status}) {isSuccessed}");
      }

      try {

        //this.EnableROSpec(14150);
      } catch(Exception except) {
        this.Dispose();

        throw except;
      }
    }


    /// <summary></summary>
    public void Close() {
      if(! this.IsConnected) {
        return;
      }

      if(this.BaseClient != null) {
        this.BaseClient.OnRoAccessReportReceived -= this.OnLLRPClientRoAccessReportReceived;
        this.BaseClient.OnReaderEventNotification -= this.OnLLRPClientReaderEventNotification;
      }

      this.Stop();


      this.ResetToFactoryDefault();
      this.EnableImpinjExtensions();

      this.BaseClient?.Close();
    }


    /// <summary></summary>
    public void Start() {
      if(this.isReading) {
        return;
      }

      this.isReading = true;

      this.ResetToFactoryDefault();
      this.EnableImpinjExtensions();

      this.SetReaderConfig();
      this.SetAntennaConfig();

      this.AddROSpec(14150);
      this.EnableROSpec(14150);
      this.StartROSpec(14150);
    }


    /// <summary></summary>
    public void Stop() {
      if(! this.isReading) {
        return;
      }

      this.isReading = false;

      this.StopROSpec(14150);
      this.DisableROSpec(14150);
      this.DeleteROSpec(14150);
    }


    /// <summary></summary>
    public void Dispose() {
      try {
        this.Close();
      } catch(Exception) {
      } finally {
        this.BaseClient = null;
      }
    }


    private readonly static string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffffzzz";

    /// <summary></summary>
    private void OnLLRPClientRoAccessReportReceived(MSG_RO_ACCESS_REPORT msg) {
      if(msg.Custom.Length > 0) {
        for(int i = 0; i < msg.Custom.Length; ++i) {
          IParameter custom = msg.Custom[i];

          if(custom is PARAM_ImpinjExtendedTagInformation) {
            PARAM_ImpinjExtendedTagInformation info = (PARAM_ImpinjExtendedTagInformation)custom;
            string epc = info.EPCData[0].EPC.ToHexString();

            if(info.ImpinjDirectionReportData != null) {
              byte firstSeenSectorId = info.ImpinjDirectionReportData.FirstSeenSectorID;
              byte lastSeenSectorId = info.ImpinjDirectionReportData.LastSeenSectorID;

              ENUM_ImpinjDirectionTagPopulationStatus status = info.ImpinjDirectionReportData.TagPopulationStatus;
              ENUM_ImpinjDirectionReportType type = info.ImpinjDirectionReportData.Type;

              DateTime dtFirstSeen = UnixDateTime.Convert(info.ImpinjDirectionReportData.FirstSeenTimestampUTC);
              DateTime dtLastSeen = UnixDateTime.Convert(info.ImpinjDirectionReportData.LastSeenTimestampUTC);

              /*
                 Console.Error.WriteLine($"{epc} Type: {type}, Status: {status}");
                 Console.Error.WriteLine($"  Sector: {firstSeenSectorId} => {lastSeenSectorId}");
                 Console.Error.WriteLine($"{dtFirstSeen.ToString("yyyy-MM-ddTHH:mm:ss")} => {dtLastSeen.ToString("yyyy-MM-ddTHH:mm:ss")}");
                 */

              Console.Error.WriteLine($"{epc},{dtFirstSeen.ToString(DateTimeFormat)},{dtLastSeen.ToString(DateTimeFormat)},{type},{firstSeenSectorId},{lastSeenSectorId},{status}");
            }
          }
        }
      }
    }


    /// <summary></summary>
    private void OnLLRPClientKeepalive(MSG_KEEPALIVE msg) {
#if DEBUG
      Console.Error.WriteLine($"[Debug] {msg.MSG_ID}");
      Console.Error.WriteLine($"{msg.ToString()}");
#endif
    }


    /// <summary></summary>
    private void OnLLRPClientReaderEventNotification(MSG_READER_EVENT_NOTIFICATION msg) {
      var nd = msg.ReaderEventNotificationData;
      Console.Error.WriteLine($"[Debug] {msg.ToString()}");
    }
  }
}
