using System;
using System.Diagnostics;

using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;


namespace LLRPDirection.UhfRfid {
  /// <summary></summary>
  public partial class ImpinjR660Client {
    /// <summary></summary>
    private void EnableImpinjExtensions() {
      MSG_IMPINJ_ENABLE_EXTENSIONS msg = new MSG_IMPINJ_ENABLE_EXTENSIONS();
      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_CUSTOM_MESSAGE? msgResp = this.BaseClient?.CUSTOM_MESSAGE(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.Timeout);

      LLRPHelper.CheckLLRPResponse(msgResp, msgErr);
    }


    /// <summary></summary>
    protected override void SetReaderConfig() {
      MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();

      // Keepalive
      PARAM_KeepaliveSpec pKeepalive = new PARAM_KeepaliveSpec();
      msg.KeepaliveSpec = pKeepalive;

      pKeepalive.KeepaliveTriggerType = ENUM_KeepaliveTriggerType.Periodic;
      //pKeepalive.PeriodicTriggerValue = 15000;
      pKeepalive.PeriodicTriggerValue = 30000;

      // Link monitor
      PARAM_ImpinjLinkMonitorConfiguration pLinkMonitor = new PARAM_ImpinjLinkMonitorConfiguration();
      pLinkMonitor.LinkMonitorMode = ENUM_ImpinjLinkMonitorMode.Enabled;
      pLinkMonitor.LinkDownThreshold = 4;

      msg.Custom.Add(pLinkMonitor);

      // Event notification
      PARAM_ReaderEventNotificationSpec pNotificationSpec = new PARAM_ReaderEventNotificationSpec();
      msg.ReaderEventNotificationSpec = pNotificationSpec;

      pNotificationSpec.EventNotificationState = new PARAM_EventNotificationState[1];

      PARAM_EventNotificationState pROSpecState = new PARAM_EventNotificationState();
      pNotificationSpec.EventNotificationState[0] = pROSpecState;

      pROSpecState.NotificationState = true;
      pROSpecState.EventType = ENUM_NotificationEventType.ROSpec_Event;

      //Console.Error.WriteLine($"{msg.ToString()}");

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_SET_READER_CONFIG_RESPONSE? msgResp = this.BaseClient?.SET_READER_CONFIG(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.Timeout);

      LLRPHelper.CheckLLRPResponse(msgResp, msgErr);
    }


    /// <summary></summary>
    private void SetAntennaConfig() {
      MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();
      msg.ResetToFactoryDefault = false;

      msg.AntennaConfiguration = new PARAM_AntennaConfiguration[1];

      // Antenna Configuration
      PARAM_AntennaConfiguration pAntennaConfig = new PARAM_AntennaConfiguration();
      msg.AntennaConfiguration[0] = pAntennaConfig;

      pAntennaConfig.AntennaID = 0;
      pAntennaConfig.AirProtocolInventoryCommandSettings = new UNION_AirProtocolInventoryCommandSettings();

      // C1G2 Inventory Command
      PARAM_C1G2InventoryCommand pInventoryCommand = new PARAM_C1G2InventoryCommand();
      pAntennaConfig.AirProtocolInventoryCommandSettings.Add(pInventoryCommand);

      pInventoryCommand.TagInventoryStateAware = false;

      PARAM_ImpinjFixedFrequencyList pFixedFreqList = new PARAM_ImpinjFixedFrequencyList();
      pInventoryCommand.Custom.Add(pFixedFreqList);

      pFixedFreqList.FixedFrequencyMode = ENUM_ImpinjFixedFrequencyMode.Disabled;
      pFixedFreqList.ChannelList = new UInt16Array();


      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_SET_READER_CONFIG_RESPONSE? msgResp = this.BaseClient?.SET_READER_CONFIG(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.Timeout);
    }


