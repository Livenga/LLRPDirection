using System;

namespace LLRPDirection.UhfRfid {
  /// <summary></summary>
  public interface IUhfReader {
    bool IsConnected { get; }
    bool IsReading { get; }

    event UhfReaderConnectionLostEventHandler? ConnectionLost;

    void Open();
    void Close();

    void Start();
    void Stop();
  }
}
