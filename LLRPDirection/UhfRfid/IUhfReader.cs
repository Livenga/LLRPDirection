using System;

namespace LLRPDirection.UhfRfid {
  /// <summary></summary>
  public interface IUhfReader {
    bool IsConnected { get; }
    bool IsReading { get; }

    void Open();
    void Close();

    void Start();
    void Stop();
  }
}
