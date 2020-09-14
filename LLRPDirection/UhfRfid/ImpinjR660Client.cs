using System;
using System.Data;
using System.Diagnostics;
using System.Timers;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;
using LLRPDirection.DataType;


namespace LLRPDirection.UhfRfid {
  using ServerTimer = System.Timers.Timer;

  /// <summary></summary>
  public partial class ImpinjR660Client : AbsLLRPReader, IUhfReader, IDisposable {
    //
    private static readonly uint roSpecId = 14150;
    //
    private readonly static string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffffzzz";

    /// <summary></summary>
    public bool IsConnected => this.BaseClient?.IsConnected ?? false;
    /// <summary></summary>
    public bool IsReading => this.isReading;

    public event UhfReaderConnectionLostEventHandler? ConnectionLost = null;


    private readonly ServerTimer intervalTimer;

    private bool isReading = false;
    private DateTime keepalivedAt = DateTime.Now;


    /// <summary></summary>
    public ImpinjR660Client(string host, int port)
      : base(host, port, 15000) {
        this.intervalTimer = new ServerTimer(10000f);
        this.intervalTimer.Elapsed += this.OnIntervalTimerElapsed;
      }


    /// <summary></summary>
    public ImpinjR660Client(string host)
      : this(host: host, port: 5084) {
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
        this.ResetToFactoryDefault();

        this.EnableImpinjExtensions();

        this.SetReaderConfig();
        this.SetAntennaConfig();

        this.AddROSpec(roSpecId);
        this.EnableROSpec(roSpecId);

        this.keepalivedAt = DateTime.Now;
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

      this.DisableROSpec(roSpecId);
      this.DeleteROSpec(roSpecId);

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
      this.StartROSpec(roSpecId);

      this.intervalTimer.Start();
    }


    /// <summary></summary>
    public void Stop() {
      if(! this.isReading) {
        return;
      }

      this.intervalTimer.Stop();
      this.isReading = false;

      this.StopROSpec(roSpecId);
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
#endif

      // Keepalive ACK の送信
      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_KEEPALIVE_ACK msgAck = new MSG_KEEPALIVE_ACK();

      msgAck.MSG_ID = msg.MSG_ID;

      this.BaseClient?.KEEPALIVE_ACK(msgAck, out msgErr, this.Timeout);
#if DEBUG
      if(msgErr != null) {
        Console.Error.WriteLine($"{msgErr.ToString()}");
      }
#endif

      this.keepalivedAt = DateTime.Now;
    }


    /// <summary></summary>
    private void OnLLRPClientReaderEventNotification(MSG_READER_EVENT_NOTIFICATION msg) {
      var nd = msg.ReaderEventNotificationData;
      Console.Error.WriteLine($"[Debug] {msg.ToString()}");
    }


    //
    private void OnIntervalTimerElapsed(object source, ElapsedEventArgs e) {
      double v = (DateTime.Now - this.keepalivedAt).TotalSeconds;

      Console.Error.WriteLine($"Elapsed: {v}");
      if(v >= 60f) {
        this.intervalTimer.Stop();

        this.Dispose();
        this.ConnectionLost?.Invoke(this);
      }
    }
  }
}