    /// <summary></summary>
    protected override void AddROSpec(uint roSpecId) {
      MSG_ADD_ROSPEC msg = new MSG_ADD_ROSPEC();

      // ROSpec
      PARAM_ROSpec pROSpec = new PARAM_ROSpec();
      msg.ROSpec = pROSpec;

      pROSpec.ROSpecID = roSpecId;
      pROSpec.Priority = 0;
      pROSpec.CurrentState = ENUM_ROSpecState.Disabled;


      // Boundary Spec
      PARAM_ROBoundarySpec pBoundary = new PARAM_ROBoundarySpec();
      pROSpec.ROBoundarySpec = pBoundary;

      // Start Trigger
      pBoundary.ROSpecStartTrigger = new PARAM_ROSpecStartTrigger();
      //pBoundary.ROSpecStartTrigger.ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Immediate;
      pBoundary.ROSpecStartTrigger.ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Null;

      // Stop Trigger
      pBoundary.ROSpecStopTrigger = new PARAM_ROSpecStopTrigger();
      pBoundary.ROSpecStopTrigger.DurationTriggerValue = 0;
      pBoundary.ROSpecStopTrigger.ROSpecStopTriggerType = ENUM_ROSpecStopTriggerType.Null;

      // Report Spec
      PARAM_ROReportSpec pReport = new PARAM_ROReportSpec();
      pROSpec.ROReportSpec = pReport;

      pReport.N = 1;
      pReport.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec;

      // Tag report content selector
      PARAM_TagReportContentSelector pTagReportContentSelector = new PARAM_TagReportContentSelector();
      pReport.TagReportContentSelector = pTagReportContentSelector;

      pTagReportContentSelector.AirProtocolEPCMemorySelector = new UNION_AirProtocolEPCMemorySelector();
      PARAM_C1G2EPCMemorySelector pEpcMemorySelector = new PARAM_C1G2EPCMemorySelector();
      pTagReportContentSelector.AirProtocolEPCMemorySelector.Add(pEpcMemorySelector);

      pEpcMemorySelector.EnableCRC = false;
      pEpcMemorySelector.EnablePCBits = false;

      pTagReportContentSelector.EnableROSpecID = false;
      pTagReportContentSelector.EnableSpecIndex = false;
      pTagReportContentSelector.EnableInventoryParameterSpecID = false;
      pTagReportContentSelector.EnableAntennaID = true;
      pTagReportContentSelector.EnablePeakRSSI = true;
      pTagReportContentSelector.EnableFirstSeenTimestamp = true;
      pTagReportContentSelector.EnableLastSeenTimestamp = false;
      pTagReportContentSelector.EnableTagSeenCount = false;
      pTagReportContentSelector.EnableAccessSpecID = false;


      pROSpec.SpecParameter = new UNION_SpecParameter();

      // Impinj DI spec
      PARAM_ImpinjDISpec pDI = new PARAM_ImpinjDISpec();
      pROSpec.SpecParameter.Add(pDI);

      // Impinj Direction Sectors
      PARAM_ImpinjDirectionSectors pDirectionSectors = new PARAM_ImpinjDirectionSectors();
      pDI.ImpinjDirectionSectors = pDirectionSectors;
      pDirectionSectors.EnabledSectorIDs = new UInt16Array();
      pDirectionSectors.EnabledSectorIDs.Add(2);
      pDirectionSectors.EnabledSectorIDs.Add(3);

      // Direction Config
      PARAM_ImpinjDirectionConfig pDirectionConfig = new PARAM_ImpinjDirectionConfig();
      pDI.ImpinjDirectionConfig = pDirectionConfig;

      pDirectionConfig.TagAgeIntervalSeconds = 0x02;
      pDirectionConfig.UpdateIntervalSeconds = 0x02;
      pDirectionConfig.FieldOfView = ENUM_ImpinjDirectionFieldOfView.ReaderSelected;

      PARAM_ImpinjDirectionUserTagPopulationLimit pDITagPopulationLimit = new PARAM_ImpinjDirectionUserTagPopulationLimit();
      pDirectionConfig.ImpinjDirectionUserTagPopulationLimit = pDITagPopulationLimit;
      pDITagPopulationLimit.UserTagPopulationLimit = 20; // 0x14


      // Impinj  C1G2 Direction Config
      PARAM_ImpinjC1G2DirectionConfig pC1G2DirectionConfig = new PARAM_ImpinjC1G2DirectionConfig();
      pDI.ImpinjC1G2DirectionConfig = pC1G2DirectionConfig;
      pC1G2DirectionConfig.RFMode = ENUM_ImpinjDirectionRFMode.HighPerformance;

      // 出力電力
      PARAM_ImpinjTransmitPower pTransmitPower = new PARAM_ImpinjTransmitPower();
      pC1G2DirectionConfig.ImpinjTransmitPower = pTransmitPower;
      pTransmitPower.TransmitPower = 81;
      //pTransmitPower.TransmitPower = 21;

      PARAM_ImpinjDirectionReporting pDirectionReporting = new PARAM_ImpinjDirectionReporting();
      pDI.ImpinjDirectionReporting = pDirectionReporting;
      pDirectionReporting.DiagnosticReportLevel = 0;
      pDirectionReporting.EnableEntryReport = true;
      pDirectionReporting.EnableExitReport = true;
      pDirectionReporting.EnableUpdateReport = true;
      pDirectionReporting.EnableDiagnosticReport = false;

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_ADD_ROSPEC_RESPONSE? msgResp = this.BaseClient?.ADD_ROSPEC(
          msg: msg,
          msg_err: out msgErr,
          time_out: this.Timeout);

      LLRPHelper.CheckLLRPResponse(msgResp, msgErr);
    }
  }
}
