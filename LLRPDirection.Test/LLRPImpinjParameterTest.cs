using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;


namespace LLRPDirection.Test {
  /// <summary></summary>
  public class LLRPImpinjParameterTest {
    private readonly ITestOutputHelper outputHelper;
    private readonly Assembly[] assemblies;

    /// <summary></summary>
    public LLRPImpinjParameterTest(ITestOutputHelper outputHelper) {
      Regex regex = new Regex("^LLRP.*$", RegexOptions.Compiled);

      this.outputHelper = outputHelper;

      this.assemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Where(asm => regex.IsMatch(input: asm.GetName().Name))
        .Where(asm => asm != Assembly.GetExecutingAssembly())
        .ToArray();
    }


    /// <summary></summary>
    private string ToHexString(ValueType value) {
      switch(value) {
        case Byte  b: return string.Format("0x{0:X4}", b);
        case SByte b: return string.Format("0x{0:X4}", b);

        case Int16 i: return string.Format("0x{0:X4}", i);
        case UInt16 u: return string.Format("0x{0:X4}", u);

        case Int32 i: return string.Format("0x{0:X4}", i);
        case UInt32 u: return string.Format("0x{0:X4}", u);
      }

      return string.Empty;
    }

    [Fact]
    public void GetOctaneLLRP() {
      Type[] types = this.assemblies
        .SelectMany(asm => asm.GetTypes())
        .Where(type => type.GetProperties().Any(p => p.Name == "VendorID"))
        .ToArray();

      foreach(Type type in types) {
        try {
          this.outputHelper.WriteLine($"{type.FullName}");
          object? o = type.GetConstructor(types: Type.EmptyTypes)?.Invoke(null);

          if(o != null) {
            object? vendorId = type.GetProperty(name: "VendorID")?.GetValue(o);
            object? subType = type.GetProperty(name: "SubType")?.GetValue(o);
            object? typeId = type.GetProperty(name: "TypeID")?.GetValue(o);


            if(vendorId != null && vendorId is ValueType) {
              this.outputHelper.WriteLine($"  VendorId : {vendorId} ({ToHexString((ValueType)vendorId)})");
            }

            if(subType != null && subType is ValueType) {
              this.outputHelper.WriteLine($"  SubType  : {subType.ToString()} ({ToHexString((ValueType)subType)})");
            }

            if(typeId != null && typeId is ValueType) {
              this.outputHelper.WriteLine($"  TypeId   : {typeId.ToString()} ({ToHexString((ValueType)typeId)})");
            }
          }
        } catch(Exception except) {
          this.outputHelper.WriteLine($"{except.GetType().Name} [{except.Message}] [{except.StackTrace}]");
        } finally {
          this.outputHelper.WriteLine("");
        }
      }
    }
  }
}
